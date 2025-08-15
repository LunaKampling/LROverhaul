using OpenTK.Mathematics;
using System.Runtime.InteropServices;
namespace linerider.Game
{
    [StructLayout(LayoutKind.Sequential)]

    public struct SimulationPoint(Vector2d location, Vector2d prev, Vector2d momentum, double friction)
    {
        public readonly Vector2d Location = location;
        public readonly Vector2d Previous = prev;
        public readonly Vector2d Momentum = momentum;
        public readonly double Friction = friction;

        public SimulationPoint Step(bool gravity = true)
        {
            Vector2d momentum = Location - Previous + (gravity ? RiderConstants.Gravity : Vector2d.Zero);
            return new SimulationPoint(Location + momentum, Location, momentum, Friction);
        }
        public SimulationPoint StepFriction()
        {
            Vector2d momentum = (Location - Previous) * Friction + RiderConstants.Gravity;
            return new SimulationPoint(Location + momentum, Location, momentum, Friction);
        }
        public SimulationPoint AddPosition(Vector2d add) => new(Location + add, Previous, Momentum, Friction);
        public SimulationPoint Replace(Vector2d location) => new(location, Previous, Momentum, Friction);
        public SimulationPoint Replace(Vector2d location, Vector2d prev) => new(location, prev, Momentum, Friction);
        public override bool Equals(object obj) => obj is SimulationPoint && this == (SimulationPoint)obj;
        public override int GetHashCode() => Location.GetHashCode() ^ Previous.GetHashCode();
        public static bool FastEquals(ref SimulationPoint a, ref SimulationPoint b) => a.Location == b.Location && a.Previous == b.Previous;
        public static bool operator ==(SimulationPoint x, SimulationPoint y)
        {
            return x.Location == y.Location && x.Previous == y.Previous;
        }
        public static bool operator !=(SimulationPoint x, SimulationPoint y)
        {
            return !(x == y);
        }
    }
}