using linerider.Audio;
using linerider.Game;
using linerider.Utils;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace linerider.IO
{
    public static class TRKLoader
    {
        private static readonly string[] supported_features = [
    "REDMULTIPLIER",
    "SCENERYWIDTH",
    "6.1","SONGINFO",
    "IGNORABLE_TRIGGER",
    "ZEROSTART",
        ];

        //private const int REDMULTIPLIER_INDEX = 0;
        //private const int SCENERYWIDTH_INDEX = 1;
        //private const int SIX_ONE_INDEX = 2;
        //private const int SONGINFO_INDEX = 3;
        //private const int IGNORABLE_TRIGGER_INDEX = 4;
        //private const int ZEROSTART_INDEX = 5;
        private static float ParseFloat(string f) => !float.TryParse(
                f,
                NumberStyles.Float,
                Program.Culture,
                out float ret)
                ? throw new TrackIO.TrackLoadException(
                    "Unable to parse string into float")
                : ret;
        private static double ParseDouble(string f) => !double.TryParse(
                f,
                NumberStyles.Float,
                Program.Culture,
                out double ret)
                ? throw new TrackIO.TrackLoadException(
                    "Unable to parse string into double")
                : ret;
        private static int ParseInt(string f) => !int.TryParse(
                f,
                NumberStyles.Float,
                Program.Culture,
                out int ret)
                ? throw new TrackIO.TrackLoadException(
                    "Unable to parse string into int")
                : ret;
        private static void ParseMetadata(Track ret, BinaryReader br)
        {
            short count = br.ReadInt16();
            for (int i = 0; i < count; i++)
            {
                string[] metadata = ReadString(br).Split('=');
                switch (metadata[0])
                {
                    case TrackMetadata.startzoom:
                        ret.StartZoom = ParseFloat(metadata[1]);
                        break;
                    case TrackMetadata.ygravity:
                        ret.YGravity = ParseFloat(metadata[1]);
                        break;
                    case TrackMetadata.xgravity:
                        ret.XGravity = ParseFloat(metadata[1]);
                        break;
                    case TrackMetadata.gravitywellsize:
                        ret.GravityWellSize = ParseDouble(metadata[1]);
                        break;
                    case TrackMetadata.bgcolorR:
                        ret.BGColorR = ParseInt(metadata[1]);
                        break;
                    case TrackMetadata.bgcolorG:
                        ret.BGColorG = ParseInt(metadata[1]);
                        break;
                    case TrackMetadata.bgcolorB:
                        ret.BGColorB = ParseInt(metadata[1]);
                        break;
                    case TrackMetadata.linecolorR:
                        ret.LineColorR = ParseInt(metadata[1]);
                        break;
                    case TrackMetadata.linecolorG:
                        ret.LineColorG = ParseInt(metadata[1]);
                        break;
                    case TrackMetadata.linecolorB:
                        ret.LineColorB = ParseInt(metadata[1]);
                        break;
                    case TrackMetadata.triggers:
                        string[] triggers = metadata[1].Split('&');
                        foreach (string t in triggers)
                        {
                            string[] tdata = t.Split(':');
                            TriggerType ttype;
                            try
                            {
                                ttype = (TriggerType)int.Parse(tdata[0]);
                            }
                            catch
                            {
                                throw new TrackIO.TrackLoadException(
                                    "Unsupported trigger type");
                            }
                            GameTrigger newtrigger;
                            int start;
                            int end;
                            switch (ttype)
                            {
                                case TriggerType.Zoom:
                                    float target = ParseFloat(tdata[1]);
                                    start = ParseInt(tdata[2]);
                                    end = ParseInt(tdata[3]);
                                    newtrigger = new GameTrigger()
                                    {
                                        Start = start,
                                        End = end,
                                        TriggerType = TriggerType.Zoom,
                                        ZoomTarget = target,
                                    };
                                    break;
                                case TriggerType.BGChange:
                                    int red = ParseInt(tdata[1]);
                                    int green = ParseInt(tdata[2]);
                                    int blue = ParseInt(tdata[3]);
                                    start = ParseInt(tdata[4]);
                                    end = ParseInt(tdata[5]);
                                    newtrigger = new GameTrigger()
                                    {
                                        Start = start,
                                        End = end,
                                        TriggerType = TriggerType.BGChange,
                                        backgroundRed = red,
                                        backgroundGreen = green,
                                        backgroundBlue = blue,
                                    };
                                    break;
                                case TriggerType.LineColor:
                                    int linered = ParseInt(tdata[1]);
                                    int linegreen = ParseInt(tdata[2]);
                                    int lineblue = ParseInt(tdata[3]);
                                    start = ParseInt(tdata[4]);
                                    end = ParseInt(tdata[5]);
                                    newtrigger = new GameTrigger()
                                    {
                                        Start = start,
                                        End = end,
                                        TriggerType = TriggerType.LineColor,
                                        lineRed = linered,
                                        lineGreen = linegreen,
                                        lineBlue = lineblue,
                                    };
                                    break;
                                default:
                                    throw new TrackIO.TrackLoadException(
                                        "Unsupported trigger type");
                            }
                            ret.Triggers.Add(newtrigger);
                        }
                        break;
                }
            }
        }
        public static Track LoadTrack(string trackfile, string trackname)
        {
            Track ret = new()
            {
                Filename = trackfile,
                Name = trackname,
                Remount = false
            };
            Dictionary<int, StandardLine> addedlines = [];
            string location = trackfile;
            byte[] bytes = File.ReadAllBytes(location);
            using (MemoryStream file =
                    new(bytes))
            {
                BinaryReader br = new(file);
                int magic = br.ReadInt32();
                if (magic != ('T' | 'R' << 8 | 'K' << 16 | 0xF2 << 24))
                    throw new TrackIO.TrackLoadException("File was read as .trk but it is not valid");
                byte version = br.ReadByte();
                string[] features = ReadString(br).Split([';'], StringSplitOptions.RemoveEmptyEntries);
                if (version != 1)
                    throw new TrackIO.TrackLoadException("Unsupported version");
                bool redmultipier = false;
                bool scenerywidth = false;
                bool supports61 = false;
                bool songinfo = false;
                bool ignorabletrigger = false;
                for (int i = 0; i < features.Length; i++)
                {
                    switch (features[i])
                    {
                        case TrackFeatures.redmultiplier:
                            redmultipier = true;
                            break;

                        case TrackFeatures.scenerywidth:
                            scenerywidth = true;
                            break;

                        case TrackFeatures.six_one:
                            supports61 = true;
                            break;

                        case TrackFeatures.songinfo:
                            songinfo = true;
                            break;

                        case TrackFeatures.ignorable_trigger:
                            ignorabletrigger = true;
                            break;

                        case TrackFeatures.zerostart:
                            ret.ZeroStart = true;
                            break;

                        case TrackFeatures.remount:
                            ret.Remount = true;
                            break;

                        case TrackFeatures.frictionless:
                            ret.frictionless = true;
                            break;

                        default:
                            throw new TrackIO.TrackLoadException("Unsupported feature");
                    }
                }
                if (supports61)
                {
                    ret.SetVersion(61);
                }
                else
                {
                    ret.SetVersion(62);
                }
                if (songinfo)
                {
                    string song = br.ReadString();
                    try
                    {
                        string[] strings = song.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);
                        string fn = Path.Combine(Settings.Local.UserDirPath, Constants.SongsFolderName, strings[0]);
                        if (File.Exists(fn))
                        {
                            if (AudioService.LoadFile(ref fn))
                            {
                                ret.Song = new Song(Path.GetFileName(fn), float.Parse(strings[1], Program.Culture));
                            }
                            else
                            {
                                Program.NonFatalError("An unknown error occured trying to load the song file");
                            }
                        }
                    }
                    catch
                    {
                        // Ignored
                    }
                }
                ret.StartOffset = new Vector2d(br.ReadDouble(), br.ReadDouble());
                int lines = br.ReadInt32();
                List<LineTrigger> linetriggers = [];
                for (int i = 0; i < lines; i++)
                {
                    GameLine l;
                    byte ltype = br.ReadByte();
                    LineType lt = (LineType)(ltype & 0x1F); // We get 5 bits
                    bool inv = (ltype >> 7) != 0;
                    int lim = (ltype >> 5) & 0x3;
                    int ID = -1;
                    int prvID = -1;
                    int nxtID = -1;
                    int multiplier = 1;
                    float linewidth = 1f;
                    LineTrigger tr = null;
                    if (redmultipier)
                    {
                        if (lt == LineType.Acceleration)
                        {
                            multiplier = br.ReadByte();
                        }
                    }
                    if (lt == LineType.Standard || lt == LineType.Acceleration)
                    {
                        if (ignorabletrigger)
                        {
                            tr = new LineTrigger();
                            bool zoomtrigger = br.ReadBoolean();
                            if (zoomtrigger)
                            {
                                tr.ZoomTrigger = true;
                                float target = br.ReadSingle();
                                short frames = br.ReadInt16();
                                tr.ZoomFrames = frames;
                                tr.ZoomTarget = target;
                            }
                            else
                            {
                                tr = null;
                            }
                        }
                        ID = br.ReadInt32();
                        if (lim != 0)
                        {
                            prvID = br.ReadInt32(); // Ignored
                            nxtID = br.ReadInt32(); // Ignored
                        }
                    }
                    if (lt == LineType.Scenery)
                    {
                        if (scenerywidth)
                        {
                            float b = br.ReadByte();
                            linewidth = b / 10f;
                        }
                    }
                    double x1 = br.ReadDouble();
                    double y1 = br.ReadDouble();
                    double x2 = br.ReadDouble();
                    double y2 = br.ReadDouble();

                    if (tr != null)
                    {
                        tr.LineID = ID;
                        linetriggers.Add(tr);
                    }
                    switch (lt)
                    {
                        case LineType.Standard:
                            StandardLine bl = new(new Vector2d(x1, y1), new Vector2d(x2, y2), inv)
                            {
                                ID = ID,
                                Extension = (StandardLine.Ext)lim
                            };
                            l = bl;
                            break;

                        case LineType.Acceleration:
                            RedLine rl = new(new Vector2d(x1, y1), new Vector2d(x2, y2), inv)
                            {
                                ID = ID,
                                Extension = (StandardLine.Ext)lim
                            };
                            if (redmultipier)
                            {
                                rl.Multiplier = multiplier;
                            }
                            l = rl;
                            break;

                        case LineType.Scenery:
                            l = new SceneryLine(new Vector2d(x1, y1), new Vector2d(x2, y2)) { Width = linewidth };

                            break;

                        default:
                            throw new TrackIO.TrackLoadException("Invalid line type at ID " + ID);
                    }
                    if (l is StandardLine line)
                    {
                        if (!addedlines.ContainsKey(l.ID))
                        {
                            addedlines[ID] = line;
                            ret.AddLine(l);
                        }
                    }
                    else
                    {
                        ret.AddLine(l);
                    }
                }
                ret.Triggers = TriggerConverter.ConvertTriggers(linetriggers, ret);
                if (br.BaseStream.Position != br.BaseStream.Length)
                {
                    int meta = br.ReadInt32();
                    if (meta == ('M' | 'E' << 8 | 'T' << 16 | 'A' << 24))
                    {
                        ParseMetadata(ret, br);
                    }
                    else
                    {
                        throw new TrackIO.TrackLoadException("Expected metadata tag but got " + meta.ToString("X8"));
                    }
                }
            }
            return ret;
        }
        private static string ReadString(BinaryReader br) => Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt16()));
    }
}