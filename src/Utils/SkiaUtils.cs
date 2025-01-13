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
        public static SKBitmap LoadSVG(string xml) => LoadSVG(xml, 1);
        public static SKBitmap LoadSVG(string xml, double scale)
        {
            var svg = new SKSvg();

            using (XmlReader xmlr = XmlReader.Create(new StringReader(xml)))
                _ = svg.Load(xmlr);

            scale *= Settings.Computed.UIScale;
            int width = (int)(svg.Picture.CullRect.Width * scale);
            int height = (int)(svg.Picture.CullRect.Height * scale);
            
            var imageInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            SKBitmap toBitmap = new SKBitmap(imageInfo);

            using (SKCanvas canvas = new SKCanvas(toBitmap))
            {
                canvas.Scale((float)scale);
                canvas.Clear(SKColors.Transparent);
                canvas.DrawPicture(svg.Picture);
                canvas.Flush();
            }

            return toBitmap;
        }

    }
}