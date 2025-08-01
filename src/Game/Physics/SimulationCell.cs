using linerider.Game;

namespace linerider
{
    public class SimulationCell : LineContainer<StandardLine>
    {
        public SimulationCell FullClone()
        {
            SimulationCell ret = new();
            foreach (StandardLine l in this)
            {
                ret.AddLine((StandardLine)l.Clone());
            }
            return ret;
        }
    }
}