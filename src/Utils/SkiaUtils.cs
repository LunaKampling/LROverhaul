using SkiaSharp;
using System.IO;
using System.Xml;
using Svg.Skia;

namespace linerider {
    class SkiaUtils {
        public static void ClearBitmap(SKBitmap bitmap, SKColor clearColor) {
            SKColor[] colors = new SKColor[bitmap.Pixels.Length];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = clearColor;
            }
            bitmap.Pixels = colors;
        }
        public static SKBitmap LoadSVG(string xml, int width = -1, int height = -1)
        {
            var svg = new SKSvg();

            using (XmlReader xmlr = XmlReader.Create(new StringReader(xml)))
                _ = svg.Load(xmlr);

            if (width == -1)
                width = (int)svg.Picture.CullRect.Width;
            if (height == -1)
                height = (int)svg.Picture.CullRect.Height;

            SKBitmap toBitmap = new SKBitmap(
                (int)svg.Picture.CullRect.Width,
                (int)svg.Picture.CullRect.Height);

            using (SKCanvas canvas = new SKCanvas(toBitmap))
            {
                canvas.Clear(SKColors.Transparent);
                canvas.DrawPicture(svg.Picture);
                canvas.Flush();
            }

            return toBitmap;
        }

    }
}