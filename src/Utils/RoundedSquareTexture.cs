using Gwen;
using Gwen.Skin.Texturing;
using SkiaSharp;

namespace linerider.Utils
{
    public class RoundedSquareTexture
    {
        private Texture _texture;
        private SKBitmap _bitmap;
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
        public SKBitmap Bitmap => _bitmap;

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

            Margin bounds = new(
                _stretchHorizontally ? flatAreaStart : _width,
                _stretchVertically ? flatAreaStart : _height,
                _stretchHorizontally ? flatAreaEnd : 0,
                _stretchVertically ? flatAreaEnd : 0
            );

            Texture tx = new(Gwen.Skin.SkinBase.DefaultSkin.Renderer);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, _bitmap);

            _texture = tx;
            _bordered = new Bordered(_texture, 0, 0, _width, _height, bounds);
        }

        private SKBitmap GenerateBitmap()
        {
            int radius = _stretchHorizontally || _stretchVertically ? Radius - 1 : Radius;

            SKBitmap bitmap = new(_width + radius, _height + radius);
            SKPaint paint = new()
            {
                Color = Color.White
            };

            using (var surface = new SKCanvas(bitmap))
            {
                surface.DrawRoundRect(new Rectangle(0, 0, _width, _height), radius, radius, paint);
            }

            return bitmap;
        }
    }
}
