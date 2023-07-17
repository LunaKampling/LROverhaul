using linerider.Game;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace linerider.IO
{
    public static class TRKWriter
    {
        public static string SaveTrack(Track trk, string savename)
        {
            string dir = TrackIO.GetTrackDirectory(trk);
            if (trk.Name.Equals(Constants.InternalDefaultTrackName))
                dir = Path.Combine(Settings.Local.UserDirPath, Constants.TracksFolderName, Constants.DefaultTrackName);
            if (!Directory.Exists(dir))
                _ = Directory.CreateDirectory(dir);
            string filename = Path.Combine(dir, savename + ".trk");
            using (FileStream file = File.Create(filename))
            {
                BinaryWriter bw = new BinaryWriter(file);
                bw.Write(new byte[] { (byte)'T', (byte)'R', (byte)'K', 0xF2 }); //TRK
                bw.Write((byte)1);
                string featurestring = "";
                GameLine[] lines = trk.GetLines();
                Dictionary<string, bool> featurelist = TrackIO.GetTrackFeatures(trk);
                _ = featurelist.TryGetValue(TrackFeatures.songinfo, out bool songinfo);
                _ = featurelist.TryGetValue(TrackFeatures.redmultiplier, out bool redmultiplier);
                _ = featurelist.TryGetValue(TrackFeatures.zerostart, out bool zerostart);
                _ = featurelist.TryGetValue(TrackFeatures.scenerywidth, out bool scenerywidth);
                _ = featurelist.TryGetValue(TrackFeatures.six_one, out bool six_one);
                _ = featurelist.TryGetValue(TrackFeatures.ignorable_trigger, out bool ignorable_trigger);
                _ = featurelist.TryGetValue(TrackFeatures.remount, out bool remount);
                _ = featurelist.TryGetValue(TrackFeatures.frictionless, out bool frictionless);
                foreach (KeyValuePair<string, bool> feature in featurelist)
                {
                    if (feature.Value)
                    {
                        featurestring += feature.Key + ";";
                    }
                }
                WriteString(bw, featurestring);
                if (songinfo)
                {
                    // Unfotrunately this relies on .net to save and parse in
                    // its own way, and we're kind of stuck with it instead of
                    // the right way to write strings
                    bw.Write(trk.Song.ToString());
                }
                bw.Write(trk.StartOffset.X);
                bw.Write(trk.StartOffset.Y);
                bw.Write(lines.Length);
                foreach (GameLine line in lines)
                {
                    byte type = (byte)line.Type;
                    if (line is StandardLine l)
                    {
                        if (l.inv)
                            type |= 1 << 7;
                        byte ext = (byte)l.Extension;
                        type |= (byte)((ext & 0x03) << 5); // Bits: 2
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
                                if (l.Trigger.ZoomTrigger) // Check other triggers here for at least one
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
                            // This was extension writing
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
                List<string> metadata = new List<string>
                {
                    TrackMetadata.startzoom + "=" + trk.StartZoom.ToString(Program.Culture)
                };

                // Only add if the values are different from default
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
                if (!trk.HasDefaultBackground)
                {
                    metadata.Add(TrackMetadata.bgcolorR + "=" + trk.BGColorR.ToString(Program.Culture));
                    metadata.Add(TrackMetadata.bgcolorG + "=" + trk.BGColorG.ToString(Program.Culture));
                    metadata.Add(TrackMetadata.bgcolorB + "=" + trk.BGColorB.ToString(Program.Culture));
                }
                if (!trk.HasDefaultLineColor)
                {
                    metadata.Add(TrackMetadata.linecolorR + "=" + trk.LineColorR.ToString(Program.Culture));
                    metadata.Add(TrackMetadata.linecolorG + "=" + trk.LineColorG.ToString(Program.Culture));
                    metadata.Add(TrackMetadata.linecolorB + "=" + trk.LineColorB.ToString(Program.Culture));
                }

                StringBuilder triggerstring = new StringBuilder();
                for (int i = 0; i < trk.Triggers.Count; i++)
                {
                    GameTrigger t = trk.Triggers[i];
                    if (i != 0)
                    {
                        _ = triggerstring.Append("&");
                    }
                    switch (t.TriggerType)
                    {
                        case TriggerType.Zoom:
                            _ = triggerstring.Append((int)TriggerType.Zoom);
                            _ = triggerstring.Append(":");
                            _ = triggerstring.Append(t.ZoomTarget.ToString(Program.Culture));
                            _ = triggerstring.Append(":");
                            break;
                        case TriggerType.BGChange:
                            _ = triggerstring.Append((int)TriggerType.BGChange);
                            _ = triggerstring.Append(":");
                            _ = triggerstring.Append(t.backgroundRed.ToString(Program.Culture));
                            _ = triggerstring.Append(":");
                            _ = triggerstring.Append(t.backgroundGreen.ToString(Program.Culture));
                            _ = triggerstring.Append(":");
                            _ = triggerstring.Append(t.backgroundBlue.ToString(Program.Culture));
                            _ = triggerstring.Append(":");
                            break;
                        case TriggerType.LineColor:
                            _ = triggerstring.Append((int)TriggerType.LineColor);
                            _ = triggerstring.Append(":");
                            _ = triggerstring.Append(t.lineRed.ToString(Program.Culture));
                            _ = triggerstring.Append(":");
                            _ = triggerstring.Append(t.lineGreen.ToString(Program.Culture));
                            _ = triggerstring.Append(":");
                            _ = triggerstring.Append(t.lineBlue.ToString(Program.Culture));
                            _ = triggerstring.Append(":");
                            break;
                    }
                    _ = triggerstring.Append(t.Start.ToString(Program.Culture));
                    _ = triggerstring.Append(":");
                    _ = triggerstring.Append(t.End.ToString(Program.Culture));
                }
                if (trk.Triggers.Count > 0) // If here are not trigger don't add triggers entry
                {
                    metadata.Add(TrackMetadata.triggers + "=" + triggerstring.ToString());
                }
                bw.Write((short)metadata.Count);
                foreach (string str in metadata)
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
