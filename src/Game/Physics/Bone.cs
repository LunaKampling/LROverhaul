namespace linerider.Game
{
    public sealed class Bone(int joint1, int joint2, double rest, bool breakable, bool onlyrepel) // Haha Boner Ray - Arglin
    {
        public readonly int joint1 = joint1;
        public readonly int joint2 = joint2;
        public readonly double RestLength = rest;

        public readonly bool Breakable = breakable;
        public readonly bool OnlyRepel = onlyrepel;
    }
}