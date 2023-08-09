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
        private int _width => _stretchHorizontally ? _size + 1 : _size;
        private int _height => _stretchVertically ? _size + 10 : _size;
        private bool _stretchVertically { get; set; } = true;
        private bool _stretchHorizontally { get; set; } = true;

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

        /// <summary>
        /// <para>Generates a circle-like square with 1px solid area in the middle to use with <see cref="Gwen.Skin.Texturing.Bordered"/>.</para>
        /// </summary>
        /// <param name="radius">Corners roundness in pixels</param>
        /// <param name="stretchVertically">Prevents texture distortion by extending it vertically by one pixel in the middle of the bitmap. Should be false if parent height is fixed.</param>
        /// <param name="stretchHorizontally">Prevents texture distortion by extending it horizontally by one pixel in the middle of the bitmap. Should be false if parent width is fixed.</param>
        public RoundedSquareTexture(int radius, bool stretchVertically = true, bool stretchHorizontally = true)
        {
            _stretchVertically = stretchVertically;
            _stretchHorizontally = stretchHorizontally;
            Radius = radius;
        }
        private void GenerateTexture()
        {
            _texture?.Dispose();
            _bitmap = GenerateBitmap();
            int flatAreaStart = Radius - 1;
            int flatAreaEnd = Radius + 1;

            Margin bounds = new Margin(
                _stretchHorizontally ? flatAreaStart : _width,
                _stretchVertically ? flatAreaStart : _height,
                _stretchHorizontally ? flatAreaEnd : 0,
                _stretchVertically ? flatAreaEnd : 0
            );

            Texture tx = new Texture(Gwen.Skin.SkinBase.DefaultSkin.Renderer);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, _bitmap);

            _texture = tx;
            _bordered = new Bordered(_texture, 0, 0, _width, _height, bounds);
        }

        private Bitmap GenerateBitmap()
        {
            int radius = _stretchHorizontally || _stretchVertically ? Radius - 1 : Radius;

            string svg = string.Join("\n", new string[]
            {
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {_width} {_height}\">",
                $"<rect width=\"100%\" height=\"100%\" rx=\"{radius}\" fill=\"#FFF\"/>",
                "</svg>"
            });

            Bitmap bitmap = SvgDocument.FromSvg<SvgDocument>(svg).Draw(_width, _height);

            return bitmap;
        }
    }
}
