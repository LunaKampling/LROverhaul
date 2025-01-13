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

using linerider.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using SkiaSharp;

#pragma warning disable IDE1006 // Naming Styles

namespace linerider
{
    internal class GameResources
    {
        #region Init
        public class VectorResource
        {
            public string Raw;
            public Size BaseSize;
            public SKBitmap Bitmap => SkiaUtils.LoadSVG(Raw);
        }
        private static Assembly Assembly = null;
        private static Dictionary<string, object> _lookuptable = null;
        public static void Init()
        {
            if (Assembly == null)
                Assembly = typeof(GameResources).Assembly;
            if (_lookuptable == null)
                _lookuptable = new Dictionary<string, object>();
        }
        #endregion
        #region Getters
        public static SKBitmap GetBitmap(string name)
        {
            if (_lookuptable.TryGetValue(name, out object lookup))
            {
                return (SKBitmap)lookup;
            }
            using (Stream stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                // PNG spec says alpha is never premultiplied, skia assumes premultiplied anyways
                // and this is what has to be done to have it not do that
                var codec = SKCodec.Create(stream);
                var info = codec.Info;
                var imageInfo = new SKImageInfo(info.Width, info.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                SKBitmap ret = new SKBitmap(imageInfo);
                codec.GetPixels(ret.Info, ret.GetPixels());
                _lookuptable[name] = ret;
                return ret;
            }
        }
        private static byte[] GetBytes(string name)
        {
            if (_lookuptable.TryGetValue(name, out object lookup))
            {
                return ((byte[])lookup).ToArray(); // Prevent writing to resource
            }
            using (Stream stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                byte[] ret = new byte[stream.Length];
                _ = stream.Read(ret, 0, ret.Length);
                _lookuptable[name] = ret;
                return ret;
            }
        }
        private static string GetString(string name)
        {
            if (_lookuptable.TryGetValue(name, out object lookup))
            {
                return (string)lookup; // Strings are immutable so there's no chance of writing to resource
            }
            using (Stream stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string ret = reader.ReadToEnd();
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

            double.TryParse(boundings[2], System.Globalization.NumberStyles.Any, Program.Culture.NumberFormat, out double rawW);
            double.TryParse(boundings[3], System.Globalization.NumberStyles.Any, Program.Culture.NumberFormat, out double rawH);

            Size size = new Size(
                (int)Math.Round(rawW),
                (int)Math.Round(rawH)
            );

            VectorResource res = new VectorResource()
            {
                Raw = raw,
                BaseSize = size,
            };

            return res;
        }
        public static Fonts GetFont(string name)
        {
            string fnt = GetString($"{name}.fnt");
            SKBitmap bitmap = GetBitmap($"{name}_0.png");
            string fntbold = GetString($"{name}_bold.fnt");
            SKBitmap bitmapbold = GetBitmap($"{name}_bold_0.png");

            Gwen.Renderer.OpenTK renderer = new Gwen.Renderer.OpenTK();
            Gwen.Renderer.BitmapFont bitmapfont = new Gwen.Renderer.BitmapFont(renderer, fnt, renderer.CreateTexture(bitmap));
            Gwen.Renderer.BitmapFont bitmapfontbold = new Gwen.Renderer.BitmapFont(renderer, fntbold, renderer.CreateTexture(bitmapbold));
            Fonts font = new Fonts(bitmapfont, bitmapfontbold);

            return font;
        }
        #endregion
        #region Resources: Generic
        internal static byte[] beep => GetBytes("beep.wav");
        internal static SKBitmap defaultskin => GetBitmap("DefaultSkin.png");
        internal static byte[] icon => GetBytes("icon.ico");
        internal static string defaultcolors => GetString("DefaultColors.xml");
        #endregion
        #region Resources: Fonts
        internal static Fonts font_liberation_sans_15 => GetFont("fonts.liberation_sans_15");
        #endregion
        #region Resources: Rider
        internal static SKBitmap rider_sled => GetBitmap("rider.sled.png");
        internal static SKBitmap rider_sledbroken => GetBitmap("rider.sledbroken.png");
        internal static SKBitmap rider_arm => GetBitmap("rider.arm.png");
        internal static SKBitmap rider_leg => GetBitmap("rider.leg.png");
        internal static SKBitmap rider_body => GetBitmap("rider.body.png");
        internal static SKBitmap rider_bodydead => GetBitmap("rider.bodydead.png");
        internal static SKBitmap rider_rope => GetBitmap("rider.rope.png");
        internal static string rider_regions_file => GetString("rider..regions");
        #endregion
        #region Resources: Cursors
        internal static VectorResource cursor_hand => GetVectorImage("cursors.hand.svg");
        internal static VectorResource cursor_drag_inactive => GetVectorImage("cursors.drag-inactive.svg");
        internal static VectorResource cursor_drag_active => GetVectorImage("cursors.drag-active.svg");
        internal static VectorResource cursor_select => GetVectorImage("cursors.select.svg");
        internal static VectorResource cursor_line => GetVectorImage("cursors.line.svg");
        internal static VectorResource cursor_eraser => GetVectorImage("cursors.eraser.svg");
        internal static VectorResource cursor_pencil => GetVectorImage("cursors.pencil.svg");
        internal static VectorResource cursor_size_swne => GetVectorImage("cursors.size-swne.svg");
        internal static VectorResource cursor_size_nwse => GetVectorImage("cursors.size-nwse.svg");
        internal static VectorResource cursor_size_we => GetVectorImage("cursors.size-we.svg");
        internal static VectorResource cursor_size_ns => GetVectorImage("cursors.size-ns.svg");
        internal static VectorResource cursor_zoom => GetVectorImage("cursors.zoom.svg");
        internal static VectorResource cursor_beam => GetVectorImage("cursors.beam.svg");
        internal static VectorResource cursor_default => GetVectorImage("cursors.default.svg");
        #endregion
        #region Resources: Shaders
        internal static string simline_frag => GetString("shaders.simline.frag");
        internal static string simline_vert => GetString("shaders.simline.vert");
        internal static string rider_frag => GetString("shaders.rider.frag");
        internal static string rider_vert => GetString("shaders.rider.vert");
        internal static string simgrid_frag => GetString("shaders.simgrid.frag");
        internal static string simgrid_vert => GetString("shaders.simgrid.vert");
        internal static string floatgrid_vert => GetString("shaders.floatgrid.vert");
        internal static string floatgrid_frag => GetString("shaders.floatgrid.frag");
        #endregion
        #region Resources: Icons
        internal static VectorResource icon_tool_pencil => GetVectorImage("icons.tool_pencil.svg");
        internal static VectorResource icon_tool_smooth_pencil => GetVectorImage("icons.tool_smooth_pencil.svg");
        internal static VectorResource icon_tool_line => GetVectorImage("icons.tool_line.svg");
        internal static VectorResource icon_tool_bezier => GetVectorImage("icons.tool_bezier.svg");
        internal static VectorResource icon_tool_eraser => GetVectorImage("icons.tool_eraser.svg");
        internal static VectorResource icon_tool_select => GetVectorImage("icons.tool_select.svg");
        internal static VectorResource icon_tool_pan => GetVectorImage("icons.tool_pan.svg");
        internal static VectorResource icon_play => GetVectorImage("icons.play.svg");
        internal static VectorResource icon_pause => GetVectorImage("icons.pause.svg");
        internal static VectorResource icon_stop => GetVectorImage("icons.stop.svg");
        internal static VectorResource icon_flag => GetVectorImage("icons.flag.svg");
        internal static VectorResource icon_generators => GetVectorImage("icons.generators.svg");
        internal static VectorResource icon_menu => GetVectorImage("icons.menu.svg");
        internal static VectorResource icon_reset_camera => GetVectorImage("icons.reset_camera.svg");
        internal static VectorResource icon_speedup => GetVectorImage("icons.speedup.svg");
        internal static VectorResource icon_slowdown => GetVectorImage("icons.slowdown.svg");
        #endregion
        #region Resources: UX
        internal static VectorResource ux_loading => GetVectorImage("ux.loading.svg");
        internal static VectorResource ux_widget_background => GetVectorImage("ux.widget_background.svg");
        internal static VectorResource ux_tool_background => GetVectorImage("ux.tool_background.svg");
        internal static VectorResource ux_swatch => GetVectorImage("ux.swatch.svg");
        internal static VectorResource ux_swatch_active => GetVectorImage("ux.swatch_active.svg");
        internal static VectorResource ux_multitool_indicator => GetVectorImage("ux.multitool_indicator.svg");
        internal static VectorResource ux_playhead_main => GetVectorImage("ux.playhead_main.svg");
        internal static VectorResource ux_playhead_flag => GetVectorImage("ux.playhead_flag.svg");
        internal static VectorResource ux_playhead_limiter => GetVectorImage("ux.playhead_limiter.svg");
        internal static VectorResource ux_playhead_defaultzoom => GetVectorImage("ux.playhead_defaultzoom.svg");
        #endregion
    }
}
