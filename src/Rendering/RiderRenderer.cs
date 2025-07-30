using linerider.Drawing;
using linerider.Drawing.RiderModel;
using linerider.Game;
using linerider.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace linerider.Rendering
{
    public class RiderRenderer
    {
        private readonly AutoArray<RiderVertex> Array = new(500);
        public float Scale = 1.0f;
        private readonly Shader _shader;
        private readonly GLBuffer<RiderVertex> _vbo;
        private readonly LineVAO _lines = new();
        private readonly LineVAO _momentumvao = new();
        public RiderRenderer()
        {
            _shader = Shaders.RiderShader;
            _vbo = new GLBuffer<RiderVertex>(BufferTarget.ArrayBuffer);
        }
        public void DrawMomentum(Rider rider, float opacity)
        {
            Color color = Constants.MomentumVectorColor;
            color = ChangeOpacity(color, opacity);
            for (int i = 0; i < rider.Body.Length; i++)
            {
                SimulationPoint anchor = rider.Body[i];
                Vector2d vec1 = anchor.Location;
                _ = vec1 + anchor.Momentum;
                Line line = Line.FromAngle(
                    vec1,
                    Angle.FromVector(anchor.Momentum),
                    2);
                _momentumvao.AddLine(
                    GameDrawingMatrix.ScreenCoordD(line.Position1),
                    GameDrawingMatrix.ScreenCoordD(line.Position2),
                    color,
                    GameDrawingMatrix.Scale / 2.5f);
            }
        }
        public void DrawContacts(Rider rider, List<int> diagnosis, float opacity, int subiteration = RiderConstants.Subiterations)
        {
            diagnosis ??= [];
            Bone[] bones = RiderConstants.Bones;
            for (int i = 0; i < bones.Length; i++)
            {
                Color constraintcolor = bones[i].OnlyRepel
                ? Constants.ConstraintRepelColor
                : Constants.ConstraintColor;

                float sizemult = 1;

                if (subiteration != RiderConstants.Subiterations)
                {
                    if (i > subiteration)
                    {
                        constraintcolor = Color.Gray;
                        sizemult = 0.5f;
                    }

                    if (i < subiteration)
                    {
                        constraintcolor = Color.Lime;
                        sizemult = 0.5f;
                    }

                    if (i == subiteration)
                    {
                        constraintcolor = Color.Cyan;
                    }
                }

                constraintcolor = ChangeOpacity(constraintcolor, opacity);

                if (bones[i].Breakable && subiteration == RiderConstants.Subiterations)
                {
                    continue;
                }
                else if (bones[i].OnlyRepel && subiteration == RiderConstants.Subiterations)
                {
                    _lines.AddLine(
                        GameDrawingMatrix.ScreenCoordD(rider.Body[bones[i].joint1].Location),
                        GameDrawingMatrix.ScreenCoordD(rider.Body[bones[i].joint2].Location),
                        constraintcolor,
                        GameDrawingMatrix.Scale / 4 * sizemult);
                }
                else if (i <= 3 || subiteration != RiderConstants.Subiterations)
                {
                    _lines.AddLine(
                        GameDrawingMatrix.ScreenCoordD(rider.Body[bones[i].joint1].Location),
                        GameDrawingMatrix.ScreenCoordD(rider.Body[bones[i].joint2].Location),
                        constraintcolor,
                        GameDrawingMatrix.Scale / 4 * sizemult);
                }
            }
            if (!rider.Crashed && diagnosis.Count != 0)
            {
                Color firstbreakcolor = Constants.ConstraintFirstBreakColor;
                Color breakcolor = Constants.ConstraintBreakColor;
                breakcolor = ChangeOpacity(breakcolor, opacity / 2);
                firstbreakcolor = ChangeOpacity(firstbreakcolor, opacity);
                for (int i = 1; i < diagnosis.Count; i++)
                {
                    int broken = diagnosis[i];
                    if (broken >= 0)
                    {
                        _lines.AddLine(
                            GameDrawingMatrix.ScreenCoordD(rider.Body[bones[broken].joint1].Location),
                            GameDrawingMatrix.ScreenCoordD(rider.Body[bones[broken].joint2].Location),
                            breakcolor,
                            GameDrawingMatrix.Scale / 4);
                    }
                }
                // The first break is most important so we give it a better color, assuming its not just a fakie death
                if (diagnosis[0] > 0)
                {
                    _lines.AddLine(
                        GameDrawingMatrix.ScreenCoordD(rider.Body[bones[diagnosis[0]].joint1].Location),
                        GameDrawingMatrix.ScreenCoordD(rider.Body[bones[diagnosis[0]].joint2].Location),
                        firstbreakcolor,
                        GameDrawingMatrix.Scale / 4);
                }
            }
            for (int i = 0; i < rider.Body.Length; i++)
            {
                Color c = Constants.ContactPointColor;
                if (
                    ((i == RiderConstants.SledTL || i == RiderConstants.SledBL) && diagnosis.Contains(-1)) ||
                    ((i == RiderConstants.BodyButt || i == RiderConstants.BodyShoulder) && diagnosis.Contains(-2)))
                {
                    c = Constants.ContactPointFakieColor;
                }
                c = ChangeOpacity(c, opacity);
                _lines.AddLine(
                    GameDrawingMatrix.ScreenCoordD(rider.Body[i].Location),
                    GameDrawingMatrix.ScreenCoordD(rider.Body[i].Location),
                    c,
                    GameDrawingMatrix.Scale / 4);
            }
        }
        public void DrawRider(float opacity, Rider rider, bool scarf = false)
        {
            if (scarf)
            {
                DrawScarf(rider.GetScarfLines(), opacity);
            }
            ImmutablePointCollection points = rider.Body;
            DrawTexture(
                 Tex.Leg,
                 Models.LegRect,
                 Models.LegUV,
                 points[RiderConstants.BodyButt].Location,
                 points[RiderConstants.BodyFootRight].Location,
                 opacity);

            DrawTexture(
                Tex.Arm,
                Models.ArmRect,
                Models.ArmUV,
                points[RiderConstants.BodyShoulder].Location,
                points[RiderConstants.BodyHandRight].Location,
                opacity);
            if (!rider.Crashed)
                DrawLine(
                    points[RiderConstants.BodyHandRight].Location,
                    points[RiderConstants.SledTR].Location,
                    ChangeOpacity(Models.RopeColor, opacity),
                    Models.RopeThickness);

            if (rider.SledBroken)
            {
                Vector2d nose = points[RiderConstants.SledTR].Location - points[RiderConstants.SledTL].Location;
                Vector2d tail = points[RiderConstants.SledBL].Location - points[RiderConstants.SledTL].Location;
                if (nose.X * tail.Y - nose.Y * tail.X < 0)
                {
                    FloatRect olduv = Models.SledBrokenUV;
                    // We're upside down
                    DrawTexture(
                        Tex.SledBroken,
                        Models.SledBrokenRect,
                        FloatRect.FromLRTB(
                            olduv.Left,
                            olduv.Right,
                            olduv.Bottom,
                            olduv.Top),
                        points[RiderConstants.SledBL].Location,
                        points[RiderConstants.SledBR].Location, opacity);
                }
                else
                {
                    DrawTexture(
                        Tex.SledBroken,
                        Models.SledBrokenRect,
                        Models.SledBrokenUV,
                        points[RiderConstants.SledTL].Location,
                        points[RiderConstants.SledTR].Location,
                        opacity);
                }
            }
            else
            {
                DrawTexture(
                    Tex.Sled,
                    Models.SledRect,
                    Models.SledUV,
                    points[RiderConstants.SledTL].Location,
                    points[RiderConstants.SledTR].Location,
                    opacity);
            }

            DrawTexture(
                Tex.Leg,
                Models.LegRect,
                    Models.LegUV,
                points[RiderConstants.BodyButt].Location,
                points[RiderConstants.BodyFootLeft].Location,
                opacity);
            if (!rider.Crashed)
            {
                DrawTexture(
                    Tex.Body,
                    Models.BodyRect,
                    Models.BodyUV,
                    points[RiderConstants.BodyButt].Location,
                    points[RiderConstants.BodyShoulder].Location,
                    opacity);
            }
            else
            {
                DrawTexture(
                    Tex.BodyDead,
                    Models.BodyRect,
                    Models.BodyDeadUV,
                    points[RiderConstants.BodyButt].Location,
                    points[RiderConstants.BodyShoulder].Location,
                    opacity);
            }
            if (!rider.Crashed)
                DrawLine(
                    points[RiderConstants.BodyHandLeft].Location,
                    points[RiderConstants.SledTR].Location,
                    ChangeOpacity(Models.RopeColor, opacity),
                    Models.RopeThickness);

            DrawTexture(
                Tex.Arm,
                Models.ArmRect,
                Models.ArmUV,
                points[RiderConstants.BodyShoulder].Location,
                points[RiderConstants.BodyHandLeft].Location,
                opacity);
        }
        public void Clear()
        {
            Array.UnsafeSetCount(0);
            _lines.Clear();
            _momentumvao.Clear();
        }
        protected unsafe void BeginDraw()
        {
            _vbo.Bind();
            _shader.Use();
            EnsureBufferSize(Array.Count);
            int in_vertex = _shader.GetAttrib("in_vertex");
            int in_texcoord = _shader.GetAttrib("in_texcoord");
            int in_unit = _shader.GetAttrib("in_unit");
            int in_color = _shader.GetAttrib("in_color");
            GL.EnableVertexAttribArray(in_vertex);
            GL.EnableVertexAttribArray(in_texcoord);
            GL.EnableVertexAttribArray(in_unit);
            GL.EnableVertexAttribArray(in_color);
            _vbo.SetData(Array.unsafe_array, 0, 0, Array.Count);
            int offset = 0;
            GL.VertexAttribPointer(in_vertex, 2, VertexAttribPointerType.Float, false, RiderVertex.Size, offset);
            offset += 8;
            GL.VertexAttribPointer(in_texcoord, 2, VertexAttribPointerType.Float, false, RiderVertex.Size, offset);
            offset += 8;
            GL.VertexAttribPointer(in_unit, 1, VertexAttribPointerType.Float, false, RiderVertex.Size, offset);
            offset += 4;
            GL.VertexAttribPointer(in_color, 4, VertexAttribPointerType.UnsignedByte, true, RiderVertex.Size, offset);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Models.BodyTexture);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, Models.BodyDeadTexture);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, Models.ArmTexture);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, Models.LegTexture);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, Models.SledTexture);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, Models.SledBrokenTexture);

            GL.Uniform1(_shader.GetUniform("u_bodytex"), 0);
            GL.Uniform1(_shader.GetUniform("u_bodydeadtex"), 1);
            GL.Uniform1(_shader.GetUniform("u_armtex"), 2);
            GL.Uniform1(_shader.GetUniform("u_legtex"), 3);
            GL.Uniform1(_shader.GetUniform("u_sledtex"), 4);
            GL.Uniform1(_shader.GetUniform("u_sledbrokentex"), 5);

        }
        public void DrawLines()
        {
            if (_lines.Array.Count != 0)
            {
                _lines.Scale = 1;
                _lines.Draw(PrimitiveType.Triangles);
            }
        }
        public void Draw()
        {
            BeginDraw();
            using (new GLEnableCap(EnableCap.Blend))
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, Array.Count);
            }
            EndDraw();
            if (_momentumvao.Array.Count != 0)
            {
                _momentumvao.Scale = 1;
                _momentumvao.Draw(PrimitiveType.Triangles);
            }
        }
        protected void EndDraw()
        {
            int in_vertex = _shader.GetAttrib("in_vertex");
            int in_texcoord = _shader.GetAttrib("in_texcoord");
            int in_unit = _shader.GetAttrib("in_unit");
            int in_color = _shader.GetAttrib("in_color");
            GL.DisableVertexAttribArray(in_vertex);
            GL.DisableVertexAttribArray(in_texcoord);
            GL.DisableVertexAttribArray(in_unit);
            GL.DisableVertexAttribArray(in_color);

            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // End on unit 0
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            _vbo.Unbind();
            _shader.Stop();
        }
        private void DrawTexture(
            Tex tex,
            DoubleRect rect,
            FloatRect uv,
            Vector2d origin,
            Vector2d rotationAnchor,
            float opacity,
            bool pretty = false
            )
        {
            Angle angle = Angle.FromLine(origin, rotationAnchor);
            Vector2d[] t = Utility.RotateRect(rect, Vector2d.Zero, angle);
            Vector2d[] transform = [
            t[0] + origin,// 0 tl
            t[1] + origin,// 1 tr
            t[2] + origin,  // 2 br
            t[3] + origin,  // 3 bl
            ];
            Vector2[] texrect = GameDrawingMatrix.ScreenCoords(transform);
            Color color = Color.FromArgb((int)(255f * opacity), Color.White);
            Color[] colors = [color, color, color, color];
            if (pretty)
            {
                Random random = new(Environment.TickCount / 100 % 255);
                for (int i = 0; i < colors.Length; i++)
                {
                    bool redness = random.Next() % 2 == 0;
                    bool blueness = random.Next() % 2 == 0;
                    int random1 = Math.Max(200, random.Next() % 255);
                    int red = Math.Min(255, redness ? (random1 * 2) : (random1 / 2));
                    int green = Math.Min(255, (blueness && redness) ? random1 : (random1 / 2));
                    int blue = Math.Min(255, blueness ? (random1 * 2) : (random1 / 2));

                    colors[i] = Color.FromArgb((int)(255f * opacity), red, green, blue);
                }
            }

            _ = uv.Left;
            _ = uv.Top;
            _ = uv.Right;
            _ = uv.Bottom;
            RiderVertex[] verts = [
                new(texrect[0],  new Vector2(uv.Left, uv.Top),tex,colors[0]),
                new(texrect[1],  new Vector2(uv.Right, uv.Top),tex,colors[1]),
                new(texrect[2],  new Vector2(uv.Right, uv.Bottom),tex,colors[2]),
                new(texrect[3],  new Vector2(uv.Left, uv.Bottom),tex,colors[3])
            ];
            Array.Add(verts[0]);
            Array.Add(verts[1]);
            Array.Add(verts[2]);

            Array.Add(verts[3]);
            Array.Add(verts[2]);
            Array.Add(verts[0]);
        }
        private void DrawLine(Vector2d p1, Vector2d p2, Color Color, float size) => DrawLine(p1, p2, Utility.ColorToRGBA_LE(Color), size);
        private Vector2[] DrawLine(Vector2d p1, Vector2d p2, int color, float size)
        {
            Vector2d[] calc = Utility.GetThickLine(p1, p2, Angle.FromLine(p1, p2), size);
            Vector2[] t = GameDrawingMatrix.ScreenCoords(calc);
            RiderVertex[] verts = [
                RiderVertex.NoTexture(t[0],color),
                RiderVertex.NoTexture(t[1],color),
                RiderVertex.NoTexture(t[2],color),
                RiderVertex.NoTexture(t[3],color)
            ];
            Array.Add(verts[0]);
            Array.Add(verts[1]);
            Array.Add(verts[2]);

            Array.Add(verts[3]);
            Array.Add(verts[2]);
            Array.Add(verts[0]);
            return t;
        }
        private void DrawScarf(Line[] lines, float opacity)
        {
            if (ScarfColors.AreEmpty())
                ScarfColors.Add(0xffffff, 0);

            List<int> colorSegments = ScarfColors.GetColorList();
            List<byte> opacitySegments = ScarfColors.GetOpacityList();
            int segmentsCount = ScarfColors.Count();
            int c;
            int alt;
            int scarfPart;

            List<Vector2> altvectors = [];
            for (int i = 0; i < lines.Length; i += 2)
            {
                scarfPart = (i % segmentsCount + (segmentsCount - 1)) % segmentsCount;
                c = Utility.ColorToRGBA_LE(colorSegments[scarfPart], (byte)(opacitySegments[scarfPart] * opacity));

                Vector2[] verts = DrawLine(lines[i].Position1, lines[i].Position2, c, 2);

                if (i != 0)
                {
                    altvectors.Add(verts[0]);
                    altvectors.Add(verts[1]);
                }
                altvectors.Add(verts[2]);
                altvectors.Add(verts[3]);
            }
            for (int i = 0; i < altvectors.Count - 4; i += 4)
            {
                scarfPart = i / 2 % segmentsCount;
                alt = Utility.ColorToRGBA_LE(colorSegments[scarfPart], (byte)(opacitySegments[scarfPart] * opacity));
                RiderVertex[] verts = [
                    RiderVertex.NoTexture(altvectors[i + 0],alt),
                    RiderVertex.NoTexture(altvectors[i + 1],alt),
                    RiderVertex.NoTexture(altvectors[i + 2],alt),
                    RiderVertex.NoTexture(altvectors[i + 3],alt)
                ];
                Array.Add(verts[0]);
                Array.Add(verts[1]);
                Array.Add(verts[2]);

                Array.Add(verts[3]);
                Array.Add(verts[2]);
                Array.Add(verts[0]);
            }
        }
        private void EnsureBufferSize(int size)
        {
            if (_vbo.BufferSize < size)
            {
                _vbo.SetSize(size * 2, BufferUsageHint.StreamDraw);
            }
        }
        private Color ChangeOpacity(Color c, float opacity) => Color.FromArgb((int)(opacity * c.A), c);
        private enum Tex
        {
            None = 0,
            Body = 1,
            BodyDead = 2,
            Arm = 3,
            Leg = 4,
            Sled = 5,
            SledBroken = 6,
        }
        /// <summary>
        /// A vertex meant for the simulation line shader
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RiderVertex(Vector2 position, Vector2 uv, RiderRenderer.Tex unit, Color Color)
        {
            public static readonly int Size = Marshal.SizeOf(typeof(RiderVertex));
            public Vector2 Position = position;
            public Vector2 tex_coord = uv;
            public float texture_unit = (float)unit;
            public int color = Utility.ColorToRGBA_LE(Color);

            public static RiderVertex NoTexture(Vector2 position, int color)
            {
                RiderVertex ret = new()
                {
                    color = color,
                    Position = position,
                    texture_unit = (float)Tex.None
                };
                return ret;
            }
        }
    }
}