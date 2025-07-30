using linerider.Game;
using linerider.IO.SOL;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace linerider.IO
{
    public class SOLWriter
    {

        public static int LineTypeForSOL(LineType t)
        {
            switch (t)
            {
                case LineType.Standard:
                    return 0;
                case LineType.Acceleration:
                    return 1;
                case LineType.Scenery:
                    return 2;
                default:
                    throw new TrackIO.TrackLoadException("Unsupported Linetype");
            }
        }
        public static string SaveTrack(Track track, string savename)
        {
            string dir = TrackIO.GetTrackDirectory(track);
            if (track.Name.Equals(Constants.InternalDefaultTrackName))
                dir = Path.Combine(Settings.Local.UserDirPath, Constants.TracksFolderName, Constants.DefaultTrackName);
            if (!Directory.Exists(dir))
                _ = Directory.CreateDirectory(dir);
            string filename = Path.Combine(dir, savename + ".sol");
            BigEndianWriter bw = new();
            bw.WriteShort(0x00BF); // .sol version
            bw.WriteInt(0); // Length, placeholder            
            bw.WriteString("TCSO");
            bw.WriteBytes([0, 4, 0, 0, 0, 0]);
            bw.WriteMapleString("savedLines");
            bw.WriteInt(0); // Padding
            Amf0Object rootobj = new()
            {
                name = "trackList",
                type = Amf0Object.Amf0Type.AMF0_ECMA_ARRAY
            };
            List<Amf0Object> tracks = [];
            rootobj.data = tracks;
            WriteTrack(tracks, track);
            Amf0 amf = new(bw);
            amf.WriteAmf0Object(rootobj);
            bw.WriteByte(0);
            bw.Reset(2);
            bw.WriteInt(bw.Length - 6);
            File.WriteAllBytes(filename, bw.ToArray());
            return filename;
        }
        private static void WriteTrack(List<Amf0Object> parent, Track trk)
        {
            Amf0Object track = new(parent.Count);
            parent.Add(track);
            List<Amf0Object> trackdata = [];
            track.data = trackdata;
            trackdata.Add(new Amf0Object("label", trk.Name));
            trackdata.Add(new Amf0Object("version", "6.2"));
            trackdata.Add(new Amf0Object("level", trk.Lines.Count));
            Amf0Object sl = new("startLine");
            Amf0Object dataobj = new("data") { type = Amf0Object.Amf0Type.AMF0_ECMA_ARRAY };

            List<Amf0Object> data = [];
            dataobj.data = data;
            sl.data = new List<Amf0Object>() { new(0, trk.StartOffset.X), new(1, trk.StartOffset.Y) };

            trackdata.Add(sl);
            trackdata.Add(dataobj);

            SortedList<int, GameLine> list = [];
            GameLine[] lines = trk.GetLines();
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                int id = lines[i].ID;
                if (id < 0)
                {
                    id = Math.Abs(id) + trk.Lines.Max() + 100;
                }
                list.Add(id, lines[i]);
            }
            int counter = 0;
            for (int i = list.Values.Count - 1; i >= 0; i--)
            {
                GameLine line = list.Values[i];
                StandardLine stl = line as StandardLine;
                Amf0Object lineobj = new(counter++);
                List<Amf0Object> linedata =
                [
                    new Amf0Object(0, line.Position1.X),
                    new Amf0Object(1, line.Position1.Y),
                    new Amf0Object(2, line.Position2.X),
                    new Amf0Object(3, line.Position2.Y),
                    new Amf0Object(4, stl != null ? (int)((StandardLine)line).Extension : 0),
                    new Amf0Object(5, stl != null && ((StandardLine)line).inv),
                    new Amf0Object(6, 0), //stl?.Prev?.ID
                    new Amf0Object(7, 0), //tl?.Next?.ID
                    new Amf0Object(8, list.Keys[i]),
                    new Amf0Object(9, LineTypeForSOL(line.Type))
                ];

                lineobj.type = Amf0Object.Amf0Type.AMF0_ECMA_ARRAY;
                lineobj.data = linedata;
                data.Add(lineobj);
            }
            if (trk.ZeroStart)
            {
                List<Amf0Object> kevans =
                [
                    new Amf0Object(0) { type = Amf0Object.Amf0Type.AMF0_NULL }
                ];
                List<Amf0Object> in1 =
                [
                    new Amf0Object(0) { type = Amf0Object.Amf0Type.AMF0_NULL },
                    new Amf0Object(1) { type = Amf0Object.Amf0Type.AMF0_NULL },
                    new Amf0Object(2) { type = Amf0Object.Amf0Type.AMF0_NULL }
                ];
                kevans.Add(new Amf0Object(1) { type = Amf0Object.Amf0Type.AMF0_ECMA_ARRAY, data = in1 });
                List<Amf0Object> importantpart = new(3)
                {
                    new Amf0Object(0) { type = Amf0Object.Amf0Type.AMF0_NULL },
                    new Amf0Object(1) { type = Amf0Object.Amf0Type.AMF0_NULL },
                    new Amf0Object(2) { type = Amf0Object.Amf0Type.AMF0_NULL },
                    new Amf0Object(3) { type = Amf0Object.Amf0Type.AMF0_NULL },
                    new Amf0Object(4) { type = Amf0Object.Amf0Type.AMF0_NULL },
                    new Amf0Object(5, true)
                };
                kevans.Add(new Amf0Object(2) { type = Amf0Object.Amf0Type.AMF0_ECMA_ARRAY, data = importantpart });
                trackdata.Add(new Amf0Object("trackData") { data = kevans, type = Amf0Object.Amf0Type.AMF0_ECMA_ARRAY });
            }
        }
    }
}
