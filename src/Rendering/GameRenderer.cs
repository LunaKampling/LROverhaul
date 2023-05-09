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

using linerider.Tools;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using linerider.Game;
using linerider.Utils;
using linerider.Drawing;
using System.IO;
using System.Linq;

namespace linerider.Rendering
{
    public static class GameRenderer
    {
        public static MainWindow Game;
        private static LineVAO _linevao = null;

        public static void DrawTrackLine(StandardLine line, Color color, bool drawwell, bool drawcolor)
        {
            var lv = new AutoArray<LineVertex>(24);
            var verts = new AutoArray<GenericVertex>(30);
            if (drawcolor)
            {
                if (line is RedLine redline)
                {
                    verts.AddRange(LineAccelRenderer.GetAccelDecor(redline));
                }
                lv.AddRange(LineColorRenderer.CreateDecorationLine(line, line.Color));
            }
            lv.AddRange(
                LineRenderer.CreateTrackLine(
                    line.Start,
                    line.End,
                    line.Width * 2,
                    Utility.ColorToRGBA_LE(color)));
            if (drawwell)
            {
                verts.AddRange(WellRenderer.GetWell(line));
            }
            var vao = GetLineVAO();
            vao.Scale = Game.Track.Zoom;
            foreach (var v in lv.unsafe_array)
            {
                vao.AddVertex(v);
            }
            GameDrawingMatrix.Enter();
            using (new GLEnableCap(EnableCap.Blend))
            {
                if (verts.Count != 0)
                {
                    GenericVAO gvao = new GenericVAO();
                    foreach (var v in verts.unsafe_array)
                    {
                        gvao.AddVertex(v);
                    }
                    gvao.Draw(PrimitiveType.Triangles);
                }
                vao.Draw(PrimitiveType.Triangles);
            }
            GameDrawingMatrix.Exit();
        }
        private static LineVAO GetLineVAO()
        {
            if (_linevao == null)
                _linevao = new LineVAO();
            _linevao.Clear();
            return _linevao;
        }
        public static void DrawKnob(
            Vector2d position,
            bool highlight,
            bool lifelock,
            float linewidth,
            float growratio)
        {
            var knobdefault = Constants.DefaultKnobColor;
            var knobsize = (highlight ? (0.8f + (0.1f * growratio)) : 0.8f);
            var size = linewidth * 2 * knobsize;
            var color = knobdefault;
            if (lifelock)
                color = Color.FromArgb(0xff, 0x00, 0x00);
            else if (highlight)
                color = Color.FromArgb(0x70, 0x6B, 0x75);
            GameRenderer.RenderRoundedLine(
                position,
                position,
                color,
                (size));
        }
        public static void RenderRoundedLine(Vector2d position, Vector2d position2, Color color, float thickness, bool knobs = false, bool redknobs = false)
        {
            using (new GLEnableCap(EnableCap.Blend))
            {
                using (new GLEnableCap(EnableCap.Texture2D))
                {
                    GameDrawingMatrix.Enter();
                    var vao = GetLineVAO();
                    vao.Scale = GameDrawingMatrix.Scale;
                    vao.AddLine(position, position2, color, thickness);
                    vao.knobstate = knobs ? (redknobs ? 2 : 1) : 0;
                    vao.Draw(PrimitiveType.Triangles);
                    GameDrawingMatrix.Exit();
                }
            }
        }
        public static void RenderRoundedRectangle(DoubleRect rect, Color color, float thickness, bool gamecoords = true)
        {
            using (new GLEnableCap(EnableCap.Blend))
            {
                using (new GLEnableCap(EnableCap.Texture2D))
                {
                    if (gamecoords)
                        GameDrawingMatrix.Enter();
                    var vao = GetLineVAO();
                    vao.Scale = GameDrawingMatrix.Scale;
                    var vec1 = rect.Vector;
                    var vec2 = vec1 + new Vector2d(rect.Width, 0);
                    var vec3 = vec1 + rect.Size;
                    var vec4 = vec1 + new Vector2d(0, rect.Height);
                    vao.AddLine(vec1, vec2, color, thickness);
                    vao.AddLine(vec2, vec3, color, thickness);
                    vao.AddLine(vec3, vec4, color, thickness);
                    vao.AddLine(vec4, vec1, color, thickness);
                    vao.knobstate = 0;
                    vao.Draw(PrimitiveType.Triangles);
                    if (gamecoords)
                        GameDrawingMatrix.Exit();
                }
            }
        }
        public static void DbgDrawCamera()
        {
            GL.PushMatrix();
            var center = new Vector2(Game.RenderSize.Width / 2, Game.RenderSize.Height / 2);
            var rect = Game.Track.Camera.getclamp(1, Game.RenderSize.Width, Game.RenderSize.Height);

            rect.Width *= Game.Track.Zoom;
            rect.Height *= Game.Track.Zoom;
            var circle = StaticRenderer.GenerateEllipse((float)rect.Width, (float)rect.Height, 100);

            var clamprect = new DoubleRect(center.X, center.Y, 0, 0);
            clamprect.Left -= rect.Width / 2;
            clamprect.Top -= rect.Height / 2;
            clamprect.Width = rect.Width;
            clamprect.Height = rect.Height;
            if (!Settings.SmoothCamera && !Settings.RoundLegacyCamera)
            {
                GL.Begin(PrimitiveType.LineStrip);
                GL.Color3(0, 0, 0);
                GL.Vertex2(clamprect.Left, clamprect.Top);
                GL.Vertex2(clamprect.Right, clamprect.Top);
                GL.Vertex2(clamprect.Right, clamprect.Bottom);
                GL.Vertex2(clamprect.Left, clamprect.Bottom);
                GL.Vertex2(clamprect.Left, clamprect.Top);
                GL.End();
                GL.PopMatrix();
                return;
            }
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(0, 0, 0);
            for (int i = 0; i < circle.Length; i++)
            {
                var pos = (Vector2d)center + (Vector2d)circle[i];
                var square = clamprect.Clamp(pos);
                var oval = clamprect.EllipseClamp(pos);
                pos = (Vector2d.Lerp(square, oval, CameraBoundingBox.roundness));
                GL.Vertex2(pos);
            }
            GL.End();
            // visualize example points being clamped
            GL.Begin(PrimitiveType.Lines);
            circle = StaticRenderer.GenerateEllipse((float)rect.Width / 1.5f, (float)rect.Height / 1.5f, 20);
            for (int i = 0; i < circle.Length; i++)
            {
                var pos = (Vector2d)center + (Vector2d)circle[i];
                var square = clamprect.Clamp(pos);
                var oval = clamprect.EllipseClamp(pos);
                pos = (Vector2d.Lerp(square, oval, CameraBoundingBox.roundness));
                if (pos != (Vector2d)center + (Vector2d)circle[i])
                {
                    GL.Vertex2(pos);
                    GL.Vertex2((Vector2d)center + (Vector2d)circle[i]);
                }
            }
            GL.End();
            GL.PopMatrix();
            //visualize rider center
            // DrawCircle(Game.Track.Camera.GetSmoothPosition(), 5, Color.Red);
            // DrawCircle(Game.Track.Camera.GetSmoothedCameraOffset(), 5, Color.Blue);
            DrawCircle(Game.Track.Timeline.GetFrame(Game.Track.Offset).CalculateCenter(), 5, Color.Green);
        }
        public static void DrawCircle(Vector2d point, float size, Color color)
        {
            GameDrawingMatrix.Enter();
            var center = (Vector2)point;
            var circ = StaticRenderer.GenerateCircle(center.X, center.Y, size, 360);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(color);
            for (int i = 0; i < circ.Length; i++)
            {
                GL.Vertex2((Vector2)circ[i]);
            }
            GL.End();
            GameDrawingMatrix.Exit();
        }
        public static void DrawBezierCurve(Vector2[] points, Color color, int resolution)
        {
            
            Vector2[] curvePoints = GenerateBezierCurve(points, resolution);
            if (points.Length > 0)
            {
                GameDrawingMatrix.Enter();
                GL.Begin(PrimitiveType.LineStrip);
                GL.Color3(color);
                for (int i = 0; i < curvePoints.Length; i++)
                {
                    GL.Vertex2(curvePoints[i]);
                }
                GL.End();
                GameDrawingMatrix.Exit();
            }
        }
        public static void RenderPoints(List<Vector2d> points, Color color, float nodeSize)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0 || i == points.Count - 1)
                {
                    DoubleRect rect = new DoubleRect(points[i].X - nodeSize, points[i].Y - nodeSize, nodeSize * 2, nodeSize * 2);
                    RenderRoundedRectangle(rect, color, 1);
                }
                else
                {
                    DrawCircle(points[i], nodeSize, color);
                }
            }
            RenderPointOutline(points, color);
        }
        private static void RenderPointOutline(List<Vector2d> points, Color color)
        {
            GameDrawingMatrix.Enter();
            GL.Begin(PrimitiveType.LineStrip);
            for (int i = 0; i < points.Count; i++)
            {
               Color col = (i < 1 || i == points.Count - 1) ? color : Color.FromArgb(255, 200, 0);
                GL.Color3(col);
                GL.Vertex2(points[i]);
            }
            GL.End();
            GameDrawingMatrix.Exit();
        }
        public static void RenderPoints(List<Vector2d> points, BezierCurve curve, Color color, float nodeSize)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0 || i == points.Count - 1)
                {
                    DoubleRect rect = new DoubleRect(points[i].X - nodeSize, points[i].Y - nodeSize, nodeSize * 2, nodeSize * 2);
                    RenderRoundedRectangle(rect, color, 1);
                }
                else
                {
                    DrawCircle(points[i], nodeSize, color);
                }                
            }
            renderPointTrace(points, curve, color);
        }
        private static void renderPointTrace(List<Vector2d> points, BezierCurve curve, Color color)
        {
            double lineLength = 0;
            List<double> lengthsPerPoint = new List<double> { 0 };
            for (int i = 1; i < points.Count; i++)
            {
                lineLength += Distance(points[i - 1], points[i]);
                lengthsPerPoint.Add(lineLength);
            }
            for (int i = 1; i < points.Count-1; i++)
            {
                Vector2 curvePoint = curve.CalculatePoint((float)lengthsPerPoint[i] / (float)lineLength);
                GameDrawingMatrix.Enter();
                GL.Begin(PrimitiveType.LineStrip);
                GL.Color3(color);
                GL.Vertex2(points[i]);
                GL.Vertex2(curvePoint);
                GL.End();
                GameDrawingMatrix.Exit();
            }
        }
        public static double Distance(Vector2d a, Vector2d b)
            => Math.Sqrt(((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y)));
        public static Vector2[] GenerateBezierCurve(Vector2[] points, int resPerHundred)
        {
            BezierCurve curve = new BezierCurve(points);
            float curveLength = curve.CalculateLength(0.1f);
            float resolution = (curveLength / 100) * resPerHundred;
            List<Vector2> curvePoints = new List<Vector2> {};

            for (int i = 0; i < resolution; i++)
            {
                float t = (float)i / resolution;
                curvePoints.Add(curve.CalculatePoint(t));
            }

            return curvePoints.ToArray();
        }

        public static Vector2[] GenerateBezierCurve(Vector2d[] points, int resPerHundred)
        {
            Vector2[] newPoints = new Vector2[points.Length];
            for(int i = 0; i < points.Length; i++)
            {
                newPoints[i] = (Vector2) points[i];
            }

            BezierCurve curve = new BezierCurve(newPoints);
            List<Vector2> curvePoints = new List<Vector2> { };
            float curveLength = curve.CalculateLength(0.1f);
            float resolution = (curveLength / 100) * resPerHundred;

            for (int i = 0; i < resolution; i++)
            {
                float t = (float)i / (float)resolution;
                curvePoints.Add(curve.CalculatePoint(t));
            }

            return curvePoints.ToArray();
        }

        public static Vector2d[] GenerateBezierCurve2d(Vector2d[] points, int resPerHundred, out BezierCurve curveOut)
        {
            Vector2[] newPoints = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                newPoints[i] = (Vector2)points[i];
            }

            BezierCurve curve = new BezierCurve(newPoints);
            curveOut = curve;
            List<Vector2d> curvePoints = new List<Vector2d> { };
            float curveLength = curve.CalculateLength(0.1f);
            float resolution = (curveLength / 100) * resPerHundred;

            for (int i = 0; i < resolution; i++)
            {
                float t = (float)i / (float)resolution;
                curvePoints.Add((Vector2d) curve.CalculatePoint(t));
            }

            return curvePoints.ToArray();
        }

        public static void DrawFloatGrid() //Draws the grid of floating-point 'regions', used in the creation of stable angled kramuals
        {
            Shader _shader = Shaders.FloatGridShader;
            _shader.Use();

            var u_zoom = _shader.GetUniform("u_zoom"); //Set uniform var used in fragment shader

            GL.Uniform1(u_zoom, Game.Track.Zoom);

            GL.PushMatrix();
            GL.Translate(new Vector3d(-Game.ScreenTranslation)); //This transforms from pixel coordinates back to world coordinates (used in vert shader)
            GL.Scale(1.0 / Game.Track.Zoom, 1.0 / Game.Track.Zoom, 0);

            GL.Begin(PrimitiveType.Quads);

            GL.Vertex2(new Vector2d(0, 0));
            GL.Vertex2(new Vector2d(Game.RenderSize.Width, 0));
            GL.Vertex2(new Vector2d(Game.RenderSize.Width, Game.RenderSize.Height));
            GL.Vertex2(new Vector2d(0, Game.RenderSize.Height));

            GL.End();
            _shader.Stop();
            GL.PopMatrix();
        }

        public static void DrawGrid_Shader(int sqsize) //Draw the grid using per-pixel shading (more efficient for low zoom where more grid-lines are needed)
        {
            Shader _shader = Shaders.SimGridShader;
            _shader.Use();

            var u_zoom = _shader.GetUniform("u_zoom"); //Set uniform var used in fragment shader
            var u_cellsize = _shader.GetUniform("u_cellsize");

            GL.Uniform1(u_zoom, Game.Track.Zoom);
            GL.Uniform1(u_cellsize, (float)sqsize); //TODO make this sync with DbgDrawGrid() cellsize

            GL.PushMatrix();
            GL.Translate(new Vector3d(-Game.ScreenTranslation)); //This transforms from pixel coordinates back to world coordinates (used in vert shader)
            GL.Scale(1.0 / Game.Track.Zoom, 1.0 / Game.Track.Zoom, 0);

            GL.Begin(PrimitiveType.Quads);

            GL.Vertex2(new Vector2d(0, 0));
            GL.Vertex2(new Vector2d(Game.RenderSize.Width, 0));
            GL.Vertex2(new Vector2d(Game.RenderSize.Width, Game.RenderSize.Height));
            GL.Vertex2(new Vector2d(0, Game.RenderSize.Height));

            GL.End();
            _shader.Stop();
            GL.PopMatrix();
        }

        public static void DbgDrawGrid()
        {
            bool fastgrid = false;
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            bool renderext = true;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            bool renderridersquare = true;
            bool useshadergrid = true;
            int sqsize = fastgrid ? EditorGrid.CellSize : SimulationGrid.CellSize;

            GL.PushMatrix();
            GL.Scale(Game.Track.Zoom, Game.Track.Zoom, 0);
            GL.Translate(new Vector3d(Game.ScreenTranslation));
            GL.Begin(PrimitiveType.Quads);
            for (var x = -sqsize; x < (Game.RenderSize.Width / Game.Track.Zoom); x += sqsize)
            {
                for (var y = -sqsize; y < (Game.RenderSize.Height / Game.Track.Zoom); y += sqsize)
                {
                    var yv = new Vector2d(x + (Game.ScreenPosition.X - (Game.ScreenPosition.X % sqsize)), y + (Game.ScreenPosition.Y - (Game.ScreenPosition.Y % sqsize)));

                    if (!fastgrid)
                    {
                        var gridpos = new GridPoint((int)Math.Floor(yv.X / sqsize), (int)Math.Floor(yv.Y / sqsize));

                        if (Game.Track.GridCheck(yv.X, yv.Y))
                        {
                            if (Game.Track.RenderRider.PhysicsBounds.ContainsPoint(gridpos))
                                GL.Color3(Color.LightSlateGray);
                            else
                                GL.Color3(Color.Yellow);
                            GL.Vertex2(yv);
                            yv.Y += sqsize;
                            GL.Vertex2(yv);
                            yv.X += sqsize;
                            GL.Vertex2(yv);
                            yv.Y -= sqsize;
                            GL.Vertex2(yv);
                        }
                        if (renderridersquare)
                        {
                            if (Game.Track.RenderRider.PhysicsBounds.ContainsPoint(gridpos))
                            {
                                GL.Color3(Color.LightGray);
                                GL.Vertex2(yv);
                                yv.Y += sqsize;
                                GL.Vertex2(yv);
                                yv.X += sqsize;
                                GL.Vertex2(yv);
                                yv.Y -= sqsize;
                                GL.Vertex2(yv);
                            }
                        }
                    }
                    else if (Game.Track.FastGridCheck(yv.X, yv.Y))
                    {
                        GL.Color3(Color.Yellow);
                        GL.Vertex2(yv);
                        yv.Y += sqsize;
                        GL.Vertex2(yv);
                        yv.X += sqsize;
                        GL.Vertex2(yv);
                        yv.Y -= sqsize;
                        GL.Vertex2(yv);
                    }
                }
            }

            GL.End();

            if (!useshadergrid)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.Red);
                for (var x = -sqsize; x < (Game.RenderSize.Width / Game.Track.Zoom); x += sqsize)
                {
                    var yv = new Vector2d(x + (Game.ScreenPosition.X - (Game.ScreenPosition.X % sqsize)), Game.ScreenPosition.Y);
                    GL.Vertex2(yv);
                    yv.Y += Game.RenderSize.Height / Game.Track.Zoom;
                    GL.Vertex2(yv);
                }
                for (var y = -sqsize; y < (Game.RenderSize.Height / Game.Track.Zoom); y += sqsize)
                {
                    var yv = new Vector2d(Game.ScreenPosition.X, y + (Game.ScreenPosition.Y - (Game.ScreenPosition.Y % sqsize)));
                    GL.Vertex2(yv);
                    yv.X += Game.RenderSize.Width / Game.Track.Zoom;
                    GL.Vertex2(yv);
                }
                GL.End();
            }
            GL.PopMatrix();
            if (useshadergrid)
            {
                DrawGrid_Shader(sqsize);
            }
        }
        public static void DrawAGWs()
        {
            bool renderext = true;

            if (renderext)
            {
                using (var trk = Game.Track.CreateTrackReader())
                {
                    foreach (var v in trk.GetLinesInRect(Game.Track.Camera.GetViewport(
                        Game.Track.Zoom,
                        Game.RenderSize.Width,
                        Game.RenderSize.Height), false))
                    {
                        if (v is StandardLine std)
                        {
                            if (std.Extension != StandardLine.Ext.None)
                            {
                                var d = std.Difference * std.ExtensionRatio;
                                if (std.Extension.HasFlag(StandardLine.Ext.Left))
                                {
                                    RenderRoundedLine(std.Position - d, std.Position, Color.Red, 1);
                                }
                                if (std.Extension.HasFlag(StandardLine.Ext.Right))
                                {
                                    RenderRoundedLine(std.Position2 + d, std.Position2, Color.Red, 1);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}