using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using Svg;
using System.Xml;
using System.Text.RegularExpressions;

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

        internal void Refresh(GameCanvas canvas)
        {
            canvas.Platform.SetCursor(Gwen.Cursors.Default);
        }
        private void AddSvgCursor(Type name, string svgRaw)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(svgRaw);

            Size size = GetSvgSize(doc);
            Point hotspot = GetSvgHotSpot(doc);

            PruneSvgDoc(doc);

            SvgDocument svg = SvgDocument.Open(doc);
            Bitmap bitmap = svg.Draw(size.Width, size.Height);

            RegisterCursor(name, bitmap, hotspot.X, hotspot.Y);
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
                doc.DocumentElement.RemoveChild(xmlNode);
                xmlNode = xmlNode2;
            }

            // Append required layers back
            cursorLayer.Attributes.RemoveNamedItem("display");
            cursorLayer.Attributes.RemoveNamedItem("class");
            if (styleNode != null)
                doc.DocumentElement.AppendChild(styleNode);
            doc.DocumentElement.AppendChild(cursorLayer);
        }

        private Point GetSvgHotSpot(XmlDocument doc)
        {
            // For some reason doc.GetElementById("...") doesn't work.
            XmlNodeList allNodes = doc.GetElementsByTagName("path");
            float scale = Settings.Computed.UIScale;
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

            hotspot = new Point()
            {
                X = (int)Math.Round(double.Parse(hotspotCoords[0]) * scale),
                Y = (int)Math.Round(double.Parse(hotspotCoords[1]) * scale),
            };

            return hotspot;
        }

        private Size GetSvgSize(XmlDocument doc)
        {
            XmlNode rootNode = doc.GetElementsByTagName("svg")[0];
            XmlNode viewBoxNode = rootNode.Attributes.GetNamedItem("viewBox");
            float scale = Settings.Computed.UIScale;
            string[] boundings = viewBoxNode.InnerText.Split(' ');

            Size size = new Size(
                (int)Math.Round(double.Parse(boundings[2]) * scale),
                (int)Math.Round(double.Parse(boundings[3]) * scale)
            );

            return size;
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

        private void RegisterCursor(Type name, Bitmap image, int hotx, int hoty)
        {
            BitmapData data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppPArgb);

            List[name] = new MouseCursor(hotx, hoty, image.Width, image.Height, data.Scan0);

            image.UnlockBits(data);
        }
    }
}
