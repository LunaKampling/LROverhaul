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

using System;

namespace linerider.Tools
{
    public class Swatch : GameService
    {
        public float GreenMultiplier { get; set; } = 1;
        public double RedMultiplier { get; set; } = 1;

        public const int MaxRedMultiplier = 3;
        public const int MinRedMultiplier = 1;
        public const int MaxGreenMultiplier = 3;
        public const float MinGreenMultiplier = 0.5f;
        public LineType Selected { get; set; } = LineType.Standard;
        public void IncrementSelectedMultiplier()
        {
            if (CurrentTools.CurrentTool != CurrentTools.EraserTool &&
                CurrentTools.CurrentTool != CurrentTools.SelectSubtool &&
                CurrentTools.CurrentTool != CurrentTools.SelectTool &&
            CurrentTools.CurrentTool.ShowSwatch)
            {
                Swatch sw = CurrentTools.CurrentTool.Swatch;
                switch (Selected)
                {
                    case LineType.Acceleration:
                    {
                        double mul = sw.RedMultiplier;
                        mul++;
                        if (mul > MaxRedMultiplier)
                            mul = MinRedMultiplier;
                        sw.RedMultiplier = mul;
                    }
                    break;
                    case LineType.Scenery:
                    {
                        float mul = sw.GreenMultiplier;
                        mul++;
                        mul = (float)Math.Floor(mul);
                        if (mul > MaxGreenMultiplier)
                            mul = MinGreenMultiplier;
                        sw.GreenMultiplier = mul;
                    }
                    break;
                }
            }
        }
    }
}