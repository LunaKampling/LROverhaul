using linerider.Drawing;
using linerider.Game;
using linerider.Utils;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace linerider.Rendering
{
    public class LineColorRenderer : IDisposable
    {
        private Dictionary<int, int> _lines = new Dictionary<int, int>();
        private const int linesize = 6;
        private readonly LineRenderer _linebuffer;
        public LineColorRenderer()
        {
            _linebuffer = new LineRenderer(Shaders.LineShader)
            {
                OverrideColor = Color.FromArgb(0),
                OverridePriority = 0
            };
        }

        public void Initialize(AutoArray<GameLine> lines)
        {
            Clear();
            LineVertex[] vertices = new LineVertex[lines.Count * linesize];
            AutoArray<GenericVertex> redverts = new AutoArray<GenericVertex>(lines.Count / 2 * 3);
            _ = System.Threading.Tasks.Parallel.For(0, lines.Count, (idx) =>
            {
                StandardLine line = (StandardLine)lines[idx];
                LineVertex[] lineverts = CreateDecorationLine(line, line.Color);
                for (int i = 0; i < lineverts.Length; i++)
                {
                    vertices[idx * 6 + i] = lineverts[i];
                }
            });
            Dictionary<int, int> dict = _linebuffer.AddLines(lines, vertices);
            _lines = dict;
        }
        public void Draw(DrawOptions draw)
        {
            _linebuffer.Scale = draw.Zoom;
            _linebuffer.Draw();
        }
        public void Clear()
        {
            _lines.Clear();
            _linebuffer.Clear();
        }
        public void AddLine(StandardLine line)
        {
            if (_lines.ContainsKey(line.ID))
            {
                LineChanged(line, false);
                return;
            }
            Color color = line.GetColor();
            LineVertex[] lineverts = CreateDecorationLine(line, color);
            int start = _linebuffer.AddLine(lineverts);
            _lines.Add(line.ID, start);
        }
        public void LineChanged(StandardLine line, bool hit)
        {
            int colorindex = _lines[line.ID];
            Color color = line.GetColor();
            LineVertex[] lineverts = hit ? new LineVertex[6] : CreateDecorationLine(line, color);
            _linebuffer.ChangeLine(colorindex, lineverts);
        }
        public void RemoveLine(StandardLine line)
        {
            int colorindex = _lines[line.ID];
            _linebuffer.RemoveLine(colorindex);
        }
        public void Dispose() => _linebuffer.Dispose();
        public static LineVertex[] CreateDecorationLine(StandardLine line, Color color)
        {
            Vector2d slant = new Vector2d(
                line.DiffNormal.X > 0 ? Math.Ceiling(line.DiffNormal.X) : Math.Floor(line.DiffNormal.X),
                line.DiffNormal.Y > 0 ? Math.Ceiling(line.DiffNormal.Y) : Math.Floor(line.DiffNormal.Y));
            return LineRenderer.CreateTrackLine(line.Position1 + slant, line.Position2 + slant, 2, Utility.ColorToRGBA_LE(color));
        }
    }
}