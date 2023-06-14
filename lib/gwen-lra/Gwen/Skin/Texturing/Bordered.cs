using System;
using System.Drawing;

namespace Gwen.Skin.Texturing
{
    /// <summary>
    /// 3x3 texture grid.
    /// </summary>
    public struct Bordered
    {
        #region Constructors

        public Bordered(Texture texture, float x, float y, float w, float h, Margin inMargin, float drawMarginScale = 1.0f)
            : this()
        {
            m_Rects = new SubRect[9];
            for (int i = 0; i < m_Rects.Length; i++)
            {
                m_Rects[i].uv = new float[4];
            }

            Init(texture, x, y, w, h, inMargin, drawMarginScale);
        }

        #endregion Constructors

        #region Methods

        // Can't have this as default param
        public void Draw(Renderer.RendererBase render, Rectangle r) => Draw(render, r, Color.White);
        private void DrawRect(Renderer.RendererBase render, int i, int x, int y, int w, int h)
        {
            if (w > 0 && h > 0)
                render.DrawTexturedRect(m_Texture,
                                        new Rectangle(x, y, w, h),
                                        m_Rects[i].uv[0], m_Rects[i].uv[1], m_Rects[i].uv[2], m_Rects[i].uv[3]);
        }
        private void DrawRect(Renderer.RendererBase renderer, Rectangle r, int i, int cellx, int celly)
        {
            System.Diagnostics.Debug.Assert(cellx >= 0 && cellx <= 2 && celly >= 0 && celly <= 2, "BorderedTexture indices out of range");
            int xleft = r.X;
            int xmid = r.X + m_Margin.Left;
            int xright = r.X + r.Width - m_Margin.Right;
            int ytop = r.Y;
            int ymid = r.Y + m_Margin.Top;
            int ybottom = r.Y + r.Height - m_Margin.Bottom;
            int x = 0;
            int w = 0;
            int y = 0;
            int h = 0;
            switch (cellx)
            {
                case 0: // Left
                    x = xleft;
                    w = m_Margin.Left;
                    break;
                case 1: // Middle
                    x = xmid;
                    w = r.Width - m_Margin.Left - m_Margin.Right;
                    break;
                case 2: // Right
                    //x = Math.Max(xmid, xright);
                    x = Math.Max(xmid, xright);
                    w = m_Margin.Right;
                    break;
            }

            switch (celly)
            {
                case 0: // Top
                    y = ytop;
                    h = m_Margin.Top;
                    break;
                case 1: // Middle
                    y = ymid;
                    h = r.Height - m_Margin.Top - m_Margin.Bottom;
                    break;
                case 2: // Bottom
                    y = Math.Max(ymid, ybottom);
                    h = m_Margin.Bottom;
                    break;
            }
            DrawRect(renderer, i, x, y, w, h);
        }

        public void Draw(Renderer.RendererBase render, Rectangle r, Color col)
        {
            if (m_Texture == null)
                return;

            render.DrawColor = col;

            // This code would shrink window titles on small windows.
            // i have removed it, but i'm not positive of how it affects others

            // if (r.Width < m_Width && r.Height < m_Height)
            // {
            //     render.DrawTexturedRect(m_Texture, r, m_Rects[0].uv[0], m_Rects[0].uv[1], m_Rects[8].uv[2], m_Rects[8].uv[3]);
            //     return;
            // }
            DrawRect(render, r, 0, 0, 0);
            DrawRect(render, r, 1, 1, 0);
            DrawRect(render, r, 2, 2, 0);
            DrawRect(render, r, 3, 0, 1);
            DrawRect(render, r, 4, 1, 1);
            DrawRect(render, r, 5, 2, 1);
            DrawRect(render, r, 6, 0, 2);
            DrawRect(render, r, 7, 1, 2);
            DrawRect(render, r, 8, 2, 2);
        }

        #endregion Methods

        #region Fields

        private readonly SubRect[] m_Rects;
        private float m_Height;
        private Margin m_Margin;
        private Texture m_Texture;
        private float m_Width;

        #endregion Fields

        private void Init(Texture texture, float x, float y, float w, float h, Margin inMargin, float drawMarginScale = 1.0f)
        {
            m_Texture = texture;

            m_Margin = inMargin;

            SetRect(0,
            x,
            y,
            m_Margin.Left,
            m_Margin.Top); // Top left

            SetRect(1,
            x + m_Margin.Left,
            y,
            w - m_Margin.Left - m_Margin.Right,
            m_Margin.Top); // Top middle

            SetRect(2,
            x + w - m_Margin.Right,
            y,
            m_Margin.Right,
            m_Margin.Top); // Top right

            SetRect(3,
            x,
            y + m_Margin.Top,
            m_Margin.Left,
            h - m_Margin.Top - m_Margin.Bottom); // Middle left

            SetRect(4,
            x + m_Margin.Left,
            y + m_Margin.Top,
            w - m_Margin.Left - m_Margin.Right,
            h - m_Margin.Top - m_Margin.Bottom); // Middle middle

            SetRect(5,
            x + w - m_Margin.Right,
            y + m_Margin.Top,
            m_Margin.Right,
            h - m_Margin.Top - m_Margin.Bottom); // Middle right

            SetRect(6,
            x,
            y + h - m_Margin.Bottom,
            m_Margin.Left,
            m_Margin.Bottom); // Bottom left

            SetRect(7,
            x + m_Margin.Left,
            y + h - m_Margin.Bottom,
            w - m_Margin.Left - m_Margin.Right,
            m_Margin.Bottom); // bBottom middle

            SetRect(8,
            x + w - m_Margin.Right,
            y + h - m_Margin.Bottom,
            m_Margin.Right,
            m_Margin.Bottom); // Bottom right

            m_Margin.Left = (int)(m_Margin.Left * drawMarginScale);
            m_Margin.Right = (int)(m_Margin.Right * drawMarginScale);
            m_Margin.Top = (int)(m_Margin.Top * drawMarginScale);
            m_Margin.Bottom = (int)(m_Margin.Bottom * drawMarginScale);

            m_Width = w - x;
            m_Height = h - y;
        }

        private void SetRect(int num, float x, float y, float w, float h)
        {
            float texw = m_Texture.Width;
            float texh = m_Texture.Height;

            //x -= 1.0f;
            //y -= 1.0f;

            m_Rects[num].uv[0] = x / texw;
            m_Rects[num].uv[1] = y / texh;

            m_Rects[num].uv[2] = (x + w) / texw;
            m_Rects[num].uv[3] = (y + h) / texh;

            //	rects[num].uv[0] += 1.0f / m_Texture->width;
            //	rects[num].uv[1] += 1.0f / m_Texture->width;
        }
    }

    public struct SubRect
    {
        #region Fields

        public float[] uv;

        #endregion Fields
    }
}