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

using linerider.Drawing;
using linerider.Utils;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using Color = System.Drawing.Color;

namespace linerider.Rendering
{
    public static class StaticRenderer
    {
        public static Vector2[] Arc(float cx, float cy, float r, float start_angle, float end_angle)
        {
            List<Vector2> ret = new List<Vector2>();
            for (float i = start_angle; i < end_angle; i += 0.05f)
            {
                ret.Add(new Vector2(cx + (float)Math.Cos(i) * r, cy + (float)Math.Sin(i) * r));
            }
            return ret.ToArray();
        }

        internal static Vector2 CalculateLine(Vector2 position, Angle angle, double length)
        {
            Vector2 ret = position;
            double radians = angle.Radians;
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);
            ret.X = (float)(ret.X + length * cos);
            ret.Y = (float)(ret.Y + length * sin);
            return ret;
        }

        internal static Vector2d CalculateLine(Vector2d position, Angle angle, double length)
        {
            Vector2d ret = position;
            double radians = angle.Radians;
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);
            ret.X += length * cos;
            ret.Y += length * sin;
            return ret;
        }

        public static Vector2[] DrawArc(float cx, float cy, float r, float start_angle, float arc_angle, int num_segments)
        {
            Vector2[] ret = new Vector2[num_segments];
            float theta = arc_angle / (num_segments - 1); // Theta is now calculated from the arc angle instead, the - 1 bit comes from the fact that the arc is open

            float tangetial_factor = (float)Math.Tan(theta);

            float radial_factor = (float)Math.Cos(theta);

            float x = r * (float)Math.Cos(start_angle); // We now start at the start angle
            float y = r * (float)Math.Sin(start_angle);

            for (int ii = 0; ii < num_segments; ii++)
            {
                ret[ii] = new Vector2((float)x + cx, (float)y + cy);

                float tx = -y;
                float ty = x;

                x += tx * tangetial_factor;
                y += ty * tangetial_factor;

                x *= radial_factor;
                y *= radial_factor;
            }
            return ret;
        }

        public static void DrawTexture(int tex, DoubleRect rect, float alpha = 1, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
        {
            GenericVAO buf = new GenericVAO();
            Vector2d tr = new Vector2d(rect.Right, rect.Top);
            Vector2d tl = new Vector2d(rect.Left, rect.Top);
            Vector2d bl = new Vector2d(rect.Left, rect.Bottom);
            Vector2d br = new Vector2d(rect.Right, rect.Bottom);
            Color c = Color.FromArgb((int)Math.Min(255, alpha * 255), 255, 255, 255);
            buf.AddVertex(new GenericVertex((Vector2)tl, c, u1, v1));
            buf.AddVertex(new GenericVertex((Vector2)tr, c, u2, v1));
            buf.AddVertex(new GenericVertex((Vector2)br, c, u2, v2));
            buf.AddVertex(new GenericVertex((Vector2)bl, c, u1, v2));
            using (new GLEnableCap(EnableCap.Texture2D))
            using (new GLEnableCap(EnableCap.Blend))
            {
                GL.BindTexture(TextureTarget.Texture2D, tex);
                buf.Draw(PrimitiveType.Quads);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }
        public static void DrawTexture(int tex, RectangleF rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1) => DrawTexture(tex, new DoubleRect(rect.Left, rect.Top, rect.Width, rect.Height), 1, u1, v1, u2, v2);

        public static Vector2d[] GenerateCircle(double cx, double cy, double r, int num_segments)
        {
            Vector2d[] ret = new Vector2d[num_segments + 1];
            double theta = 2 * Math.PI / num_segments;
            double tangetialFactor = Math.Tan(theta); // Calculate the tangential factor
            double radialFactor = Math.Cos(theta); // Calculate the radial factor
            double x = r; // We start at angle = 0
            double y = 0;
            for (int ii = 0; ii < num_segments; ii++)
            {
                ret[ii] = new Vector2d(x + cx, y + cy);
                // Calculate the tangential vector.
                // Remember, the radial vector is (x, y).
                // To get the tangential vector we flip those coordinates and negate one of them.
                double tx = -y;
                double ty = x;

                // Add the tangential vector
                x += tx * tangetialFactor;
                y += ty * tangetialFactor;

                // Correct using the radial factor
                x *= radialFactor;
                y *= radialFactor;
            }
            ret[ret.Length - 1] = ret[0];
            return ret;
        }

        public static Vector2d[] GenerateBezierCurve(int num_segments)
        {
            Vector2d[] ret = new Vector2d[num_segments + 1];
            ret[ret.Length - 1] = ret[0];
            return ret;
        }
        public static int LoadTexture(Bitmap bmp)
        {
            System.Drawing.Imaging.PixelFormat lock_format;
            switch (bmp.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    lock_format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    break;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    lock_format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    break;

                default:
                    throw new Exception("Failed to load texture");
            }

            // Create the opengl texture
            GL.GenTextures(1, out int glTex);

            GL.BindTexture(TextureTarget.Texture2D, glTex);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToBorder);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, lock_format);

            switch (lock_format)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    break;

                default:
                    // Invalid
                    break;
            }

            bmp.UnlockBits(data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return glTex;
        }

        public static Vector2[] GenerateEllipse(float radiusX, float radiusY, int segments)
        {
            Vector2[] ret = new Vector2[segments];
            double percent;
            for (int i = 0; i < segments; i++)
            {
                percent = i / (double)segments * 360.0;
                float rad = (float)MathHelper.DegreesToRadians(percent);
                ret[i] = new Vector2((float)(Math.Cos(rad) * radiusX), (float)(Math.Sin(rad) * radiusY));
            }
            ret[segments - 1] = ret[0];
            return ret;
        }
        public static Vector2[] GenerateThickLine(Vector2 p, Vector2 p1, float width) => GenerateThickLine(p, p1, Angle.FromLine(p, p1), width);
        public static Vector2[] GenerateThickLine(Vector2 p, Vector2 p1, Angle angle, float width)
        {
            FloatRect rect = new FloatRect(
                p.X - width / 2,
                p.Y,
                width,
                (p1 - p).Length);
            angle.Degrees -= 90;
            // Returns tl tr br bl of the rotated rectangle
            Vector2[] rot = Utility.RotateRect(rect, p, angle);
            // We return tr br bl tl
            return new Vector2[] { rot[1], rot[2], rot[3], rot[0] };
        }
        public static void RenderRect(FloatRect rect, Color color) => RenderRect(new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height), color);

        public static void RenderRect(RectangleF rect, Color color)
        {
            GL.Enable(EnableCap.Blend);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(color);
            GL.Vertex2(new Vector2(rect.Left, rect.Top));
            GL.Vertex2(new Vector2(rect.Left + rect.Width, rect.Top));
            GL.Vertex2(new Vector2(rect.Left + rect.Width, rect.Top + rect.Height));
            GL.Vertex2(new Vector2(rect.Left, rect.Top + rect.Height));
            GL.End();
        }
    }
}