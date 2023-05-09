using System;
using System.Collections.Generic;
using OpenTK;
namespace linerider.Game
{
    public class RiderConstants
    {
        public static Vector2d[] DefaultRider = createDefaultRider();
        public static Vector2d[] createDefaultRider()
        {
            List<Vector2d> defaultRiderVectorList = new List<Vector2d>();
            
            defaultRiderVectorList.Add((new Vector2d(0, 0)));
            defaultRiderVectorList.Add(new Vector2d(0, 5));
            defaultRiderVectorList.Add(new Vector2d(15, 5));
            defaultRiderVectorList.Add(new Vector2d(17.5, 0));
            defaultRiderVectorList.Add(new Vector2d(5, 0));
            defaultRiderVectorList.Add(new Vector2d(5, -5.5));
            defaultRiderVectorList.Add(new Vector2d(11.5, -5));
            defaultRiderVectorList.Add(new Vector2d(11.5, -5));
            defaultRiderVectorList.Add(new Vector2d(10, 5));
            defaultRiderVectorList.Add(new Vector2d(10, 5));

            return defaultRiderVectorList.ToArray();
        }
        public static Vector2d[] DefaultScarf = createDefaultScarf();
        public static Vector2d[] createDefaultScarf()
        {
            List<Vector2d> scarfVectors = new List<Vector2d>();
            double scarfPos = 0;
            for (int i=0; i<Settings.ScarfSegments; i++)
            {
                if (i % 2 == 0) {
                    scarfPos = scarfPos - 2;
                    scarfVectors.Add(new Vector2d(scarfPos, 0.5)); 
                }
                else {
                    scarfPos = scarfPos - 1.5;
                    scarfVectors.Add(new Vector2d(scarfPos, 0.5)); 
                }
                
            }
            return scarfVectors.ToArray();
        }
        public const double EnduranceFactor = 0.0285;
        public const double RemountEnduranceMultiplier = 2.0;
        public const double RemountStrengthMultiplier = 0.5;
        public const double StartingMomentum = 0.4;
        public static Vector2d Gravity = new Vector2d(0, 0.175); //gravity
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
        public static readonly Bone[] Bones;
        public static Bone[] ScarfBones;
        static RiderConstants()
        {
            var bonelist = new List<Bone>();
            bonelist.Add(CreateBone(SledTL, SledBL));
            bonelist.Add(CreateBone(SledBL, SledBR));
            bonelist.Add(CreateBone(SledBR, SledTR));
            bonelist.Add(CreateBone(SledTR, SledTL));
            bonelist.Add(CreateBone(SledTL, SledBR));
            bonelist.Add(CreateBone(SledTR, SledBL));

            bonelist.Add(CreateBone(SledTL, BodyButt, breakable: true));
            bonelist.Add(CreateBone(SledBL, BodyButt, breakable: true));
            bonelist.Add(CreateBone(SledBR, BodyButt, breakable: true));

            bonelist.Add(CreateBone(BodyShoulder, BodyButt));
            bonelist.Add(CreateBone(BodyShoulder, BodyHandLeft));
            bonelist.Add(CreateBone(BodyShoulder, BodyHandRight));
            bonelist.Add(CreateBone(BodyButt, BodyFootLeft));
            bonelist.Add(CreateBone(BodyButt, BodyFootRight));
            bonelist.Add(CreateBone(BodyShoulder, BodyHandRight));

            bonelist.Add(CreateBone(BodyShoulder, SledTL, breakable: true));
            bonelist.Add(CreateBone(SledTR, BodyHandLeft, breakable: true));
            bonelist.Add(CreateBone(SledTR, BodyHandRight, breakable: true));
            bonelist.Add(CreateBone(BodyFootLeft, SledBR, breakable: true));
            bonelist.Add(CreateBone(BodyFootRight, SledBR, breakable: true));

            bonelist.Add(CreateBone(BodyShoulder, BodyFootLeft, repel: true));
            bonelist.Add(CreateBone(BodyShoulder, BodyFootRight, repel: true));
            Bones = bonelist.ToArray();
            bonelist = new List<Bone>();

            for (int i=0; i<Settings.ScarfSegments; i++)
            {
                AddScarfBone(bonelist, i+1);
            }
            ScarfBones = bonelist.ToArray();
        }
        private static void AddScarfBone(List<Bone> bones, int index)
        {
            var even = index % 2 == 0;
            bones.Add(new Bone(index - 1, index, even ? 1.5 : 2.0, false, false));
        }
        private static Bone CreateBone(int a, int b, bool breakable = false, bool repel = false)
        {
            var rest = (DefaultRider[a] - DefaultRider[b]).Length;
            if (repel)
            {
                rest *= 0.5;
            }
            var ret = new Bone(a, b, rest, breakable, repel);
            return ret;
        }
    }
}
