using linerider.Utils;
using OpenTK.Windowing.Common.Input;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using SkiaSharp;

namespace linerider.UI
{
    public class CursorsHandler
    {
        public enum Type
        {
            Default,
            Pencil,
            Line,
            Eraser,
            Beam,
            Hand,
            DragInactive,
            DragActive,
            Select,
            Zoom,
            SizeWE,
            SizeNS,
            SizeSWNE,
            SizeNWSE,
        }
        private class SvgLayerName
        {
            public const string Prefix = "# ";
            public const string HdPostfix = " (HD)";
            public const string Dark = "Dark";
            public const string Light = "Light";
            public const string HotSpot = "HotSpot";
        }

        public Dictionary<Type, MouseCursor> List = new Dictionary<Type, MouseCursor>();

        public void Reload()
        {
            List.Clear();

            AddSvgCursor(Type.Default, GameResources.cursor_default);
            AddSvgCursor(Type.Pencil, GameResources.cursor_pencil);
            AddSvgCursor(Type.Line, GameResources.cursor_line);
            AddSvgCursor(Type.Eraser, GameResources.cursor_eraser);
            AddSvgCursor(Type.Hand, GameResources.cursor_hand);
            AddSvgCursor(Type.DragInactive, GameResources.cursor_drag_inactive);
            AddSvgCursor(Type.DragActive, GameResources.cursor_drag_active);
            AddSvgCursor(Type.Select, GameResources.cursor_select);
            AddSvgCursor(Type.SizeWE, GameResources.cursor_size_we);
            AddSvgCursor(Type.SizeNS, GameResources.cursor_size_ns);
            AddSvgCursor(Type.SizeSWNE, GameResources.cursor_size_swne);
            AddSvgCursor(Type.SizeNWSE, GameResources.cursor_size_nwse);
            AddSvgCursor(Type.Zoom, GameResources.cursor_zoom);
            AddSvgCursor(Type.Beam, GameResources.cursor_beam);
        }

        internal void Refresh(GameCanvas canvas) => canvas.Platform.SetCursor(Gwen.Cursors.Default);
        private void AddSvgCursor(Type name, GameResources.VectorResource res)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(res.Raw);

            Point hotspot = GetSvgHotSpot(doc);

            PruneSvgDoc(doc);

            SKBitmap bitmap = SkiaUtils.LoadSVG(doc.OuterXml);

            int shadowX = (int)Math.Round(2 * Settings.Computed.UIScale);
            int shadowY = (int)Math.Round(1 * Settings.Computed.UIScale);
            float shadowOpacity = Settings.Computed.UIScale == 1 ? 0.25f : 0.15f;
            bool applyBlur = Settings.Computed.UIScale == 1; // Blurring is a bit slow at high resolutions
            bitmap = AddShadow(bitmap, shadowX, shadowY, shadowOpacity, applyBlur);

            RegisterCursor(name, bitmap, hotspot.X, hotspot.Y);
        }
        private SKBitmap AddShadow(SKBitmap bitmap, int shiftX, int shiftY, float shadowOpacity, bool applyBlur)
        {
            GaussianBlur blur = new GaussianBlur();
            SKBitmap bitmapWithShadow = new SKBitmap(bitmap.Info);

            SKBitmap shadow = new SKBitmap(bitmap.Info);
            using (var surface = new SKCanvas(shadow))
            {
                SKPaint paint = new SKPaint {
                    ColorFilter = SKColorFilter.CreateColorMatrix(new float[] {
                        0,0,0,shadowOpacity,0,
                        0,0,0,shadowOpacity,0,
                        0,0,0,shadowOpacity,0,
                        0,0,0,shadowOpacity,0
                    })
                };

                surface.DrawBitmap(bitmap, shiftX, shiftY, paint);
            }
            if (applyBlur)
                shadow = blur.Apply(shadow, 2.0, 5);

            using (var surface = new SKCanvas(bitmapWithShadow))
            {
                surface.DrawBitmap(shadow, 0, 0);
                surface.DrawBitmap(bitmap, 0, 0);
            }

            return bitmapWithShadow;
        }

        private void PruneSvgDoc(XmlDocument doc)
        {
            XmlNodeList layerNodes = doc.GetElementsByTagName("g");
            bool preferHD = Settings.Computed.UIScale > 1;
            bool preferDark = Settings.NightMode;
            XmlNode styleNode = null;
            XmlNode cursorLayer = null;

            try
            {
                styleNode = doc.GetElementsByTagName("style")[0];
            }
            catch (Exception)
            { }

            List<XmlNode> layers = new List<XmlNode>();
            foreach (XmlNode el in layerNodes)
            {
                bool isRightLayer = GetSvgAttrId(el).StartsWith(SvgLayerName.Prefix);
                if (isRightLayer)
                    layers.Add(el);
            }

            // Find "Light" layer and upgrade to "Light (HD)" / "Dark" / "Dark (HD) if needed
            cursorLayer = layers.Find(x => GetSvgAttrId(x) == SvgLayerName.Prefix + SvgLayerName.Light);
            if (preferHD)
            {
                XmlNode layer = layers.Find(x => GetSvgAttrId(x) == SvgLayerName.Prefix + SvgLayerName.Light + SvgLayerName.HdPostfix);
                if (layer != null)
                    cursorLayer = layer;
            }
            if (preferDark)
            {
                XmlNode layer = layers.Find(x => GetSvgAttrId(x) == SvgLayerName.Prefix + SvgLayerName.Dark);
                if (layer != null)
                    cursorLayer = layer;
            }
            if (preferDark && preferHD)
            {
                XmlNode layer = layers.Find(x => GetSvgAttrId(x) == SvgLayerName.Prefix + SvgLayerName.Dark + SvgLayerName.HdPostfix);
                if (layer != null)
                    cursorLayer = layer;
            }

            bool dontChangeSvg = cursorLayer == null;
            if (dontChangeSvg)
                return;

            // Remove all unnecessary layers
            XmlNode xmlNode = doc.DocumentElement.FirstChild;
            XmlNode xmlNode2 = null;
            while (xmlNode != null)
            {
                xmlNode2 = xmlNode.NextSibling;
                _ = doc.DocumentElement.RemoveChild(xmlNode);
                xmlNode = xmlNode2;
            }

            // Append required layers back
            _ = cursorLayer.Attributes.RemoveNamedItem("display");
            _ = cursorLayer.Attributes.RemoveNamedItem("class");
            if (styleNode != null)
                _ = doc.DocumentElement.AppendChild(styleNode);
            _ = doc.DocumentElement.AppendChild(cursorLayer);
        }

        private Point GetSvgHotSpot(XmlDocument doc)
        {
            // For some reason doc.GetElementById("...") doesn't work.
            XmlNodeList allNodes = doc.GetElementsByTagName("path");
            double scale = Settings.Computed.UIScale;
            XmlNode hotspotEl = null;
            Point hotspot;

            foreach (XmlNode el in allNodes)
            {
                bool isHotspot = GetSvgAttrId(el) == SvgLayerName.Prefix + SvgLayerName.HotSpot;
                if (isHotspot)
                {
                    hotspotEl = el;
                    break;
                }
            }
            if (hotspotEl == null)
                throw new Exception("Cannot load SVG cursor (missing HotSpot layer)");

            string hotspotData = hotspotEl.Attributes.GetNamedItem("d").InnerText;
            string[] hotspotCoords = hotspotData.Replace("M", "").Split(',');
            double.TryParse(hotspotCoords[0], System.Globalization.NumberStyles.Any, Program.Culture.NumberFormat, out double rawX);
            double.TryParse(hotspotCoords[1], System.Globalization.NumberStyles.Any, Program.Culture.NumberFormat, out double rawY);

            hotspot = new Point()
            {
                X = (int)Math.Round(rawX * scale),
                Y = (int)Math.Round(rawY * scale),
            };

            return hotspot;
        }

        private string GetSvgAttrId(XmlNode el)
        {
            XmlNode idNode = el.Attributes.GetNamedItem("id");
            string id = idNode == null ? "" : idNode.InnerText;

            if (string.IsNullOrEmpty(id))
                return id;

            Match duplicatePostfixMatch = Regex.Match(id, @"_\d*_$");
            if (duplicatePostfixMatch.Success)
                id = id.Replace(duplicatePostfixMatch.Value, "");

            MatchCollection unicodeCharMatch = Regex.Matches(id, @"_x([A-F0-9]*?)_", RegexOptions.IgnoreCase);
            foreach (Match m in unicodeCharMatch)
            {
                string charCode = m.Groups[1].Value;
                string charParsed = Convert.ToChar(Convert.ToUInt32(charCode, 16)).ToString();
                id = id.Replace(m.Value, charParsed);
            }

            id = Regex.Replace(id, "_", " ");

            return id;
        }

        private void RegisterCursor(Type name, SKBitmap image, int hotx, int hoty)
        {
            List[name] = new MouseCursor(hotx, hoty, image.Width, image.Height, image.Bytes);
        }
    }
}
