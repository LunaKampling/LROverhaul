//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using linerider.Utils;
using System;
using System.Diagnostics;

namespace linerider
{
    public class GameScheduler
    {
        private readonly Stopwatch sw = new();
        private double lastupdate = 0;
        private double updateperiod = 1;
        private bool reset = true;
        public float UpdatePeriod => (float)updateperiod;
        public int UpdatesPerSecond
        {
            get => (int)(1.0 / updateperiod);
            set
            {
                if (UpdatesPerSecond != value)
                {
                    updateperiod = 1.0 / value;
                    reset = true;
                }
            }
        }

        public float ElapsedPercent
        {
            get
            {
                double totalelapsed = sw.Elapsed.TotalSeconds;
                double elapsed = totalelapsed - lastupdate;
                return (float)(elapsed / updateperiod);
            }
        }
        public GameScheduler()
        {
            DefaultSpeed();
        }
        public void DefaultSpeed() => UpdatesPerSecond = (int)Math.Round(Constants.PhysicsRate * Settings.DefaultPlayback);
        public double Rate => Math.Round((double)UpdatesPerSecond / Constants.PhysicsRate, 3);

        public int UnqueueUpdates()
        {
            if (updateperiod == 0)
                return 1;
            if (!sw.IsRunning)
                sw.Start();
            int updates = 0;
            double totalelapsed = sw.Elapsed.TotalSeconds;
            double elapsed = totalelapsed - lastupdate;
            if (reset)
            {
                reset = false;
                lastupdate = totalelapsed;
                return 0;
            }
            else
            {
                while (elapsed >= updateperiod)
                {
                    elapsed -= updateperiod;
                    updates++;
                    int cap = (int)(2 +
                    UpdatesPerSecond / (float)Constants.PhysicsRate);
                    if (updates >= cap)
                    {
                        elapsed = Math.Min(elapsed, updateperiod * cap);
                        break;
                    }
                }
            }
            if (updates > 0)
                lastupdate = totalelapsed - elapsed;
            return updates;
        }

        public void Reset() => reset = true;
    }
}