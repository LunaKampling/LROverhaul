using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Gwen.Renderer
{
    public class BitmapFont : Font
    {
        public override int LineHeight => fontdata.LineHeight;
        public BMFont fontdata;
        public Texture texture;
        public BitmapFont(RendererBase renderer, string bmfont, Texture tx)
        : base(renderer)
        {
            fontdata = new BMFont(bmfont);
            FaceName = fontdata.Face;
            Size = fontdata.FontSize;
            texture = tx;
        }
        public override List<string> WordWrap(string input, int maxpx) => fontdata.WordWrap(input, maxpx);
    }
    /// <summary>
    /// BMFont class made for loading files created with bmfont
    /// http://www.angelcode.com/products/bmfont/)
    /// Expects the "text" output .fnt
    /// OpenGL library independent output.
    /// </summary>
    public class BMFont
    {
        private struct FontGlyph
        {
            public int id;
            public float x1;
            public float y1;
            public float x2;
            public float y2;
            public int width;
            public int height;
            public int xoffset;
            public int yoffset;
            public int xadvance;
            public sbyte[] kerning;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vertex
        {
            public int x, y;
            public float u, v;
        }
        // Just a size class.
        // Avoiding system.drawing completely.
        public struct Size
        {
            public int Width;
            public int Height;
        }
        public string Face;
        public int FontSize { get; private set; }
        public int LineHeight { get; private set; }
        //private const int charAmount = 256; // ASCII
        private const int charAmount = 65535; // Unicode
        //private const int charAmount = 200813; // Unicode + CJK
        private FontGlyph _invalid = new();
        private readonly FontGlyph[] _glyphs = new FontGlyph[charAmount];
        internal int _texWidth;
        internal int _texHeight;
        // Creates a bitmap ASCII font
        public BMFont(string bmfont)
        {
            using (StringReader sr = new(bmfont))
            {
                string fullline;
                while ((fullline = sr.ReadLine()) != null)
                {
                    string[] line = fullline.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    Queue<string> lq = new();
                    foreach (string lword in line)
                    {
                        lq.Enqueue(lword);
                    }
                    string cword = lq.Dequeue();
                    if (cword == "info")
                    {
                        string facesubstr = "face=\"";
                        int faceidx1 = fullline.IndexOf(facesubstr, StringComparison.Ordinal) + facesubstr.Length;
                        int faceidx2 = fullline.IndexOf("\"", faceidx1, StringComparison.Ordinal);

                        string sizesubstr = "size=";
                        int sizeidx1 = fullline.IndexOf(sizesubstr, StringComparison.Ordinal) + sizesubstr.Length;
                        int sizeidx2 = fullline.IndexOf(" ", sizeidx1, StringComparison.Ordinal);

                        Face = fullline.Substring(faceidx1, faceidx2 - faceidx1);
                        FontSize = ParseInt(fullline.Substring(sizeidx1, sizeidx2 - sizeidx1));
                    }
                    else if (cword == "common")
                    {
                        while (lq.Count > 0)
                        {
                            cword = lq.Dequeue();
                            string key = ExtractKey(cword);
                            string val = ExtractValue(cword);
                            switch (key)
                            {
                                case "lineHeight":
                                    LineHeight = ParseInt(val);
                                    break;
                                case "scaleW":
                                    _texWidth = ParseInt(val);
                                    break;
                                case "scaleH":
                                    _texHeight = ParseInt(val);
                                    break;
                                case "pages":
                                    if (ParseInt(val) > 1)
                                        throw new Exception("Unsupported BMFont: > 1 pages");
                                    break;
                            }
                        }
                    }
                    else if (cword == "char")
                    {
                        FontGlyph glyph = new()
                        {
                            id = 0
                        };
                        while (lq.Count > 0)
                        {
                            cword = lq.Dequeue();
                            string key = ExtractKey(cword);
                            int val = int.Parse(ExtractValue(cword));
                            switch (key)
                            {
                                case "id":
                                    glyph.id = val;
                                    break;
                                case "x":
                                    glyph.x1 = val / (float)_texWidth;
                                    break;
                                case "y":
                                    glyph.y1 = val / (float)_texHeight;
                                    break;
                                case "width":
                                    glyph.width = val;
                                    glyph.x2 = glyph.x1 + val / (float)_texWidth;
                                    break;
                                case "height":
                                    glyph.height = val;
                                    glyph.y2 = glyph.y1 + val / (float)_texHeight;
                                    break;
                                case "xoffset":
                                    glyph.xoffset = val;
                                    break;
                                case "yoffset":
                                    glyph.yoffset = val;
                                    break;
                                case "xadvance":
                                    glyph.xadvance = val;
                                    break;
                            }
                        }
                        if (glyph.id != 0)
                        {
                            if (glyph.id == -1)
                                _invalid = glyph;
                            else
                                _glyphs[glyph.id] = glyph;
                        }
                    }
                    else if (cword == "kerning")
                    {
                        int first = 0;
                        int second = 0;
                        int amount = 0;
                        while (lq.Count > 0)
                        {
                            cword = lq.Dequeue();
                            string key = ExtractKey(cword);
                            int val = int.Parse(ExtractValue(cword));
                            switch (key)
                            {
                                case "first":
                                    first = val;
                                    break;
                                case "second":
                                    second = val;
                                    break;
                                case "amount":
                                    amount = val;
                                    break;
                            }
                        }
                        if (first != 0 && second != 0 && amount != 0)
                        {
                            if (_glyphs[first].kerning == null)
                            {
                                _glyphs[first].kerning = new sbyte[charAmount];
                            }
                            FontGlyph glyph = _glyphs[first];
                            if (Math.Abs(amount) > 127)
                                throw new Exception("Unsupported kerning value");
                            glyph.kerning[second] = (sbyte)amount;
                        }
                    }
                }
            }
            for (int i = 0; i < _glyphs.Length; i++)
            {
                if (_glyphs[i].id == 0)
                {
                    switch ((char)i)
                    {
                        case '\r':
                        case '\n':
                            _glyphs[i] = CreateEmpty(i);
                            break;
                        case '\t':
                            _glyphs[i] = _glyphs[' '];
                            _glyphs[i].xadvance *= 4;
                            break;
                        default:
                            // Invalid may not be set
                            // but that's okay.
                            _glyphs[i] = _invalid;
                            break;
                    }
                }
            }
        }
        private FontGlyph CreateEmpty(int id)
        {
            FontGlyph ret = new()
            {
                id = id
            };
            return ret;
        }
        private int MeasureWordSplit(string input, int start, int px)
        {
            int width = 0;
            if (start == input.Length)
                return 0;
            for (int i = start; i < input.Length; i++)
            {
                char currentchar = input[i];
                FontGlyph glyph = _glyphs[currentchar];
                int glyphwidth = glyph.xadvance;
                if (i > 0)
                {
                    FontGlyph prev = _glyphs[input[i - 1]];
                    if (prev.kerning != null &&
                        currentchar < prev.kerning.Length)
                    {
                        sbyte ker = prev.kerning[currentchar];
                        glyphwidth += ker;
                    }
                }
                if (width + glyphwidth >= px)
                {
                    return Math.Max(1, i - start);
                }
                width += glyphwidth;
            }
            return input.Length - start;
        }
        private List<string> WrapLine(string line, int maxpx, List<int> widths)
        {
            string[] wordarr = line.Split(' ');
            List<string> ret = [];
            List<string> words = new(wordarr.Length);
            foreach (string word in wordarr)
            {
                if (MeasureText(word).Width >= maxpx)
                {
                    int index = 0;
                    do
                    {
                        int linewidth = MeasureWordSplit(word, index, maxpx);
                        Debug.Assert(
                            linewidth != 0,
                            "word wrap split line width is zero");
                        words.Add(word.Substring(index, linewidth));
                        index += linewidth;
                    }
                    while (index != word.Length);
                }
                else
                {
                    words.Add(word);
                }
            }
            string seperator = string.Empty;
            StringBuilder linebuilder = new();
            int lnwidth = 0;
            for (int i = 0; i < words.Count; i++)
            {
                string word = words[i];
                string str = linebuilder.ToString();
                string add = str + seperator + word;
                lnwidth = MeasureText(add).Width;
                if (lnwidth < maxpx)
                {
                    _ = linebuilder.Append(seperator + word);
                }
                else
                {
                    ret.Add(str);
                    widths.Add(lnwidth);
                    _ = linebuilder.Clear();
                    _ = linebuilder.Append(word);
                    continue;
                }
                seperator = " ";
            }
            if (linebuilder.Length > 0)
            {
                ret.Add(linebuilder.ToString());
                widths.Add(lnwidth);
            }
            return ret;
        }
        public List<string> WordWrap(string input, int maxpx)
        {
            // This function isnt 100% for performance but i think thats okay.
            string[] originallines = input.Replace("\r\n", "\n").Split('\n');

            List<string> ret = [];
            List<int> widths = [];
            foreach (string line in originallines)
            {
                widths.Clear();
                List<string> wrapped = WrapLine(line, maxpx, widths);
                if (wrapped.Count == 0)
                {
                    wrapped.Add("");
                }
                ret.AddRange(wrapped);
            }
            return ret;
        }
        private Vertex[] GetGlyphVerts(FontGlyph glyph, int x, int y)
        {
            Vertex[] ret = new Vertex[4];
            int rx = x + glyph.xoffset;
            int ry = y + glyph.yoffset;
            int w = glyph.width;
            int h = glyph.height;
            ret[0] = new Vertex() { x = rx, y = ry, u = glyph.x1, v = glyph.y1 };
            ret[1] = new Vertex() { x = rx + w, y = ry, u = glyph.x2, v = glyph.y1 };
            ret[2] = new Vertex() { x = rx + w, y = ry + h, u = glyph.x2, v = glyph.y2 };
            ret[3] = new Vertex() { x = rx, y = ry + h, u = glyph.x1, v = glyph.y2 };
            return ret;
        }
        private List<Vertex> GenerateTextInternal(int posx, int posy, string text, bool render, out Size size)
        {
            List<Vertex> ret = new(text.Length * 4);
            int retwidth = 0;
            int retheight = LineHeight;
            int x = posx;
            int y = posy;
            for (int i = 0; i < text.Length; i++)
            {
                int charid = text[i];
                if (charid > _glyphs.Length - 1)
                {
                    charid = 0; // Unsupported character
                }
                if (text[i] == '\n')
                {
                    retheight += LineHeight;
                    y += LineHeight;
                    // Fuck it, return to carriage.
                    x = posx;
                }
                FontGlyph glyph = _glyphs[charid];
                if (render)
                {
                    ret.AddRange(GetGlyphVerts(glyph, x, y));
                }
                x += glyph.xadvance;
                // Check for kerning
                if (i + 1 < text.Length)
                {
                    if (glyph.kerning != null)
                    {
                        char c = text[i + 1];
                        if (c < glyph.kerning.Length)
                        {
                            sbyte ker = glyph.kerning[c];
                            x += ker;
                        }
                    }
                }
                retwidth = Math.Max(retwidth, x - posx); // In case of newline
            }
            size = new Size() { Width = retwidth, Height = retheight };
            return ret;
        }

        /// <summary>
        /// Generates a list of vertices for use in your opengl library.
        // It's expected for use in an orthogonal projection
        // Using GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        // with the texture associated with this bmfont loaded.
        // Render as GL_QUADS
        /// </summary>
        public List<Vertex> GenerateText(int posx, int posy, string text) => GenerateTextInternal(posx, posy, text, true, out _);
        public Size MeasureText(string text)
        {
            _ = GenerateTextInternal(0, 0, text, false, out Size ret);
            return ret;
        }
        private static string ExtractValue(string cword) => cword.Substring(cword.IndexOf('=') + 1);
        private static string ExtractKey(string cword) => cword.Substring(0, cword.IndexOf('='));
        private static int ParseInt(string s) => int.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }
}
