using linerider.Game;
using linerider.IO.SOL;
using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace linerider.IO
{
    public static class SOLLoader
    {
        public static List<sol_track> LoadSol(string sol_location)
        {
            SOLFile sol = new SOLFile(sol_location);
            List<Amf0Object> tracks = (List<Amf0Object>)sol.RootObject.get_property("trackList");
            List<sol_track> ret = new List<sol_track>();
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].data is List<Amf0Object> list)
                {
                    sol_track add = new sol_track { data = list, filename = sol_location };
                    add.name = (string)add.get_property("label");
                    ret.Add(add);
                }
            }
            return ret;
        }
        public static Track LoadTrack(sol_track trackdata)
        {
            Track ret = new Track { Name = trackdata.name, Filename = trackdata.filename };
            List<Amf0Object> buffer = (List<Amf0Object>)trackdata.get_property("data");
            List<GameLine> lineslist = new List<GameLine>(buffer.Count);
            Dictionary<int, StandardLine> addedlines = new Dictionary<int, StandardLine>(buffer.Count);
            string version = trackdata.data.First(x => x.name == "version").data as string;

            if (version == "6.2")
            {
                ret.SetVersion(62);
            }
            else if (version == "6.1")
            {
                ret.SetVersion(61);
            }
            else
            {
                ret.SetVersion(60);
            }
            try
            {
                List<Amf0Object> options = (List<Amf0Object>)trackdata.get_property("trackData");
                if (options.Count >= 2)
                {
                    try
                    {
                        ret.ZeroStart = (bool)options.Find(x => x.name == "2").get_property("5");
                    }
                    catch
                    {
                        // Ignored
                    }
                }
            }
            catch
            {
                // Ignored
            }
            for (int i = buffer.Count - 1; i >= 0; --i)
            {
                List<Amf0Object> line = (List<Amf0Object>)buffer[i].data;
                int type = Convert.ToInt32(line[9].data, CultureInfo.InvariantCulture);
                switch (type)
                {
                    case 0:
                    {
                        StandardLine l =
                            new StandardLine(
                                new Vector2d(Convert.ToDouble(line[0].data, CultureInfo.InvariantCulture),
                                    Convert.ToDouble(line[1].data, CultureInfo.InvariantCulture)),
                                new Vector2d(Convert.ToDouble(line[2].data, CultureInfo.InvariantCulture),
                                    Convert.ToDouble(line[3].data, CultureInfo.InvariantCulture)),
                                Convert.ToBoolean(line[5].data, CultureInfo.InvariantCulture))
                            {
                                ID = Convert.ToInt32(line[8].data, CultureInfo.InvariantCulture),
                                Extension = (StandardLine.Ext)
                            Convert.ToInt32(
                                line[4].data,
                                CultureInfo.InvariantCulture)
                            };
                        if (line[6].data != null)
                        {
                            int prev = Convert.ToInt32(line[6].data, CultureInfo.InvariantCulture);
                        }
                        if (line[7].data != null)
                        {
                            int next = Convert.ToInt32(line[7].data, CultureInfo.InvariantCulture);
                        }
                        if (!addedlines.ContainsKey(l.ID))
                        {
                            lineslist.Add(l);
                            addedlines[l.ID] = l;
                        }
                    }
                    break;

                    case 1:
                    {
                        RedLine l =
                            new RedLine(
                                new Vector2d(Convert.ToDouble(line[0].data, CultureInfo.InvariantCulture),
                                    Convert.ToDouble(line[1].data, CultureInfo.InvariantCulture)),
                                new Vector2d(Convert.ToDouble(line[2].data, CultureInfo.InvariantCulture),
                                    Convert.ToDouble(line[3].data, CultureInfo.InvariantCulture)),
                                Convert.ToBoolean(line[5].data, CultureInfo.InvariantCulture))
                            {
                                ID = Convert.ToInt32(line[8].data, CultureInfo.InvariantCulture),
                                Extension = (StandardLine.Ext)
                            Convert.ToInt32(
                                line[4].data,
                                CultureInfo.InvariantCulture)
                            };
                        if (line[6].data != null)
                        {
                            int prev = Convert.ToInt32(line[6].data, CultureInfo.InvariantCulture);
                        }
                        if (line[7].data != null)
                        {
                            int next = Convert.ToInt32(line[7].data, CultureInfo.InvariantCulture);
                        }
                        if (!addedlines.ContainsKey(l.ID))
                        {
                            lineslist.Add(l);
                            addedlines[l.ID] = l;
                        }
                    }
                    break;

                    case 2:
                        lineslist.Add(
                            new SceneryLine(
                                new Vector2d(Convert.ToDouble(line[0].data, CultureInfo.InvariantCulture),
                                    Convert.ToDouble(line[1].data, CultureInfo.InvariantCulture)),
                                new Vector2d(Convert.ToDouble(line[2].data, CultureInfo.InvariantCulture),
                                    Convert.ToDouble(line[3].data, CultureInfo.InvariantCulture))));
                        break;

                    default:
                        throw new TrackIO.TrackLoadException("Unknown line type");
                }
            }
            object startlineprop = trackdata.get_property("startLine");
            List<Amf0Object> startline = startlineprop as List<Amf0Object>;
            if (startline == null && startlineprop is double)
            {
                int conv = Convert.ToInt32(startlineprop, CultureInfo.InvariantCulture);
                if (conv >= ret.Lines.Count || conv < 0)
                {
                    startline = new List<Amf0Object>
                    {
                        new Amf0Object { data = 100 },
                        new Amf0Object { data = 100 }
                    };
                }
            }
            else if (startlineprop is double)
            {
                int conv = Convert.ToInt32(startlineprop, CultureInfo.InvariantCulture);
                startline = new List<Amf0Object>
                {
                    new Amf0Object { data = lineslist[conv].Position1.X },
                    new Amf0Object { data = lineslist[conv].Position1.Y - 50 * 0.5 }
                };
            }
            ret.StartOffset = new Vector2d(
                Convert.ToDouble(startline[0].data, CultureInfo.InvariantCulture),
                Convert.ToDouble(startline[1].data, CultureInfo.InvariantCulture));
            foreach (GameLine line in lineslist)
            {
                ret.AddLine(line);
            }
            return ret;
        }
    }
}
