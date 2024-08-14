using linerider.Drawing;
using linerider.Game;
using linerider.Utils;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace linerider.Rendering
{
    public class WellRenderer : IDisposable
    {
        private readonly GLBuffer<GenericVertex> _vbo;
        private readonly Dictionary<int, int> _lines = new Dictionary<int, int>();
        private int _vertexcounter = 0;
        private const int wellsize = 6;
        public WellRenderer()
        {
            _vbo = new GLBuffer<GenericVertex>(BufferTarget.ArrayBuffer);
            _vbo.Bind();
            _vbo.SetSize(
                LineRenderer.StartingLineCount * wellsize,
                BufferUsageHint.DynamicDraw);
            _vbo.Unbind();
        }
        public void Clear()
        {
            _lines.Clear();
            _vertexcounter = 0;
        }

        public void Draw(DrawOptions draw)
        {
            using (new GLEnableCap(EnableCap.Blend))
            {
                _vbo.Bind();
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.VertexPointer(2, VertexPointerType.Float, GenericVertex.Size, 0);
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, GenericVertex.Size, 8);
                GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexcounter);
                GL.DisableClientState(ArrayCap.ColorArray);
                GL.DisableClientState(ArrayCap.VertexArray);
                _vbo.Unbind();
            }
        }
        public void Initialize(AutoArray<GameLine> lines)
        {
            Clear();
            ResourceSync initsync = new ResourceSync();
            GenericVertex[] vertices = new GenericVertex[lines.Count * wellsize];
            _ = System.Threading.Tasks.Parallel.For(0, lines.Count, (idx) =>
              {
                  StandardLine line = (StandardLine)lines[idx];
                  GenericVertex[] well = GetWell(line);
                  for (int i = 0; i < wellsize; i++)
                  {
                      vertices[idx * wellsize + i] = well[i];
                  }
                  try
                  {
                      initsync.UnsafeEnterWrite();
                      _lines.Add(line.ID, idx * wellsize);
                  }
                  finally
                  {
                      initsync.UnsafeExitWrite();
                  }
              });
            _vertexcounter = vertices.Length;
            _vbo.Bind();
            EnsureVBOSize(vertices.Length, false);
            _vbo.SetData(vertices, 0, 0, vertices.Length);
            _vbo.Unbind();
        }
        public void AddLine(StandardLine line)
        {
            if (_lines.ContainsKey(line.ID))
            {
                LineChanged(line);
                return;
            }
            GenericVertex[] well = GetWell(line);
            _vbo.Bind();
            int vertexbase = GetVertexBase();
            _vbo.SetData(well, 0, vertexbase, wellsize);
            _vbo.Unbind();
            _lines.Add(line.ID, vertexbase);
        }
        public void LineChanged(StandardLine line)
        {
            GenericVertex[] well = GetWell(line);
            int vertexbase = _lines[line.ID];
            _vbo.Bind();
            _vbo.SetData(well, 0, vertexbase, wellsize);
            _vbo.Unbind();
        }
        public void RemoveLine(StandardLine line)
        {
            int vertexbase = _lines[line.ID];
            GenericVertex[] empty = new GenericVertex[wellsize];
            _vbo.Bind();
            _vbo.SetData(empty, 0, vertexbase, wellsize);
            _vbo.Unbind();
        }
        private int GetVertexBase()
        {
            int ret = _vertexcounter;
            _vertexcounter += wellsize;
            EnsureVBOSize(_vertexcounter);
            return ret;
        }
        private void EnsureVBOSize(int size, bool copyonresize = true)
        {
            if (size > _vbo.BufferSize)
            {
                _vbo.SetSize(size * 2, BufferUsageHint.DynamicDraw, copyonresize);
            }
        }
        public void Dispose() => _vbo.Dispose();

        public static GenericVertex[] GetWell(StandardLine line)
        {
            Angle angle = Angle.FromLine(line);

            angle.Radians += line.inv ? -1.5708 : 1.5708; //90 degrees
            Vector2d offset = angle.MovePoint(Vector2d.Zero, StandardLine.Zone);
            Color wellcolor = Color.FromArgb(80, 80, 80, 80);
            GenericVertex tl = new GenericVertex((Vector2)line.Start, wellcolor);
            GenericVertex tr = new GenericVertex((Vector2)line.End, wellcolor);
            GenericVertex bl = new GenericVertex((Vector2)(line.End + offset), wellcolor);
            GenericVertex br = new GenericVertex((Vector2)(line.Start + offset), wellcolor);
            return new GenericVertex[]
                {
                    tl, tr, bl,
                    bl, br, tl
                };
        }
    }
}