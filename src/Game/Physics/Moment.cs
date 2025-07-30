namespace linerider.Game.Physics
{
    /// <summary>
    /// describes a very specific point in the physics simulation, can include intermediate steps like iterations, subiterations, and the individual components of a momentum tick.
    /// </summary>
    public class Moment(int frame, int iteration = RiderConstants.Iterations,
        int subiteration = RiderConstants.Subiterations)
    {
        private int frame = frame;
        private int iteration = iteration;
        private int subiteration = subiteration;

        public int Frame { get { return frame; } set { frame = value; } }
        public int Iteration { get { return iteration; } set { iteration = value; } }
        public int Subiteration { get { return subiteration; } set { subiteration = value; } }

        public bool interactWithLines()
        {
            return iteration != 0 && subiteration == RiderConstants.Subiterations;
        }

        public static bool operator <(Moment a, Moment b)
        {
            if (a.frame < b.frame) return true;
            if (a.iteration < b.iteration) return true;
            if (a.subiteration < b.subiteration) return true;
            return false;
        }

        public static bool operator >(Moment a, Moment b)
        {
            return b < a;
        }

        public static bool operator <=(Moment a, Moment b)
        {
            if (a.frame < b.frame) return true;
            if (a.iteration < b.iteration) return true;
            if (a.subiteration < b.subiteration) return true;
            if (a.frame == b.frame && a.iteration == b.iteration && a.subiteration == b.subiteration) return true;
            return false;
        }

        public static bool operator >=(Moment a, Moment b)
        {
            return b <= a;
        }

        public void IncrementFrame()
        {
            frame++;
            iteration = RiderConstants.Iterations;
            subiteration = maxSubiteration(iteration);
        }

        public Moment NextFrame()
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.IncrementFrame();
            return obj;
        }

        public Moment WithFrame(int fr)
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.Frame = fr;
            return obj;
        }

        public void DecrementFrame()
        {
            if (frame == 0) return;
            frame--;
            iteration = RiderConstants.Iterations;
            subiteration = maxSubiteration(iteration);
        }

        public Moment PreviousFrame()
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.DecrementFrame();
            return obj;
        }

        public void IncrementIteration()
        {
            if (iteration >= RiderConstants.Iterations)
            {
                IncrementFrame();
                iteration = 0;
                subiteration = maxSubiteration(iteration);
            }
            else
            {
                iteration++;
                subiteration = maxSubiteration(iteration);
            }
        }

        public Moment NextIteration()
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.IncrementIteration();
            return obj;
        }

        public Moment WithIteration(int iter)
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.Iteration = iter;
            return obj;
        }

        public void DecrementIteration()
        {
            if (frame == 0) return;
            if (iteration == 0)
            {
                DecrementFrame();
            }
            else
            {
                iteration--;
                subiteration = maxSubiteration(iteration);
            }
        }

        public Moment PreviousIteration()
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.DecrementIteration();
            return obj;
        }

        public void IncrementSubiteration()
        {
            if (subiteration >= maxSubiteration(iteration))
            {
                IncrementIteration();
                subiteration = 0;
            }
            else
            {
                subiteration++;
            }
        }

        public Moment NextSubiteration()
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.IncrementSubiteration();
            return obj;
        }

        public Moment WithSubiteration(int subit)
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.subiteration = subit;
            return obj;
        }

        public void DecrementSubiteration()
        {
            if (frame == 0) return;
            if (subiteration == 0)
            {
                DecrementIteration();
            }
            else
            {
                subiteration--;
            }
        }

        public Moment PreviousSubiteration()
        {
            var obj = (Moment)this.MemberwiseClone();
            obj.DecrementSubiteration();
            return obj;
        }

        public string displayString()
        {
            string value = "";
            if (iteration != RiderConstants.Iterations || subiteration != maxSubiteration(iteration))
            {
                value += $"Iteration {iteration} ";
            }

            if (subiteration != maxSubiteration(iteration))
            {
                string subiterationName = "";
                if (iteration != 0)
                {
                    var bone = RiderConstants.Bones[subiteration];
                    subiterationName = $"{RiderConstants.contactPointName(bone.joint1)}-{RiderConstants.contactPointName(bone.joint2)}";
                }
                else
                {
                    switch (subiteration)
                    {
                        case 0:
                            subiterationName = "Momentum";
                            break;
                        case 1:
                            subiterationName = "Friction";
                            break;
                        case 2:
                            subiterationName = "Acceleration";
                            break;
                    }
                }

                value += $"Subiteration {subiteration} ({subiterationName})";
            }
            return value;
        }

        // for normal iterations, we have 22 bones and then a line collision step
        // for the momentum tick it is broken into these subiterations:
        // - initial momentum
        // - friction
        // - acceleration multipliers
        // - gravity (final position of the momentum tick)
        public static int maxSubiteration(int iteration)
        {
            return iteration == 0 ? 3 : RiderConstants.Subiterations;
        }
    }
}