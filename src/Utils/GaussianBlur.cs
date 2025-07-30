using SkiaSharp;
using System;

namespace linerider.Utils
{
    public class GaussianBlur
    {
        public SKBitmap Apply(SKBitmap image, double sigma, int size)
        {
            double[,] kernel = GetGaussianKernel(size, sigma);

            SKBitmap copy = image.Copy();

            int offset = size / 2;
            int xLimit = image.Width - offset;
            int yLimit = image.Height - offset;

            for (int i = offset; i < xLimit; i++)
            {
                for (int j = offset; j < yLimit; j++)
                {
                    Color newColor = ApplyKernel(image, kernel, i, j, offset);
                    copy.SetPixel(i, j, newColor);
                }
            }

            return copy;
        }

        private Color ApplyKernel(SKBitmap image, double[,] kernel, int x, int y, int offset)
        {
            double alpha = 0.0, red = 0.0, green = 0.0, blue = 0.0;

            for (int i = 0; i < kernel.GetLength(0); i++)
            {
                for (int j = 0; j < kernel.GetLength(1); j++)
                {
                    Color pixel = image.GetPixel(x - offset + i, y - offset + j);
                    alpha += pixel.A * kernel[i, j];
                    red += pixel.R * kernel[i, j];
                    green += pixel.G * kernel[i, j];
                    blue += pixel.B * kernel[i, j];
                }
            }

            return Color.FromArgb((int)alpha, (int)red, (int)green, (int)blue);
        }

        private double[,] GetGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double sum = 0.0;
            int halfSize = size / 2;

            for (int i = -halfSize; i <= halfSize; i++)
            {
                for (int j = -halfSize; j <= halfSize; j++)
                {
                    kernel[i + halfSize, j + halfSize] = 1.0 / (2 * Math.PI * sigma * sigma) * Math.Exp(-(i * i + j * j) / (2 * sigma * sigma));
                    sum += kernel[i + halfSize, j + halfSize];
                }
            }

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= sum;

            return kernel;
        }
    }
}
