using System.Linq;

namespace linerider.Drawing.RiderModel
{
    internal class ResourcesDefault : Resources
    {
        public override void Load()
        {
            Body = GameResources.body_img;
            BodyDead = GameResources.bodydead_img;

            Sled = GameResources.sled_img;
            SledBroken = GameResources.sledbroken_img;

            Arm = GameResources.arm_img;
            Leg = GameResources.leg_img;

            Rope = GameResources.rope_img;

            RegionsCacheLines = GameResources.regions_file.Replace("\r", string.Empty).Split('\n').ToList();

            Legacy = false;

            Cache.Parse(RegionsCacheLines);
        }
    }
}
