using System.Linq;

namespace linerider.Drawing.RiderModel
{
    internal class ResourcesDefault : Resources
    {
        public override void Load()
        {
            Body = GameResources.rider_body;
            BodyDead = GameResources.rider_bodydead;

            Sled = GameResources.rider_sled;
            SledBroken = GameResources.rider_sledbroken;

            Arm = GameResources.rider_arm;
            Leg = GameResources.rider_leg;

            Rope = GameResources.rider_rope;

            RegionsCacheLines = GameResources.rider_regions_file.Replace("\r", string.Empty).Split('\n').ToList();

            Legacy = false;

            _ = Cache.Parse(RegionsCacheLines);
        }
    }
}
