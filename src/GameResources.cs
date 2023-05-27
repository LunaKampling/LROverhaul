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

using System.Drawing;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Svg;

namespace linerider
{
    internal class GameResources
    {
        public static int screensize;
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
        internal static byte[] beep
        {
            get
            {
                return GetBytes("beep.wav");
            }
        }
        internal static System.Drawing.Bitmap DefaultSkin
        {
            get
            {
                return GetBitmap("DefaultSkin.png");
            }
        }
        internal static System.Drawing.Bitmap loading
        {
            get
            {
                return GetBitmap("loading.png");
            }
        }

        internal static byte[] icon
        {
            get
            {
                return GetBytes("icon.ico");
            }
        }
        internal static string DefaultColors
        {
            get
            {
                return GetString("DefaultColors.xml");
            }
        }
        #region fonts
        internal static string liberation_sans_15_fnt
        {
            get
            {
                return GetString("fonts.liberation_sans_15.fnt");
            }
        }
        internal static System.Drawing.Bitmap liberation_sans_15_png
        {
            get
            {
                return GetBitmap("fonts.liberation_sans_15_0.png");
            }
        }
        internal static string liberation_sans_15_bold_fnt
        {
            get
            {
                return GetString("fonts.liberation_sans_15_bold.fnt");
            }
        }
        internal static System.Drawing.Bitmap liberation_sans_15_bold_png
        {
            get
            {
                return GetBitmap("fonts.liberation_sans_15_bold_0.png");
            }
        }
        #endregion
        #region rider
        internal static Bitmap sled_img
        {
            get
            {
                return GetBitmap("rider.sled.png");
            }
        }
        internal static Bitmap sledbroken_img
        {
            get
            {
                return GetBitmap("rider.sledbroken.png");
            }
        }
        internal static Bitmap arm_img
        {
            get
            {
                return GetBitmap("rider.arm.png");
            }
        }
        internal static Bitmap leg_img
        {
            get
            {
                return GetBitmap("rider.leg.png");
            }
        }
        internal static Bitmap body_img
        {
            get
            {
                return GetBitmap("rider.body.png");
            }
        }
        internal static Bitmap bodydead_img
        {
            get
            {
                return GetBitmap("rider.bodydead.png");
            }
        }
        internal static Bitmap rope_img
        {
            get
            {
                return GetBitmap("rider.rope.png");
            }
        }
        internal static string regions_file
        {
            get
            {
                return GetString("rider..regions");
            }
        }
        #endregion
        #region cursors

        
        internal static string cursor_hand
        {
            get
            {
                return GetString("cursors.hand.svg");
            }
        }
        internal static string cursor_drag_inactive
        {
            get
            {
                return GetString("cursors.drag-inactive.svg");
            }
        }
        internal static string cursor_drag_active
        {
            get
            {
                return GetString("cursors.drag-active.svg");
            }
        }

        internal static string cursor_line
        {
            get
            {
                return GetString("cursors.line.svg");
            }
        }
        internal static string cursor_eraser
        {
            get
            {
                return GetString("cursors.eraser.svg");
            }
        }
        internal static string cursor_pencil
        {
            get
            {
                return GetString("cursors.pencil.svg");
            }
        }
        internal static string cursor_size_swne
        {
            get
            {
                return GetString("cursors.size-swne.svg");
            }
        }
        internal static string cursor_size_nwse
        {
            get
            {
                return GetString("cursors.size-nwse.svg");
            }
        }
        internal static string cursor_size_we
        {
            get
            {
                return GetString("cursors.size-we.svg");
            }
        }
        internal static string cursor_size_ns
        {
            get
            {
                return GetString("cursors.size-ns.svg");
            }
        }
        internal static string cursor_zoom
        {
            get
            {
                return GetString("cursors.zoom.svg");
            }
        }
        internal static string cursor_beam
        {
            get
            {
                return GetString("cursors.beam.svg");
            }
        }
        internal static string cursor_default
        {
            get
            {
                return GetString("cursors.default.svg");
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
        #region Icons
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
        internal static System.Drawing.Bitmap flag_invalid_icon
        {
            get
            {
                return GetBitmap("icons.flag_invalid.png");
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
        #endregion
        #region ux
        internal static System.Drawing.Bitmap camera_need_reset
        {
            get
            {
                return GetBitmap("ux.cameraneedreset.png");
            }
        }
        internal static System.Drawing.Bitmap flagmarker
        {
            get
            {
                return GetBitmap("ux.flagmarker.png");
            }
        }
        internal static System.Drawing.Bitmap playheadmarker
        {
            get
            {
                return GetBitmap("ux.playheadmarker.png");
            }
        }
        #endregion
    }
}
