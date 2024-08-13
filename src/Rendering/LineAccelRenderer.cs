using linerider.Drawing;
using linerider.Game;
using linerider.Utils;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace linerider.Rendering
{
    public class LineAccelRenderer : IDisposable
    {
        private const int ShapeSize = 3;
        private struct accelentry
        {
            public int start;
            public int shapes;
        }
        private readonly AutoArray<int> _indices = new AutoArray<int>(LineRenderer.StartingLineCount / 4 * ShapeSize);
        private readonly Dictionary<int, accelentry> _lookup = new Dictionary<int, accelentry>();
        private readonly Queue<int> _freeverts = new Queue<int>();
        private int _vertcount = 0;
        private const int nullindex = 0;
        private readonly GLBuffer<GenericVertex> _accelbuffer;
        private readonly GLBuffer<int> _accelibo;
        public LineAccelRenderer()
        {
            _accelbuffer = new GLBuffer<GenericVertex>(BufferTarget.ArrayBuffer);
            _accelbuffer.Bind();
            _accelbuffer.SetSize(LineRenderer.StartingLineCount / 2 * 3, BufferUsageHint.DynamicDraw, false);
            _accelbuffer.Unbind();
            _accelibo = new GLBuffer<int>(BufferTarget.ElementArrayBuffer);
            _accelibo.Bind();
            _accelibo.SetSize(LineRenderer.StartingLineCount / 2 * 3, BufferUsageHint.DynamicDraw, false);
            _accelibo.Unbind();
        }

        public void Initialize(List<RedLine> lines)
        {
            Clear();
            if (lines.Count == 0)
                return;
            // Max size for init
            GenericVertex[][] redshapes = new GenericVertex[lines.Count * ShapeSize * 3][];
            ResourceSync initsync = new ResourceSync();
            int vertcount = 0;
            _ = System.Threading.Tasks.Parallel.For(0, lines.Count,
            (idx) =>
            {
                GenericVertex[] acc = GetAccelDecor(lines[idx]);
                redshapes[idx] = acc;
                _ = System.Threading.Interlocked.Add(ref vertcount, acc.Length);
            });
            GenericVertex[] verts = new GenericVertex[vertcount];
            _indices.EnsureCapacity(vertcount);
            _indices.UnsafeSetCount(vertcount);
            int shapepos = 0;
            for (int idx = 0; idx < lines.Count; idx++)
            {
                GenericVertex[] acc = redshapes[idx];
                accelentry entry = new accelentry()
                {
                    start = shapepos,
                    shapes = acc.Length / ShapeSize
                };
                for (int i = 0; i < acc.Length; i++)
                {
                    verts[shapepos] = acc[i];
                    _indices.unsafe_array[shapepos] = shapepos;
                    shapepos++;
                }
                _lookup.Add(lines[idx].ID, entry);
            }

            _vertcount = verts.Length;
            _accelbuffer.Bind();
            EnsureVBOSize(verts.Length, false);
            _accelbuffer.SetData(verts, 0, 0, verts.Length);
            _accelbuffer.Unbind();
            _accelibo.Bind();
            EnsureIBOSize(_indices.Count, false);
            _accelibo.SetData(_indices.unsafe_array, 0, 0, _indices.Count);
            _accelibo.Unbind();
        }
        public void Draw(DrawOptions draw)
        {
            _accelbuffer.Bind();

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.VertexPointer(2, VertexPointerType.Float, GenericVertex.Size, 0);
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, GenericVertex.Size, 8);

            _accelibo.Bind();
            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
            _accelibo.Unbind();

            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.VertexArray);

            _accelbuffer.Unbind();
        }
        public void Clear()
        {
            _vertcount = 0;
            _indices.Clear();
            _freeverts.Clear();
            _lookup.Clear();
        }
        public void AddLine(RedLine line)
        {
            if (_lookup.ContainsKey(line.ID))
            {
                LineChanged(line, false);
                return;
            }
            _lookup.Add(
                line.ID,
                new accelentry()
                {
                    start = _indices.Count,
                    shapes = 0
                });
            DrawAccel(line, false);

        }
        public void LineChanged(RedLine line, bool hit) => DrawAccel(line, hit);
        public void RemoveLine(RedLine line)
        {
            accelentry accel = _lookup[line.ID];
            for (int ix = 0; ix < accel.shapes; ix++)
            {
                int offset = accel.start + ix * ShapeSize;
                if (IsNulled(offset))
                {
                    continue;
                }
                _freeverts.Enqueue(_indices.unsafe_array[offset]);
                for (int i = 0; i < ShapeSize; i++)
                {
                    int index = accel.start + ix * ShapeSize + i;
                    _indices.unsafe_array[index] = nullindex;
                }
            }
            _accelibo.Bind();
            _accelibo.SetData(
                _indices.unsafe_array,
                accel.start,
                accel.start,
                accel.shapes * ShapeSize);
            _accelibo.Unbind();
        }
        /// <summary>
        /// Redraws the red line accel indicator.
        /// </summary>
        private void DrawAccel(RedLine line, bool hide)
        {
            accelentry entry = _lookup[line.ID];
            GenericVertex[] newdecor = hide ? new GenericVertex[(int)Math.Round(ShapeSize * line.Multiplier)] : GetAccelDecor(line);
            int shapes = newdecor.Length / ShapeSize;

            for (int ix = 0; ix < entry.shapes; ix++)
            {
                int offset = entry.start + ix * ShapeSize;
                if (IsNulled(offset))
                {
                    continue; // Nulled out
                }
                _freeverts.Enqueue(_indices.unsafe_array[offset]);
                for (int i = 0; i < ShapeSize; i++)
                {
                    _indices.unsafe_array[offset + i] = nullindex;
                }
            }
            bool growing = shapes > entry.shapes;
            _accelbuffer.Bind();
            for (int ix = 0; ix < shapes; ix++)
            {
                int vertexbase = GetVertexBase();
                _accelbuffer.SetData(
                    newdecor,
                    ShapeSize * ix,
                    vertexbase,
                    ShapeSize);
                for (int i = 0; i < ShapeSize; i++)
                {
                    if (growing)
                    {
                        _indices.Add(vertexbase + i);
                    }
                    else
                    {
                        int offset = entry.start + ix * ShapeSize + i;
                        _indices.unsafe_array[offset] = vertexbase + i;
                    }
                }
            }
            _accelbuffer.Unbind();

            _accelibo.Bind();
            _accelibo.SetData(
                _indices.unsafe_array,
                entry.start,
                entry.start,
                entry.shapes * ShapeSize);
            if (growing)
            {
                int startindex = _indices.Count - shapes * ShapeSize;
                EnsureIBOSize(_indices.Count);
                _accelibo.SetData(
                    _indices.unsafe_array,
                    startindex,
                    startindex,
                    shapes * ShapeSize);
                _lookup[line.ID] = new accelentry()
                {
                    shapes = shapes,
                    start = startindex
                };
            }
            _accelibo.Unbind();
        }
        private int GetVertexBase()
        {
            if (_freeverts.Count != 0)
                return _freeverts.Dequeue();
            int ret = _vertcount;
            _vertcount += ShapeSize;
            EnsureVBOSize(_vertcount);
            return ret;
        }
        private void EnsureVBOSize(int size, bool copyonresize = true)
        {
            if (size > _accelbuffer.BufferSize)
            {
                // Double the buffer size. this is expensive, so avoid doing it
                // as much as possible
                _accelbuffer.SetSize(size * 2, BufferUsageHint.DynamicDraw, copyonresize);
            }
        }
        private void EnsureIBOSize(int size, bool copyonresize = true)
        {
            if (size > _accelibo.BufferSize)
            {
                // Double the buffer size. this is expensive, so avoid doing it
                // as much as possible
                _accelibo.SetSize(size * 2, BufferUsageHint.DynamicDraw, copyonresize);
            }
        }
        /// <summary>
        /// Checks if a line in the index buffer was 'removed'
        /// basically nulled out
        /// </summary>
        private bool IsNulled(int index)
        {
            for (int i = 0; i < ShapeSize; i++)
            {
                if (_indices.unsafe_array[index + i] != nullindex)
                    return false;
            }
            return true;
        }
        public void Dispose() => _accelbuffer.Dispose();
        public static GenericVertex[] GetAccelDecor(RedLine line)
        {
            Color linecolor = Settings.Colors.AccelerationLine;
            double multiplier = line.Multiplier;
            int shapecount = (int)Math.Round(multiplier, 3);
            if (multiplier > 3)
                shapecount += 4;
            GenericVertex[] ret = new GenericVertex[3 * shapecount];
            Angle angle = Angle.FromLine(line.End, line.Start);
            Angle angle2 = Angle.FromRadians(angle.Radians - 1.5708f);
            Vector2d start = line.Position2;
            for (int idx = 0; idx < Math.Min(shapecount, 3); idx++)
            {
                Vector2d a = start;
                Vector2d b = angle.MovePoint(start, line.inv ? -8 : 8);
                Vector2d c = angle2.MovePoint(b, 8);
                ret[idx * 3 + 0] = new GenericVertex((Vector2)a, linecolor);
                ret[idx * 3 + 1] = new GenericVertex((Vector2)b, linecolor);
                ret[idx * 3 + 2] = new GenericVertex((Vector2)c, linecolor);
                if (idx + 1 < multiplier)
                    start = angle.MovePoint(start, line.inv ? -2 : 2);
            }

            if (multiplier > 3)
            {
                // Draw White plus
                Vector2d a = angle.MovePoint(start, line.inv ? -2 : 2);
                Vector2d b = angle2.MovePoint(a, 2);
                Vector2d c = angle2.MovePoint(b, 3);

                Vector2[] tall = Utility.GetThickLine(
                    (Vector2)b, (Vector2)c, Angle.FromVector(c - b), 0.5f);
                Vector2[] tess = Utility.TesselateThickLine(tall);
                for (int i = 0; i < tess.Length; i++)
                {
                    ret[3 * 3 + i] = new GenericVertex(tess[i], Color.White);
                }

                a = angle2.MovePoint(b, 1.5);
                b = angle.MovePoint(a, -1.5);
                c = angle.MovePoint(a, 1.5);

                tall = Utility.GetThickLine(
                    (Vector2)b, (Vector2)c, Angle.FromVector(c - b), 0.5f);
                tess = Utility.TesselateThickLine(tall);
                for (int i = 0; i < tess.Length; i++)
                {
                    ret[5 * 3 + i] = new GenericVertex(tess[i], Color.White);
                }
            }
            return ret;
        }
    }
}