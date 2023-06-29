using Gwen.Skin.Texturing;
using Gwen;
using System;
using System.Drawing;
using Svg;

namespace linerider.Utils
{
    public class RoundedSquareTexture
    {
        private Texture _texture;
        private Bitmap _bitmap;
        private Bordered _bordered;
        private int _size;
        private int _flatAreaStart => Radius - 1;
        private int _flatAreaEnd => Radius + 1;

        public int Radius
        {
            get => _size / 2;
            set
            {
                _size = value * 2;
                GenerateTexture();
            }
        }
        public Bordered Bordered => _bordered;
        public Bitmap Bitmap => _bitmap;

        public RoundedSquareTexture(int radius)
        {
            Radius = radius;
        }
        private void GenerateTexture()
        {
            _texture?.Dispose();
            _bitmap = GenerateBitmap();

            Texture tx = new Texture(Gwen.Skin.SkinBase.DefaultSkin.Renderer);
            Margin bounds = new Margin(_flatAreaStart, _flatAreaStart, _flatAreaEnd, _flatAreaEnd);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, _bitmap);

            _texture = tx;
            _bordered = new Bordered(_texture, 0, 0, _size, _size, bounds);
        }

        /// <summary>
        ///  Generates a circle-like bitmap with 1px solid area in the middle to use with Bordered
        /// </summary>
        private Bitmap GenerateBitmap()
        {
            string svg = string.Join("\n", new string[]
            {
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {_size} {_size}\">",

                @"<path d=""",
                $"  M 0 {_flatAreaStart}",
                $"  Q 0 0 {_flatAreaStart} 0",
                $"  L {_flatAreaEnd} 0",
                $"  Q {_size} 0 {_size} {_flatAreaStart}",
                $"  L {_size} {_flatAreaEnd}",
                $"  Q {_size} {_size} {_flatAreaEnd} {_size}",
                $"  L {_flatAreaStart} {_size}",
                $"  Q 0 {_size} 0 {_flatAreaEnd}",
                @""" fill=""#FFF""/>",

                "</svg>"
            });

            Bitmap bitmap = SvgDocument.FromSvg<SvgDocument>(svg).Draw(_size, _size);

            return bitmap;
        }
    }
}
