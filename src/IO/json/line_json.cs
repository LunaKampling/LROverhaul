namespace linerider.IO.json
{
    public struct line_json
    {
        public int id { get; set; }
        public int type { get; set; }
        public double x1 { get; set; }
        public double y1 { get; set; }
        public double x2 { get; set; }
        public double y2 { get; set; }
        public int extended { get; set; }
        public float width { get; set; }
        // There are parsing errors if any booleans are saved as numbers
        // we eliminate them by just loading them as 'object' and handling them
        // ourselves
        public object flipped { get; set; }
        public object leftExtended { get; set; }
        public object rightExtended { get; set; }
        public double multiplier { get; set; }
        public bool ShouldSerializemultiplier() => multiplier != 0 && type == 1;
        public bool ShouldSerializeextended() => extended != 0;
        public bool ShouldSerializeleftExtended() => false;
        public bool ShouldSerializerightExtended() => false;
    }
}
