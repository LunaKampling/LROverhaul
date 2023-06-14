﻿//  Author:
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

namespace linerider.Audio
{
    public struct Song
    {
        public bool Enabled;
        public string Location;
        public float Offset;

        public Song(string location, float offset)
        {
            Location = location;
            Offset = offset;
            Enabled = true;
        }
        public override string ToString()
        {
            // NOTE: .trk spec mandates \r\n newline here
            return Location + "\r\n" + Offset.ToString(Program.Culture);
            ;
        }
    }
}
