using linerider.Drawing.RiderModel;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace linerider.Game
{
    public class RiderConstants
    {
        public static Vector2d[] DefaultRider = CreateDefaultRider();
        public static Vector2d[] CreateDefaultRider()
        {
            List<Vector2d> defaultRiderVectorList =
            [
                new Vector2d(0, 0),
                new Vector2d(0, 5),
                new Vector2d(15, 5),
                new Vector2d(17.5, 0),
                new Vector2d(5, 0),
                new Vector2d(5, -5.5),
                new Vector2d(11.5, -5),
                new Vector2d(11.5, -5),
                new Vector2d(10, 5),
                new Vector2d(10, 5)
            ];

            return [.. defaultRiderVectorList];
        }
        public static Vector2d[] DefaultScarf = CreateDefaultScarf();
        public static Vector2d[] CreateDefaultScarf()
        {
            List<Vector2d> scarfVectors = [];
            double scarfPos = 0;
            for (int i = 0; i < ScarfColors.TotalSegments; i++)
            {
                if (i % 2 == 0)
                {
                    scarfPos -= 2;
                    scarfVectors.Add(new Vector2d(scarfPos, 0.5));
                }
                else
                {
                    scarfPos -= 1.5;
                    scarfVectors.Add(new Vector2d(scarfPos, 0.5));
                }
            }
            return [.. scarfVectors];
        }
        public const double EnduranceFactor = 0.0285;
        public const double RemountEnduranceMultiplier = 2.0;
        public const double RemountStrengthMultiplier = 0.5;
        public const double StartingMomentum = 0.4;
        public static Vector2d Gravity = new(0, 0.175); // Gravity
        public const int SledTL = 0;
        public const int SledBL = 1;
        public const int SledBR = 2;
        public const int SledTR = 3;
        public const int BodyButt = 4;
        public const int BodyShoulder = 5;
        public const int BodyHandLeft = 6;
        public const int BodyHandRight = 7;
        public const int BodyFootLeft = 8;
        public const int BodyFootRight = 9;

        public static string contactPointName(int contactPoint)
        {
            switch (contactPoint)
            {
                case SledTL:
                    return "SledTL";
                case SledBL:
                    return "SledBL";
                case SledBR:
                    return "SledBR";
                case SledTR:
                    return "SledTR";
                case BodyButt:
                    return "BodyButt";
                case BodyShoulder:
                    return "BodyShoulder";
                case BodyHandLeft:
                    return "BodyHandLeft";
                case BodyHandRight:
                    return "BodyHandRight";
                case BodyFootLeft:
                    return "BodyFootLeft";
                case BodyFootRight:
                    return "BodyFootRight";
            }

            return "?";
        }

        public const int Iterations = 6;
        public const int Subiterations = 22;

        public static readonly Bone[] Bones;
        public static Bone[] ScarfBones;
        static RiderConstants()
        {
            List<Bone> bonelist =
            [
                CreateBone(SledTL, SledBL),
                CreateBone(SledBL, SledBR),
                CreateBone(SledBR, SledTR),
                CreateBone(SledTR, SledTL),
                CreateBone(SledTL, SledBR),
                CreateBone(SledTR, SledBL),

                CreateBone(SledTL, BodyButt, breakable: true),
                CreateBone(SledBL, BodyButt, breakable: true),
                CreateBone(SledBR, BodyButt, breakable: true),

                CreateBone(BodyShoulder, BodyButt),
                CreateBone(BodyShoulder, BodyHandLeft),
                CreateBone(BodyShoulder, BodyHandRight),
                CreateBone(BodyButt, BodyFootLeft),
                CreateBone(BodyButt, BodyFootRight),
                CreateBone(BodyShoulder, BodyHandRight),

                CreateBone(BodyShoulder, SledTL, breakable: true),
                CreateBone(SledTR, BodyHandLeft, breakable: true),
                CreateBone(SledTR, BodyHandRight, breakable: true),
                CreateBone(BodyFootLeft, SledBR, breakable: true),
                CreateBone(BodyFootRight, SledBR, breakable: true),

                CreateBone(BodyShoulder, BodyFootLeft, repel: true),
                CreateBone(BodyShoulder, BodyFootRight, repel: true)
            ];
            Bones = [.. bonelist];
            bonelist = [];

            for (int i = 0; i < ScarfColors.TotalSegments; i++)
            {
                AddScarfBone(bonelist, i + 1);
            }
            ScarfBones = [.. bonelist];
        }
        private static void AddScarfBone(List<Bone> bones, int index)
        {
            bool even = index % 2 == 0;
            bones.Add(new Bone(index - 1, index, even ? 1.5 : 2.0, false, false));
        }
        private static Bone CreateBone(int a, int b, bool breakable = false, bool repel = false)
        {
            double rest = (DefaultRider[a] - DefaultRider[b]).Length;
            if (repel)
            {
                rest *= 0.5;
            }
            Bone ret = new(a, b, rest, breakable, repel);
            return ret;
        }
    }
}