//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using linerider.Utils;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace linerider.Audio
{
    public class AudioService : GameService
    {
        private static string _currentsong = null;
        private static AudioDevice _device;
        private static AudioStreamer _musicplayer;
        private static readonly Stopwatch _audiosync_timer = Stopwatch.StartNew();

        private static Dictionary<string, AudioSource> _sfx;

        public static float SongDuration => _musicplayer == null ? 0 : _musicplayer.Duration;

        public static void EnsureInitialized()
        {
            if (_device == null)
            {
                _device = new AudioDevice();
                _musicplayer = new AudioStreamer();
                _sfx = new Dictionary<string, AudioSource>
                {
                    ["beep"] = new AudioSource(new NVorbis.VorbisReader(new MemoryStream(GameResources.beep), true))
                };
            }
        }
        private static void PlayClip(AudioSource audio)
        {
            int alsource = AL.GenSource();
            audio.Position = 0;
            List<int> bufs = new List<int>();
            try
            {
                while (true)
                {
                    int len = audio.ReadBuffer();
                    if (len > 0)
                    {
                        int bufferid = AL.GenBuffer();
                        AudioDevice.Check();
                        bufs.Add(bufferid);
                        AL.BufferData(bufferid, audio.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, audio.Buffer, len * sizeof(short), audio.SampleRate);
                        AL.SourceQueueBuffer(alsource, bufferid);
                        AudioDevice.Check();
                    }
                    if (len != audio.Buffer.Length)
                        break;
                }
                AL.SourcePlay(alsource);
            }
            catch
            {
                // Not that big of a deal...
            }
            finally
            {
                new Thread(() =>
                {
                    while (AL.GetSourceState(alsource) == ALSourceState.Playing)
                        Thread.Sleep(1);
                    AL.DeleteSource(alsource);
                    if (bufs.Count != 0)
                        AL.DeleteBuffers(bufs.ToArray());
                })
                { IsBackground = true }.Start();
            }
        }
        public static void Beep()
        {
            EnsureInitialized();
            PlayClip(_sfx["beep"]);
        }

        public static bool LoadFile(ref string file)
        {
            EnsureInitialized();
            if (_musicplayer != null)
            {
                Stop();
            }
            if (_currentsong == file)
            {
                return true;
            }
            game.Title = Program.WindowTitle;
            try
            {
                file = GetOgg(file);
                if (file == null)
                    return false;
                if (File.Exists(file))
                {
                    _musicplayer.LoadSoundStream(new AudioSource(new NVorbis.VorbisReader(file)));
                    _currentsong = file;
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Program.NonFatalError("Unable to load song file " + e);
                return false;
            }
        }

        public static void Pause() => _musicplayer?.Pause();

        public static void Resume(float seconds, float rate) => _musicplayer?.Play(seconds, rate);

        public static void Stop() => _musicplayer?.Pause();

        public static void CloseDevice()
        {
            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }
        }

        private static string GetOgg(string file)
        {
            bool hardexit = false;
            TimeSpan duration = TimeSpan.Zero;
            file = IO.ffmpeg.FFMPEG.ConvertSongToOgg(file, (string obj) =>
            {
                int idx = obj.IndexOf("Duration: ", StringComparison.InvariantCulture);
                if (idx != -1)
                {
                    idx += "Duration: ".Length;
                    string length = obj.Substring(idx, obj.IndexOf(",", idx, StringComparison.InvariantCulture) - idx);
                    TimeSpan ts = TimeSpan.Parse(length);
                    duration = ts;
                }
                idx = obj.IndexOf("time=", StringComparison.InvariantCulture);
                if (idx != -1)
                {
                    idx += "time=".Length;
                    string length = obj.Substring(idx, obj.IndexOf(" ", idx, StringComparison.InvariantCulture) - idx);
                    TimeSpan ts = TimeSpan.Parse(length);
                    game.Title = Program.WindowTitle + string.Format(" [Converting song | {0:P}% | Hold ESC to cancel]", ts.TotalSeconds / duration.TotalSeconds);// "[" + (ts.TotalSeconds / duration.TotalSeconds) + "% converting song]";
                }

                if (Keyboard.GetState()[Key.Escape])
                {
                    hardexit = true;
                    return false;
                }
                return true;
            });
            game.Title = Program.WindowTitle;
            return hardexit ? null : file;
        }
        public static void EnsureSync()
        {
            Editor editor = game.Track;
            if (Settings.MuteAudio ||
                string.IsNullOrEmpty(editor.Song.Location) ||
                !editor.Song.Enabled)
                return;

            float updaterate = editor.Scheduler.UpdatesPerSecond;
            float updatepercent = (float)editor.Scheduler.ElapsedPercent;
            float expectedtime = editor.Song.Offset +
                editor.Offset / (float)Constants.PhysicsRate +
                updatepercent / Constants.PhysicsRate;

            bool shouldplay =
                _musicplayer != null &&
                editor.Playing &&
                expectedtime < _musicplayer.Duration;

            if (shouldplay && !game.Canvas.Scrubbing)
            {
                float rate = updaterate / Constants.PhysicsRate;
                if (rate > 2)
                    Pause();
                else
                {
                    if (game.ReversePlayback)
                        rate = -rate;
                    if (_musicplayer.Speed != rate || !_musicplayer.Playing)
                    {
                        Resume(
                            expectedtime,
                            rate);
                    }
                    else if (_musicplayer.Playing)
                    {
                        double sp = _musicplayer.SongPosition;
                        double syncrate = Math.Abs(expectedtime - sp);
                        if (syncrate > 0.1)
                        {
                            Resume(
                                expectedtime,
                                rate);
                            _audiosync_timer.Restart();
                            Debug.WriteLine(
                                "Audio fell out of sync by " +
                                syncrate +
                                " seconds");
                        }
                    }
                }
            }
            else
            {
                Pause();
            }
        }
    }
}