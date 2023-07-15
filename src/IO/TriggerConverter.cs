using linerider.Game;
using linerider.Utils;
using System.Collections.Generic;

namespace linerider.IO
{
    /// <summary>
    /// Static class that converts line based triggers to a timeline--based
    /// trigger system.
    /// </summary>
    public static class TriggerConverter
    {
        public static List<GameTrigger> ConvertTriggers(List<LineTrigger> triggers, Track track)
        {
            List<GameTrigger> gametriggers = new List<GameTrigger>();
            const int minute = Constants.PhysicsRate * 60;
            int lasthit = 0;
            Rider rider = track.GetStart();
            HitTestManager hittest = new HitTestManager();
            int i = 1;
            int hitframe = -1;
            LineTrigger activetrigger = null;
            float zoom = track.StartZoom;
            GameTrigger newtrigger = null;
            do
            {
                LinkedList<int> collisions = new LinkedList<int>();
                rider = rider.Simulate(
                    track.Grid,
                    track.Bones,
                    collisions);
                hittest.AddFrame(collisions);
                LineTrigger hittrigger = null;
                foreach (int lineid in collisions)
                {
                    foreach (LineTrigger trigger in triggers)
                    {
                        if (trigger.LineID == lineid)
                        {
                            hittrigger = trigger;
                        }
                    }
                }
                if (hittrigger != null &&
                    hittrigger != activetrigger)
                {
                    if (activetrigger != null)
                    {
                        newtrigger.ZoomTarget = zoom;
                        newtrigger.End = i;
                        gametriggers.Add(newtrigger);
                    }
                    hitframe = i;
                    activetrigger = hittrigger;
                    newtrigger = new GameTrigger() { TriggerType = TriggerType.Zoom, Start = i };
                }
                if (activetrigger != null)
                {
                    int delta = i - hitframe;
                    if (!activetrigger.Activate(delta, ref zoom))
                    {
                        newtrigger.ZoomTarget = zoom;
                        newtrigger.End = i;
                        gametriggers.Add(newtrigger);
                        activetrigger = null;
                    }
                }
                if (hittest.HasUniqueCollisions(i))
                {
                    lasthit = i;
                }
                i++;
            }
            while (i - lasthit < (minute * 2)); // Be REALLY sure, 2 minutes.
            return gametriggers;
        }
    }
}