using linerider.Game;
using linerider.IO.json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace linerider.IO
{
    public static class JSONLoader
    {
        public static Track LoadTrack(string trackfile)
        {
            Track ret = new Track
            {
                Filename = trackfile
            };
            track_json trackobj;

            using (StreamReader reader = new StreamReader(trackfile))
            {
                try
                {
                    var json = reader.ReadToEnd();
                    trackobj = JsonConvert.DeserializeObject<track_json>(json);
                }
                catch (Exception ex)
                {
                    throw new TrackIO.TrackLoadException(
                        "Json parsing error.\n" + ex.Message);
                }
            }
            switch (trackobj.version)
            {
                case "6.1":
                    ret.SetVersion(61);
                    break;
                case "6.2":
                    ret.SetVersion(62);
                    break;
                default:
                    throw new TrackIO.TrackLoadException(
                        "Unsupported physics.");
            }
            ret.Name = trackobj.label;
            if (trackobj.startZoom != 0)
            {
                ret.StartZoom = (float)MathHelper.Clamp(
                    trackobj.startZoom,
                    Utils.Constants.MinimumZoom,
                    Utils.Constants.MaxZoom);
            }

            ret.YGravity = trackobj.yGravity;
            ret.XGravity = trackobj.xGravity;
            ret.GravityWellSize = trackobj.gravityWellSize;
            ret.BGColorR = trackobj.bgR;
            ret.BGColorG = trackobj.bgG;
            ret.BGColorB = trackobj.bgB;

            ret.LineColorR = trackobj.lineR;
            ret.LineColorG = trackobj.lineG;
            ret.LineColorB = trackobj.lineB;

            ret.ZeroStart = trackobj.zeroStart;
            if (trackobj.startPosition != null)
            {
                ret.StartOffset = new Vector2d(
                    trackobj.startPosition.x, trackobj.startPosition.y);
            }
            if (!string.IsNullOrEmpty(trackobj.linesArrayCompressed))
            {
                string json2 = LZString.DecompressBase64(trackobj.linesArrayCompressed);
                trackobj.linesArray = JsonConvert.DeserializeObject<object[][]>(json2);
                trackobj.linesArrayCompressed = null;
            }
            if (trackobj.linesArray?.Length > 0)
            {
                ReadLinesArray(ret, trackobj.linesArray);
            }
            else if (trackobj.lines != null)
            {
                foreach (line_json line in trackobj.lines)
                {
                    AddLine(ret, line);
                }
            }
            if (trackobj.triggers != null)
            {
                List<LineTrigger> linetriggers = new List<LineTrigger>();
                foreach (track_json.zoomtrigger_json trigger in trackobj.triggers)
                {
                    if (ret.LineLookup.TryGetValue(trigger.ID, out GameLine line))
                    {
                        if (line is StandardLine)
                        {
                            if (trigger.zoom)
                            {
                                linetriggers.Add(new LineTrigger()
                                {
                                    ZoomTrigger = trigger.zoom,
                                    ZoomTarget = (float)MathHelper.Clamp(
                                        trigger.target,
                                        Utils.Constants.MinimumZoom,
                                        Utils.Constants.MaxZoom),
                                    ZoomFrames = trigger.frames,
                                    LineID = trigger.ID
                                });
                            }
                        }
                    }
                }
                ret.Triggers = TriggerConverter.ConvertTriggers(linetriggers, ret);

            }
            if (trackobj.gameTriggers != null)
            {
                foreach (track_json.gametrigger_json t in trackobj.gameTriggers)
                {
                    if (t.start < 1 || t.end < 1 || t.end < t.start)
                        throw new TrackIO.TrackLoadException(
                            "Trigger timing was outside of range");
                    TriggerType ttype;
                    try
                    {
                        ttype = (TriggerType)t.triggerType;
                    }
                    catch
                    {
                        throw new TrackIO.TrackLoadException(
                            "Unsupported trigger type " + t.triggerType);
                    }
                    switch (ttype)
                    {
                        case TriggerType.Zoom:
                            ret.Triggers.Add(new GameTrigger()
                            {
                                Start = t.start,
                                End = t.end,
                                TriggerType = ttype,
                                ZoomTarget = t.zoomTarget,
                            });
                            break;
                        case TriggerType.BGChange:
                            ret.Triggers.Add(new GameTrigger()
                            {
                                Start = t.start,
                                End = t.end,
                                TriggerType = ttype,
                                backgroundRed = t.backgroundred,
                                backgroundGreen = t.backgroundgreen,
                                backgroundBlue = t.backgroundblue,
                            });
                            break;
                        case TriggerType.LineColor:
                            ret.Triggers.Add(new GameTrigger()
                            {
                                Start = t.start,
                                End = t.end,
                                TriggerType = ttype,
                                lineRed = t.lineRed,
                                lineGreen = t.lineGreen,
                                lineBlue = t.lineBlue,
                            });
                            break;
                    }
                }
            }
            return ret;
        }
        private static void ReadLinesArray(Track track, object[][] parser)
        {
            // ['type', 'id', 'x1', 'y1', 'x2', 'y2', 'extended', 'flipped', 'leftLine', 'rightLine', 'multiplier']
            // Ignore leftLine, rightLine
            foreach (object[] lineobj in parser)
            {
                line_json line = new line_json();
                int idx = 0;
                line.type = Convert.ToInt32(lineobj[idx++]);
                line.id = Convert.ToInt32(lineobj[idx++]);
                line.x1 = Convert.ToDouble(lineobj[idx++]);
                line.y1 = Convert.ToDouble(lineobj[idx++]);
                line.x2 = Convert.ToDouble(lineobj[idx++]);
                line.y2 = Convert.ToDouble(lineobj[idx++]);
                int sz = lineobj.Length;
                if (line.type != 2 && idx < sz) // Non scenery
                {
                    line.extended = Convert.ToInt32(lineobj[idx++]);
                    if (idx < sz)
                    {
                        line.flipped = Convert.ToBoolean(lineobj[idx++]);
                        idx += 2; // Skip leftline, rightline
                        if (line.type == 1 && idx < sz)
                        {
                            line.multiplier = Convert.ToDouble(lineobj[idx++], Program.Culture);
                        }
                    }
                }
                AddLine(track, line);
            }
        }
        private static void AddLine(Track track, line_json line)
        {
            switch (line.type)
            {
                case 0:
                {
                    StandardLine add = new StandardLine(
                            new Vector2d(line.x1, line.y1),
                            new Vector2d(line.x2, line.y2),
                            Convert.ToBoolean(line.flipped))
                    {
                        ID = line.id,
                        Extension = (StandardLine.Ext)line.extended
                    };
                    if (Convert.ToBoolean(line.leftExtended))
                        add.Extension |= StandardLine.Ext.Left;
                    if (Convert.ToBoolean(line.rightExtended))
                        add.Extension |= StandardLine.Ext.Right;
                    track.AddLine(add);
                    break;
                }
                case 1:
                {
                    RedLine add = new RedLine(
                            new Vector2d(line.x1, line.y1),
                            new Vector2d(line.x2, line.y2),
                            Convert.ToBoolean(line.flipped))
                    {
                        ID = line.id,
                        Extension = (StandardLine.Ext)line.extended
                    };
                    if (Convert.ToBoolean(line.leftExtended))
                        add.Extension |= StandardLine.Ext.Left;
                    if (Convert.ToBoolean(line.rightExtended))
                        add.Extension |= StandardLine.Ext.Right;
                    if (line.multiplier > 1)
                    {
                        add.Multiplier = line.multiplier;
                    }
                    track.AddLine(add);
                    break;
                }
                case 2:
                {
                    SceneryLine add = new SceneryLine(
                            new Vector2d(line.x1, line.y1),
                            new Vector2d(line.x2, line.y2))
                    {
                        ID = GameLine.UninitializedID
                    };
                    track.AddLine(add);
                    break;
                }
                default:
                    throw new TrackIO.TrackLoadException(
                        "Unknown line type");
            }
        }
    }
}
