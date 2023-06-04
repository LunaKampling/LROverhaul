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

using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace linerider
{
    internal class GameResources
    {
        public class VectorResource
        {
            public string Raw;
            public Size BaseSize;
            public Size Size
            {
                get => new Size(
                    (int)Math.Round(BaseSize.Width * Settings.Computed.UIScale),
                    (int)Math.Round(BaseSize.Height * Settings.Computed.UIScale)
                );
            }
            public Bitmap Bitmap
            {
                get => SvgDocument.FromSvg<SvgDocument>(Raw).Draw(Size.Width, Size.Height);
            }
        }
        private static Assembly Assembly = null;
        private static Dictionary<string, object> _lookuptable = null;
        public static void Init()
        {
            if (Assembly == null)
            {
                Assembly = typeof(GameResources).Assembly;
            }
            if (_lookuptable == null)
            {
                _lookuptable = new Dictionary<string, object>();
            }
        }
        public static Bitmap GetBitmap(string name)
        {
            object lookup;
            if (_lookuptable.TryGetValue(name, out lookup))
            {
                return (Bitmap)lookup;
            }
            using (var stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                var ret = new Bitmap(stream);
                _lookuptable[name] = ret;
                return ret;
            }
        }
        private static byte[] GetBytes(string name)
        {
            object lookup;
            if (_lookuptable.TryGetValue(name, out lookup))
            {
                return ((byte[])lookup).ToArray();//prevent writing to resource
            }
            using (var stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                byte[] ret = new byte[stream.Length];
                stream.Read(ret, 0, ret.Length);
                _lookuptable[name] = ret;
                return ret;
            }
        }
        private static string GetString(string name)
        {
            object lookup;
            if (_lookuptable.TryGetValue(name, out lookup))
            {
                return (string)lookup;//strings are immutable so there's no chance of writing to resource
            }
            using (var stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                using (var reader = new StreamReader(stream))
                {
                    var ret = reader.ReadToEnd();
                    _lookuptable[name] = ret;
                    return ret;
                }
            }
        }
        public static VectorResource GetVectorImage(string name)
        {
            XmlDocument doc = new XmlDocument();
            string raw = GetString(name);

            doc.LoadXml(raw);

            XmlNode rootNode = doc.GetElementsByTagName("svg")[0];
            XmlNode viewBoxNode = rootNode.Attributes.GetNamedItem("viewBox");
            string[] boundings = viewBoxNode.InnerText.Split(' ');

            Size size = new Size(
                (int)Math.Round(double.Parse(boundings[2])),
                (int)Math.Round(double.Parse(boundings[3]))
            );

            VectorResource res = new VectorResource()
            {
                Raw = raw,
                BaseSize = size,
            };

            return res;
        }
        internal static byte[] beep
        {
            get
            {
                return GetBytes("beep.wav");
            }
        }
        internal static System.Drawing.Bitmap defaultskin
        {
            get
            {
                return GetBitmap("DefaultSkin.png");
            }
        }

        internal static byte[] icon
        {
            get
            {
                return GetBytes("icon.ico");
            }
        }
        internal static string defaultcolors
        {
            get
            {
                return GetString("DefaultColors.xml");
            }
        }
        #region fonts
        internal static string font_liberation_sans_15_fnt
        {
            get
            {
                return GetString("fonts.liberation_sans_15.fnt");
            }
        }
        internal static System.Drawing.Bitmap font_liberation_sans_15_png
        {
            get
            {
                return GetBitmap("fonts.liberation_sans_15_0.png");
            }
        }
        internal static string font_liberation_sans_15_bold_fnt
        {
            get
            {
                return GetString("fonts.liberation_sans_15_bold.fnt");
            }
        }
        internal static System.Drawing.Bitmap font_liberation_sans_15_bold_png
        {
            get
            {
                return GetBitmap("fonts.liberation_sans_15_bold_0.png");
            }
        }
        #endregion
        #region rider
        internal static Bitmap rider_sled
        {
            get
            {
                return GetBitmap("rider.sled.png");
            }
        }
        internal static Bitmap rider_sledbroken
        {
            get
            {
                return GetBitmap("rider.sledbroken.png");
            }
        }
        internal static Bitmap rider_arm
        {
            get
            {
                return GetBitmap("rider.arm.png");
            }
        }
        internal static Bitmap rider_leg
        {
            get
            {
                return GetBitmap("rider.leg.png");
            }
        }
        internal static Bitmap rider_body
        {
            get
            {
                return GetBitmap("rider.body.png");
            }
        }
        internal static Bitmap rider_bodydead
        {
            get
            {
                return GetBitmap("rider.bodydead.png");
            }
        }
        internal static Bitmap rider_rope
        {
            get
            {
                return GetBitmap("rider.rope.png");
            }
        }
        internal static string rider_regions_file
        {
            get
            {
                return GetString("rider..regions");
            }
        }
        #endregion
        #region cursors

        
        internal static VectorResource cursor_hand
        {
            get
            {
                return GetVectorImage("cursors.hand.svg");
            }
        }
        internal static VectorResource cursor_drag_inactive
        {
            get
            {
                return GetVectorImage("cursors.drag-inactive.svg");
            }
        }
        internal static VectorResource cursor_drag_active
        {
            get
            {
                return GetVectorImage("cursors.drag-active.svg");
            }
        }
        internal static VectorResource cursor_select
        {
            get
            {
                return GetVectorImage("cursors.select.svg");
            }
        }
        internal static VectorResource cursor_line
        {
            get
            {
                return GetVectorImage("cursors.line.svg");
            }
        }
        internal static VectorResource cursor_eraser
        {
            get
            {
                return GetVectorImage("cursors.eraser.svg");
            }
        }
        internal static VectorResource cursor_pencil
        {
            get
            {
                return GetVectorImage("cursors.pencil.svg");
            }
        }
        internal static VectorResource cursor_size_swne
        {
            get
            {
                return GetVectorImage("cursors.size-swne.svg");
            }
        }
        internal static VectorResource cursor_size_nwse
        {
            get
            {
                return GetVectorImage("cursors.size-nwse.svg");
            }
        }
        internal static VectorResource cursor_size_we
        {
            get
            {
                return GetVectorImage("cursors.size-we.svg");
            }
        }
        internal static VectorResource cursor_size_ns
        {
            get
            {
                return GetVectorImage("cursors.size-ns.svg");
            }
        }
        internal static VectorResource cursor_zoom
        {
            get
            {
                return GetVectorImage("cursors.zoom.svg");
            }
        }
        internal static VectorResource cursor_beam
        {
            get
            {
                return GetVectorImage("cursors.beam.svg");
            }
        }
        internal static VectorResource cursor_default
        {
            get
            {
                return GetVectorImage("cursors.default.svg");
            }
        }
        #endregion
        #region shaders
        internal static string simline_frag
        {
            get
            {
                return GetString("shaders.simline.frag");
            }
        }
        internal static string simline_vert
        {
            get
            {
                return GetString("shaders.simline.vert");
            }
        }
        internal static string rider_frag
        {
            get
            {
                return GetString("shaders.rider.frag");
            }
        }
        internal static string rider_vert
        {
            get
            {
                return GetString("shaders.rider.vert");
            }
        }
        internal static string simgrid_frag
        {
            get
            {
                return GetString("shaders.simgrid.frag");
            }
        }
        internal static string simgrid_vert
        {
            get
            {
                return GetString("shaders.simgrid.vert");
            }
        }

        internal static string floatgrid_vert
        {
            get
            {
                return GetString("shaders.floatgrid.vert");
            }
        }

        internal static string floatgrid_frag
        {
            get
            {
                return GetString("shaders.floatgrid.frag");
            }
        }

        #endregion
        #region icons
        internal static System.Drawing.Bitmap pencil_icon
        {
            get
            {
                return GetBitmap("icons.penciltool.png");
            }
        }
        internal static System.Drawing.Bitmap smoothpencil_icon
        {
            get
            {
                return GetBitmap("icons.smoothpenciltool.png");
            }
        }
        internal static System.Drawing.Bitmap line_icon
        {
            get
            {
                return GetBitmap("icons.linetool.png");
            }
        }
        internal static System.Drawing.Bitmap bezier_icon
        {
            get
            {
                return GetBitmap("icons.beziertool.png");
            }
        }
        internal static System.Drawing.Bitmap eraser_icon
        {
            get
            {
                return GetBitmap("icons.erasertool.png");
            }
        }
        internal static System.Drawing.Bitmap movetool_icon
        {
            get
            {
                return GetBitmap("icons.movetool.png");
            }
        }
        internal static System.Drawing.Bitmap pantool_icon
        {
            get
            {
                return GetBitmap("icons.pantool.png");
            }
        }
        internal static System.Drawing.Bitmap menu_icon
        {
            get
            {
                return GetBitmap("icons.menu.png");
            }
        }
        internal static System.Drawing.Bitmap flag_icon
        {
            get
            {
                return GetBitmap("icons.flag.png");
            }
        }
        internal static System.Drawing.Bitmap fast_forward
        {
            get
            {
                return GetBitmap("icons.fast-forward.png");
            }
        }
        internal static System.Drawing.Bitmap rewind
        {
            get
            {
                return GetBitmap("icons.rewind.png");
            }
        }
        internal static System.Drawing.Bitmap play_icon
        {
            get
            {
                return GetBitmap("icons.play.png");
            }
        }
        internal static System.Drawing.Bitmap stop_icon
        {
            get
            {
                return GetBitmap("icons.stop.png");
            }
        }
        internal static System.Drawing.Bitmap pause
        {
            get
            {
                return GetBitmap("icons.pause.png");
            }
        }
        internal static System.Drawing.Bitmap swatch
        {
            get
            {
                return GetBitmap("icons.swatch.png");
            }
        }
        internal static System.Drawing.Bitmap generator_icon
        {
            get
            {
                return GetBitmap("icons.generator.png");
            }
        }
        internal static System.Drawing.Bitmap camera_need_reset
        {
            get
            {
                return GetBitmap("icons.cameraneedreset.png");
            }
        }
        #endregion
        #region ux
        internal static System.Drawing.Bitmap ux_flagmarker
        {
            get
            {
                return GetBitmap("ux.flagmarker.png");
            }
        }
        internal static System.Drawing.Bitmap ux_playheadmarker
        {
            get
            {
                return GetBitmap("ux.playheadmarker.png");
            }
        }
        internal static System.Drawing.Bitmap ux_loading
        {
            get
            {
                return GetBitmap("ux.loading.png");
            }
        }
        #endregion
    }
}
