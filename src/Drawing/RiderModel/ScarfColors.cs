using System.Collections.Generic;
using System.Linq;

namespace linerider.Drawing.RiderModel
{
    public static class ScarfColors
    {
        private static readonly List<int> Colors = new List<int>();
        private static readonly List<byte> Opacity = new List<byte>();
        public static List<int> GetColorList() => Colors;
        public static List<byte> GetOpacityList() => Opacity;
        public static int TotalSegments => Settings.ScarfSegmentsPrimary + (Settings.ScarfAmount - 1) * (Settings.ScarfSegmentsSecondary + 1);
        public static void Add(int color, byte opacity)
        {
            Colors.Add(color);
            Opacity.Add(opacity);
        }
        public static void Insert(int color, byte opacity, int index)
        {
            Colors.Insert(index, color);
            Opacity.Insert(index, opacity);
        }
        public static void Remove(int index)
        {
            Colors.RemoveAt(index);
            Opacity.RemoveAt(index);
        }
        public static bool AreEmpty() => !Colors.Any();
        public static int Count() => Colors.Count();
        public static void RemoveAll()
        {
            Colors.Clear();
            Opacity.Clear();
        }
        public static void Shift(int shift) // Shifts scarf colors to the left
        {
            for (int i = 0; i < shift; i++)
            {
                Insert(GetColorList()[Count() - 1], GetOpacityList()[Count() - 1], 0);
                Remove(Count() - 1);
            }
        }
        public static void Normalize() // Make sure scarf is long enough
        {
            while (Count() < TotalSegments)
            {
                GetColorList().AddRange(GetColorList());
                GetOpacityList().AddRange(GetOpacityList());
            }
        }
        public static void SetDefault()
        {
            RemoveAll();
            Add(0xB93332, 0xFF);
            Add(0xEF7A5D, 0xFF);
            Normalize();
        }
    }
}
