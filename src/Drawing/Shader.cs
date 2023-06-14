using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace linerider.Drawing
{
    public class Shader : IDisposable
    {
        private readonly int _frag;
        private readonly int _vert;
        private readonly int _program;
        private readonly Dictionary<string, int> _attributes = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _uniforms = new Dictionary<string, int>();
        public Shader(string vert, string frag)
        {
            _program = GL.CreateProgram();
            _frag = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(_frag, frag);
            CompileShader(_frag);
            _vert = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(_vert, vert);
            CompileShader(_vert);

            GL.AttachShader(_program, _frag);
            GL.AttachShader(_program, _vert);
            GL.LinkProgram(_program);
            GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int linkstatus);
            if (linkstatus == 0)
            {
                throw new Exception("Shader program link error: " + GL.GetProgramInfoLog(_program));
            }
            GL.ValidateProgram(_program);
        }
        public int GetAttrib(string attributename)
        {
            if (!_attributes.TryGetValue(attributename, out int ret))
            {
                ret = GL.GetAttribLocation(_program, attributename);
                _attributes[attributename] = ret;
            }
            return ret;
        }
        public int GetUniform(string uniformname)
        {
            if (!_uniforms.TryGetValue(uniformname, out int ret))
            {
                ret = GL.GetUniformLocation(_program, uniformname);
                _uniforms[uniformname] = ret;
            }
            return ret;
        }
        public void Use() => GL.UseProgram(_program);
        public void Stop() => GL.UseProgram(0);
        private void CompileShader(int shader)
        {
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                Debug.WriteLine("Shader Error: " + log);
                throw new Exception("Shader compile error: " + log);
            }
        }
        public void Dispose()
        {
            GL.DeleteShader(_vert);
            GL.DeleteShader(_frag);
            GL.DeleteProgram(_program);
        }
    }
}
