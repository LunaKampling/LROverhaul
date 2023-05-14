using OpenTK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using linerider.Audio;
using linerider.Game;
using System.Drawing;

namespace linerider.IO
{
    public static class TRKWriter
    {
        public static string SaveTrack(Track trk, string savename)
        {
            var dir = TrackIO.GetTrackDirectory(trk);
            if (trk.Name.Equals("<untitled>")) { dir = Utils.Constants.TracksDirectory + "Unnamed Track" + Path.DirectorySeparatorChar; }
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var filename = dir + savename + ".trk";
            using (var file = File.Create(filename))
            {
                var bw = new BinaryWriter(file);
                bw.Write(new byte[] { (byte)'T', (byte)'R', (byte)'K', 0xF2 }); //TRK
                bw.Write((byte)1);
                string featurestring = "";
                var lines = trk.GetLines();
                var featurelist = TrackIO.GetTrackFeatures(trk);
                featurelist.TryGetValue(TrackFeatures.songinfo, out bool songinfo);
                featurelist.TryGetValue(TrackFeatures.redmultiplier, out bool redmultiplier);
                featurelist.TryGetValue(TrackFeatures.zerostart, out bool zerostart);
                featurelist.TryGetValue(TrackFeatures.scenerywidth, out bool scenerywidth);
                featurelist.TryGetValue(TrackFeatures.six_one, out bool six_one);
                featurelist.TryGetValue(TrackFeatures.ignorable_trigger, out bool ignorable_trigger);
                featurelist.TryGetValue(TrackFeatures.remount, out bool remount);
                featurelist.TryGetValue(TrackFeatures.frictionless, out bool frictionless);
                foreach (var feature in featurelist)
                {
                    if (feature.Value)
                    {
                        featurestring += feature.Key + ";";
                    }
                }
                WriteString(bw, featurestring);
                if (songinfo)
                {
                    // unfotrunately this relies on .net to save and parse in
                    // its own way, and we're kind of stuck with it instead of
                    // the right way to write strings
                    bw.Write(trk.Song.ToString());
                }
                bw.Write(trk.StartOffset.X);
                bw.Write(trk.StartOffset.Y);
                bw.Write(lines.Length);
                foreach (var line in lines)
                {
                    byte type = (byte)line.Type;
                    if (line is StandardLine l)
                    {
                        if (l.inv)
                            type |= 1 << 7;
                        var ext = (byte)l.Extension;
                        type |= (byte)((ext & 0x03) << 5); //bits: 2
                        bw.Write(type);
                        if (redmultiplier)
                        {
                            if (line is RedLine red)
                            {
                                bw.Write((byte)red.Multiplier);
                            }
                        }
                        if (ignorable_trigger)
                        {
                            if (l.Trigger != null)
                            {
                                if (l.Trigger.ZoomTrigger) // check other triggers here for at least one
                                {
                                    bw.Write(l.Trigger.ZoomTrigger);
                                    if (l.Trigger.ZoomTrigger)
                                    {
                                        bw.Write(l.Trigger.ZoomTarget);
                                        bw.Write((short)l.Trigger.ZoomFrames);
                                    }
                                }
                                else
                                {
                                    bw.Write(false);
                                }
                            }
                            else
                            {
                                bw.Write(false);//zoomtrigger=false
                            }
                        }
                        bw.Write(l.ID);
                        if (l.Extension != StandardLine.Ext.None)
                        {
                            // this was extension writing
                            // but we no longer support this.
                            bw.Write(-1);
                            bw.Write(-1);
                        }
                    }
                    else
                    {
                        bw.Write(type);
                        if (scenerywidth)
                        {
                            if (line is SceneryLine scenery)
                            {
                                byte b = (byte)(Math.Round(scenery.Width, 1) * 10);
                                bw.Write(b);
                            }
                        }
                    }

                    bw.Write(line.Position1.X);
                    bw.Write(line.Position1.Y);
                    bw.Write(line.Position2.X);
                    bw.Write(line.Position2.Y);
                }
                bw.Write(new byte[] { (byte)'M', (byte)'E', (byte)'T', (byte)'A' });
                List<string> metadata = new List<string>();
                metadata.Add(TrackMetadata.startzoom + "=" + trk.StartZoom.ToString(Program.Culture));

                //Only add if the values are different from default
                if (trk.YGravity != 1)
                {
                    metadata.Add(TrackMetadata.ygravity + "=" + trk.YGravity.ToString(Program.Culture));
                }
                if (trk.XGravity != 0)
                {
                    metadata.Add(TrackMetadata.xgravity + "=" + trk.XGravity.ToString(Program.Culture));
                }
                if (trk.GravityWellSize != 10)
                {
                    metadata.Add(TrackMetadata.gravitywellsize + "=" + trk.GravityWellSize.ToString(Program.Culture));
                }
                
                if (trk.BGColorR != Color.FromArgb(Utils.Constants.ColorOffwhite.ToArgb()).R) 
                {
                    metadata.Add(TrackMetadata.bgcolorR + "=" + trk.BGColorR.ToString(Program.Culture));
                }
                if (trk.BGColorG != Color.FromArgb(Utils.Constants.ColorOffwhite.ToArgb()).G)
                {
                    metadata.Add(TrackMetadata.bgcolorG + "=" + trk.BGColorG.ToString(Program.Culture));
                }
                if (trk.BGColorB != Color.FromArgb(Utils.Constants.ColorOffwhite.ToArgb()).B)
                {
                    metadata.Add(TrackMetadata.bgcolorB + "=" + trk.BGColorB.ToString(Program.Culture));
                }
                
                if (trk.LineColorR != Settings.Lines.DefaultLine.R)
                {
                    metadata.Add(TrackMetadata.linecolorR + "=" + trk.LineColorR.ToString(Program.Culture));
                }
                if (trk.LineColorG != Settings.Lines.DefaultLine.G)
                {
                    metadata.Add(TrackMetadata.linecolorG + "=" + trk.LineColorG.ToString(Program.Culture));
                }
                if (trk.LineColorB != Settings.Lines.DefaultLine.B)
                {
                    metadata.Add(TrackMetadata.linecolorB + "=" + trk.LineColorB.ToString(Program.Culture));
                }

                StringBuilder triggerstring = new StringBuilder();
                for (int i = 0; i < trk.Triggers.Count; i++)
                {
                    GameTrigger t = trk.Triggers[i];
                    if (i != 0) { triggerstring.Append("&"); }
                    switch (t.TriggerType)
                    {
                        case TriggerType.Zoom:
                            triggerstring.Append((int)TriggerType.Zoom);
                            triggerstring.Append(":");
                            triggerstring.Append(t.ZoomTarget.ToString(Program.Culture));
                            triggerstring.Append(":");
                            break;
                        case TriggerType.BGChange:
                            triggerstring.Append((int)TriggerType.BGChange);
                            triggerstring.Append(":");
                            triggerstring.Append(t.backgroundRed.ToString(Program.Culture));
                            triggerstring.Append(":");
                            triggerstring.Append(t.backgroundGreen.ToString(Program.Culture));
                            triggerstring.Append(":");
                            triggerstring.Append(t.backgroundBlue.ToString(Program.Culture));
                            triggerstring.Append(":");
                            break;
                        case TriggerType.LineColor:
                            triggerstring.Append((int)TriggerType.LineColor);
                            triggerstring.Append(":");
                            triggerstring.Append(t.lineRed.ToString(Program.Culture));
                            triggerstring.Append(":");
                            triggerstring.Append(t.lineGreen.ToString(Program.Culture));
                            triggerstring.Append(":");
                            triggerstring.Append(t.lineBlue.ToString(Program.Culture));
                            triggerstring.Append(":");
                            break;
                    }
                    triggerstring.Append(t.Start.ToString(Program.Culture));
                    triggerstring.Append(":");
                    triggerstring.Append(t.End.ToString(Program.Culture));
                }
                if (trk.Triggers.Count > 0) //If here are not trigger don't add triggers entry
                {
                    metadata.Add(TrackMetadata.triggers + "=" + triggerstring.ToString());
                }
                bw.Write((short)metadata.Count);
                foreach (var str in metadata)
                {
                    WriteString(bw, str);
                }
            }
            return filename;
        }
        private static void WriteString(BinaryWriter bw, string str)
        {
            bw.Write((short)str.Length);
            bw.Write(Encoding.ASCII.GetBytes(str));
        }
    }
}
