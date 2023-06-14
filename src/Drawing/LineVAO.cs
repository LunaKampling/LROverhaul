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

using linerider.Rendering;
using linerider.Utils;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace linerider.Drawing
{
    public unsafe class LineVAO : GLArray<LineVertex>
    {
        private readonly Shader _shader;
        public float Scale = 1.0f;
        public int knobstate = 0;
        public ErrorCode Error => GL.GetError();
        public LineVAO()
        {
            _shader = Shaders.LineShader;
        }
        public void AddLine(
            Vector2d start,
            Vector2d end,
            Color color,
            float size)
        {
            Vector2d d = end - start;
            _ = Angle.FromVector(d);
            LineVertex[] line = LineRenderer.CreateTrackLine(start, end, size, Utility.ColorToRGBA_LE(color));
            foreach (LineVertex v in line)
            {
                Array.Add(v);
            }
        }
        protected override void BeginDraw()
        {
            _shader.Use();
            int in_vertex = _shader.GetAttrib("in_vertex");
            int in_color = _shader.GetAttrib("in_color");
            int in_circle = _shader.GetAttrib("in_circle");
            int in_linesize = _shader.GetAttrib("in_linesize");
            GL.EnableVertexAttribArray(in_vertex);
            GL.EnableVertexAttribArray(in_circle);
            GL.EnableVertexAttribArray(in_linesize);
            GL.EnableVertexAttribArray(in_color);
            fixed (float* ptr1 = &Array.unsafe_array[0].Position.X)
            fixed (byte* ptr2 = &Array.unsafe_array[0].u)
            fixed (float* ptr3 = &Array.unsafe_array[0].ratio)
            fixed (int* ptr4 = &Array.unsafe_array[0].color)
            {
                GL.VertexAttribPointer(in_vertex, 2, VertexAttribPointerType.Float, false, LineVertex.Size, (IntPtr)ptr1);
                GL.VertexAttribPointer(in_circle, 2, VertexAttribPointerType.Byte, false, LineVertex.Size, (IntPtr)ptr2);
                GL.VertexAttribPointer(in_linesize, 2, VertexAttribPointerType.Float, false, LineVertex.Size, (IntPtr)ptr3);
                GL.VertexAttribPointer(in_color, 4, VertexAttribPointerType.UnsignedByte, true, LineVertex.Size, (IntPtr)ptr4);
            }
            int u_color = _shader.GetUniform("u_color");
            int u_scale = _shader.GetUniform("u_scale");
            int u_knobstate = _shader.GetUniform("u_knobstate");
            int u_alphachannel = _shader.GetUniform("u_alphachannel");
            GL.Uniform4(u_color, 0f, 0f, 0f, 0f);
            GL.Uniform1(_shader.GetUniform("u_overlay"), 0);
            GL.Uniform1(u_alphachannel, 1);
            GL.Uniform1(u_scale, Scale);
            GL.Uniform1(u_knobstate, knobstate);
        }
        protected override void InternalDraw(PrimitiveType primitive)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            using (new GLEnableCap(EnableCap.Blend))
            {
                GL.DrawArrays(primitive, 0, Array.Count);
            }
        }
        protected override void EndDraw()
        {
            GL.DisableVertexAttribArray(_shader.GetAttrib("in_vertex"));
            GL.DisableVertexAttribArray(_shader.GetAttrib("in_color"));
            GL.DisableVertexAttribArray(_shader.GetAttrib("in_circle"));
            GL.DisableVertexAttribArray(_shader.GetAttrib("in_linesize"));
            _shader.Stop();
        }
    }
}