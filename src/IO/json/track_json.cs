using linerider.Utils;
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
        public int bgR { get; set; } = (int) Math.Round(255f * Constants.ColorOffwhite.R);
        public int bgG { get; set; } = (int) Math.Round(255f * Constants.ColorOffwhite.G);
        public int bgB { get; set; } = (int) Math.Round(255f * Constants.ColorOffwhite.B);
        public int lineR { get; set; } = (int) Math.Round(255f * Constants.DefaultLineColor.R);
        public int lineG { get; set; } = (int) Math.Round(255f * Constants.DefaultLineColor.G);
        public int lineB { get; set; } = (int) Math.Round(255f * Constants.DefaultLineColor.B);
        public bool zeroStart { get; set; }
        public float yGravity = 1; //Default grav
        public float xGravity = 0; //Default grav
        public double gravityWellSize = 10; //Default grav well size
        public int duration { get; set; }
        public string version { get; set; }
        public point_json startPosition { get; set; }
        public List<line_json> lines { get; set; }
        public string linesArrayCompressed { get; set; }
        public object[][] linesArray { get; set; }
        public List<zoomtrigger_json> triggers { get; set; }
        public List<gametrigger_json> gameTriggers { get; set; }


        public bool ShouldSerializezeroStart()
        {
            return zeroStart;
        }
        public bool ShouldSerializecreator()
        {
            if (creator != null && creator.Length != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ShouldSerializetriggers()
        {
            if (triggers != null && triggers.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ShouldSerializeduration()
        {
            if (duration > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ShouldSerializedescription()
        {
            if (!string.IsNullOrEmpty(description))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ShouldSerializelinesArrayCompressed()
        {
            if (!string.IsNullOrEmpty(linesArrayCompressed))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ShouldSerializelinesArray()
        {
            if (linesArray != null && linesArray.Length != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
