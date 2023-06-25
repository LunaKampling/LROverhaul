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

using linerider.Game;
using linerider.IO.json;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utf8Json;

namespace linerider.IO
{
    public class TrackIO : GameService
    {
        public class TrackLoadException : Exception
        {
            public TrackLoadException(string message) : base(message)
            {
            }
        }

        public static string[] EnumerateTrackFiles(string folder) => Directory.GetFiles(folder, "*.*")
                .Where(x =>
                    x.EndsWith(".trk", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).
                    OrderByDescending(x =>
                    {
                        string fn = Path.GetFileName(x);
                        int index = fn.IndexOf(" ", StringComparison.Ordinal);
                        if (index != -1)
                        {
                            if (int.TryParse(fn.Remove(index), out int pt))
                                return pt;
                        }
                        return 0;
                    }).ToArray();
        public static string[] EnumerateSolFiles(string folder)
        {
            string[] ret = Directory.GetFiles(folder, "*.*")
                .Where(x =>
                    x.EndsWith(".sol", StringComparison.OrdinalIgnoreCase)).ToArray();
            Array.Sort(ret, StringComparer.CurrentCultureIgnoreCase);
            return ret;
        }
        public static Dictionary<string, bool> GetTrackFeatures(Track trk)
        {
            Dictionary<string, bool> ret = new Dictionary<string, bool>();
            if (trk.ZeroStart)
            {
                ret[TrackFeatures.zerostart] = true;
            }
            if (trk.frictionless)
            {
                ret[TrackFeatures.frictionless] = true;
            }
            foreach (GameLine l in trk.LineLookup.Values)
            {
                if (l is SceneryLine scenery)
                {
                    if (Math.Abs(scenery.Width - 1) > 0.0001f)
                    {
                        ret[TrackFeatures.scenerywidth] = true;
                    }
                }
                if (l is RedLine red)
                {
                    if (red.Multiplier != 1)
                    {
                        ret[TrackFeatures.redmultiplier] = true;
                    }
                }
                if (l is StandardLine stl)
                {
                    if (stl.Trigger != null)
                    {
                        ret[TrackFeatures.ignorable_trigger] = true;
                    }
                }
            }

            if (trk.GetVersion() == 61)
                ret[TrackFeatures.six_one] = true;
            if (!string.IsNullOrEmpty(trk.Song.Location) && trk.Song.Enabled)
            {
                ret[TrackFeatures.songinfo] = true;
            }

            if (trk.Remount)
            {
                ret[TrackFeatures.remount] = true;
            }

            return ret;
        }
        /// Checks a relative filename for validity
        public static bool CheckValidFilename(string relativefilename)
        {
            if (relativefilename.Length == 0)
                return false;

            char[] invalidchars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < relativefilename.Length; i++)
            {
                if (invalidchars.Contains(relativefilename[i]))
                {
                    return false;
                }
            }

            if (Path.GetFileName(relativefilename) != relativefilename ||
                Path.IsPathRooted(relativefilename))
                return false;

            try
            {
                // The ctor checks validity pretty well.
                // It also does not have the requirement of the file existing.
                FileInfo info = new FileInfo(relativefilename);
                FileAttributes attr = info.Attributes;
                if (attr != (FileAttributes)(-1) &&
                attr.HasFlag(FileAttributes.Directory))
                    throw new ArgumentException();

            }
            catch
            {
                return false;
            }

            return true;
        }
        public static string GetTrackName(string trkfile)
        {
            string trackname = Path.GetFileNameWithoutExtension(trkfile);
            string dirname = Path.GetDirectoryName(trkfile);
            string[] dirs = Directory.GetDirectories(Constants.TracksDirectory);
            foreach (string dir in dirs)
            {
                if (string.Equals(
                    dirname,
                    dir,
                    StringComparison.InvariantCulture))
                {
                    trackname = Path.GetFileName(dirname);
                    break;
                }
            }
            return trackname;
        }
        public static string GetTrackDirectory(Track track) => Constants.TracksDirectory +
            track.Name +
            Path.DirectorySeparatorChar;
        public static string ExtractSaveName(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            int index = filename.IndexOf(" ", StringComparison.Ordinal);
            if (index != -1)
            {
                string id = filename.Remove(index);
                if (int.TryParse(id, out _))
                {
                    filename = filename.Remove(0, index + 1);
                }
            }
            int ext = filename.IndexOf(".trk", StringComparison.OrdinalIgnoreCase);
            if (ext != -1)
            {
                filename = filename.Remove(ext);
            }
            return filename;
        }
        public static bool QuickSave(Track track)
        {
            if (track.Name == Constants.DefaultTrackName)
                return false;
            string dir = GetTrackDirectory(track);
            if (track.Filename != null)
            {
                // If we loaded this file from /Tracks and not 
                // /Tracks/{trackname}/file.trk then it doesnt have a folder
                // the user will have to decide one. we will not quicksave it.
                if (!track.Filename.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
                    return false;

            }
            if (Directory.Exists(dir))
            {
                string quicksaveString = "quicksave_" + DateTime.Now.Month + "." + DateTime.Now.Day + "." + DateTime.Now.Year + "_" + DateTime.Now.Hour + "." + DateTime.Now.Minute;
                try
                {
                    switch (Settings.DefaultQuicksaveFormat)
                    {
                        case ".trk": //.trk
                            _ = SaveTrackToFile(track, quicksaveString);
                            break;
                        case ".json": //.json
                            _ = SaveTrackToJsonFile(track, quicksaveString);
                            break;
                        case ".sol": //.sol
                            _ = SaveToSOL(track, quicksaveString);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Program.NonFatalError("An error occured during quicksave" +
                    Environment.NewLine +
                    e.Message);
                }
                return true;
            }
            return false;
        }
        public static string SaveToSOL(Track track, string savename)
        {
            int saveindex = GetSaveIndex(track);
            string filename = SOLWriter.SaveTrack(track, saveindex + " " + savename);
            track.Filename = filename;
            return filename;
        }
        public static void ExportTrackData(Track track, string fn, int frames, ICamera camera)
        {
            Timeline timeline = new Timeline(
                track);
            timeline.Restart(track.GetStart(), 1);
            _ = timeline.GetFrame(frames);
            camera.SetTimeline(timeline);
            List<RiderData> data = new List<RiderData>();
            for (int idx = 0; idx < frames; idx++)
            {
                Rider frame = timeline.GetFrame(idx);
                RiderData framedata = new RiderData
                {
                    Frame = idx,
                    Points = new List<track_json.point_json>()
                };
                for (int i = 0; i < frame.Body.Length; i++)
                {
                    SimulationPoint point = frame.Body[i];
                    framedata.Points.Add(new track_json.point_json()
                    {
                        x = point.Location.X,
                        y = point.Location.Y
                    });
                    OpenTK.Vector2d camframe = camera.GetFrameCamera(idx);
                    framedata.CameraCenter = new track_json.point_json()
                    {
                        x = camframe.X,
                        y = camframe.Y
                    };
                }
                data.Add(framedata);
            }

            using (FileStream file = File.Create(fn))
            {
                byte[] bytes = JsonSerializer.Serialize(data);
                file.Write(bytes, 0, bytes.Length);
            }
        }
        public static string SaveTrackToFile(Track track, string savename)
        {
            int saveindex = GetSaveIndex(track);
            string filename = TRKWriter.SaveTrack(track, saveindex + " " + savename);
            track.Filename = filename;
            return filename;
        }
        public static string SaveTrackToJsonFile(Track track, string savename)
        {
            int saveindex = GetSaveIndex(track);
            string filename = JSONWriter.SaveTrack(track, saveindex + " " + savename);
            track.Filename = filename;
            return filename;
        }
        private static bool TryMoveAndReplaceFile(string fname, string fname2)
        {
            if (File.Exists(fname))
            {
                try
                {
                    if (File.Exists(fname2))
                    {
                        File.Delete(fname2);
                    }
                    File.Move(fname, fname2);
                    return true;
                }
                catch
                {
                    //failed
                }
            }
            return false;
        }
        public static void CreateAutosave(Track track)
        {
            string dir = GetTrackDirectory(track);
            if (track.Name.Equals("*") || track.Name.Equals("<untitled>"))
            {
                dir = Constants.TracksDirectory + "Unnamed Track" + Path.DirectorySeparatorChar;
            }
            if (!Directory.Exists(dir))
                _ = Directory.CreateDirectory(dir);

            string autosaveString = "Autosave " + DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss");
            switch (Settings.DefaultAutosaveFormat)
            {
                case ".trk":
                    _ = TRKWriter.SaveTrack(track, autosaveString);
                    break;
                case ".json":
                    _ = JSONWriter.SaveTrack(track, autosaveString);
                    break;
                case ".sol":
                    _ = SOLWriter.SaveTrack(track, autosaveString);
                    break;
            }
        }
        public static void CreateTestFromTrack(Track track)
        {
            Timeline timeline = new Timeline(
                track);
            timeline.Restart(track.GetStart(), 1);
            int framecount = 40 * 60 * 5;

            string filename = TRKWriter.SaveTrack(track, track.Name + ".test");
            if (File.Exists(filename + ".result"))
                File.Delete(filename + ".result");
            using (FileStream f = File.Create(filename + ".result"))
            {
                BinaryWriter bw = new BinaryWriter(f);
                bw.Write(framecount);
                Rider state = timeline.GetFrame(framecount);
                for (int i = 0; i < state.Body.Length; i++)
                {
                    bw.Write(state.Body[i].Location.X);
                    bw.Write(state.Body[i].Location.Y);
                }
            }
        }
        public static bool TestCompare(Track track, string dir)
        {
            string testfile = dir + track.Name + ".test.trk.result";
            if (!File.Exists(testfile))
            {
                return false;
            }
            using (FileStream file =
                    File.Open(testfile, FileMode.Open))
            {
                BinaryReader br = new BinaryReader(file);
                int frame = br.ReadInt32();
                Timeline timeline = new Timeline(
                    track);
                timeline.Restart(track.GetStart(), 1);
                //track.Chunks.fg.PrintMetrics();
                Rider state = timeline.GetFrame(frame);
                for (int i = 0; i < state.Body.Length; i++)
                {
                    double x = br.ReadDouble();
                    double y = br.ReadDouble();
                    double riderx = state.Body[i].Location.X;
                    double ridery = state.Body[i].Location.Y;
                    if (x != riderx || y != ridery)
                        return false;
                }
            }
            return true;
        }

        private static int GetSaveIndex(Track track)
        {
            string dir = GetTrackDirectory(track);

            if (track.Name.Equals("<untitled>"))
            {
                dir = Constants.TracksDirectory + "Unnamed Track" + Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(dir))
            {
                _ = Directory.CreateDirectory(dir);
            }
            string[] trackfiles =
                EnumerateTrackFiles(dir);
            int saveindex = 0;
            for (int i = 0; i < trackfiles.Length; i++)
            {
                string s = Path.GetFileNameWithoutExtension(trackfiles[i]);
                int index = s.IndexOf(" ", StringComparison.Ordinal);
                if (index != -1)
                {
                    s = s.Remove(index);
                }
                if (int.TryParse(s, out saveindex))
                {
                    break;
                }
            }
            saveindex++;
            return saveindex;
        }
    }
}