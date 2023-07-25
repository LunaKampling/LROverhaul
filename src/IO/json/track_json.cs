using System;
using System.Collections.Generic;
namespace linerider.IO.json
{
    public class track_json
    {
        public class zoomtrigger_json //Line zoom trigger
        {
            public int ID;
            public bool zoom;
            public float target;
            public int frames;
        }
        public class gametrigger_json
        {
            public int start;
            public int end;
            public int triggerType;
            public float zoomTarget = -999;
            public int backgroundred = -999;
            public int backgroundgreen = -999;
            public int backgroundblue = -999;
            public int lineRed = -999;
            public int lineGreen = -999;
            public int lineBlue = -999;
        }
        public class point_json
        {
            public double x;
            public double y;
        }
        public string label { get; set; }
        public string creator { get; set; }
        public string description { get; set; }
        public float startZoom { get; set; }
        public int bgR { get; set; } = Settings.Colors.ExportBg.R;
        public int bgG { get; set; } = Settings.Colors.ExportBg.G;
        public int bgB { get; set; } = Settings.Colors.ExportBg.B;
        public int lineR { get; set; } = Settings.Colors.ExportLine.R;
        public int lineG { get; set; } = Settings.Colors.ExportLine.G;
        public int lineB { get; set; } = Settings.Colors.ExportLine.B;
        public bool zeroStart { get; set; }
        public float yGravity = 1; // Default gravity
        public float xGravity = 0; // Default gravity
        public double gravityWellSize = 10; // Default Gravity Well Size
        public int duration { get; set; }
        public string version { get; set; }
        public point_json startPosition { get; set; }
        public List<line_json> lines { get; set; }
        public string linesArrayCompressed { get; set; }
        public object[][] linesArray { get; set; }
        public List<zoomtrigger_json> triggers { get; set; }
        public List<gametrigger_json> gameTriggers { get; set; }

        public bool ShouldSerializezeroStart() => zeroStart;
        public bool ShouldSerializecreator() => creator != null && creator.Length != 0;
        public bool ShouldSerializetriggers() => triggers != null && triggers.Count != 0;
        public bool ShouldSerializeduration() => duration > 0;
        public bool ShouldSerializedescription() => !string.IsNullOrEmpty(description);
        public bool ShouldSerializelinesArrayCompressed() => !string.IsNullOrEmpty(linesArrayCompressed);
        public bool ShouldSerializelinesArray() => linesArray != null && linesArray.Length != 0;
    }
}
