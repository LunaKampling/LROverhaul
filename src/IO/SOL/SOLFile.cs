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
using System.IO;
using System.Text;
namespace linerider.IO.SOL
{
    internal class SOLFile
    {
        public Amf0Object RootObject = new();
        public SOLFile(string location)
        {
            byte[] bytes = File.ReadAllBytes(location);
            BigEndianReader br = new(bytes);

            ///HEADER///
            _ = br.ReadInt16(); // sol_version
            _ = br.ReadInt32(); // File length
            if (br.ReadInt32() != 0x5443534F) // TCSO
                throw new Exception("Invalid magic number, maybe this isn't an SOL file?");
            _ = br.ReadBytes(6); // Padding
            RootObject.name = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt16())); // Shared object name
            if (RootObject.name != "savedLines")
                throw new Exception("invalid root object");
            if (br.ReadInt32() != 0)
                throw new Exception("Invalid AMF version"); // AMF version, we only support 0o

            ///Items///			
            Amf0 amf = new(br);
            RootObject.data = amf.ReadAmf0(true);
        }
    }
}