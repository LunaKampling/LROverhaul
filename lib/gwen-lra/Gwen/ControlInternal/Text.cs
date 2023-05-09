//#define DEBUG_TEXT_MEASURE

using System;
using System.Drawing;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Displays text. Always sized to contents.
    /// </summary>
    public class Text : Controls.ControlBase
    {
        private string m_String;
        private Font m_Font;

        /// <summary>
        /// Font used to display the text.
        /// </summary>
        /// <remarks>
        /// The font is not being disposed by this class.
        /// </remarks>
        public Font Font
        {
            get { return m_Font; }
            set
            {
                m_Font = value;
                SizeToContents();
            }
        }

        /// <summary>
        /// Text to display.
        /// </summary>
        public string String
        {
            get { return m_String; }
            set
            {
                m_String = value;
                SizeToContents();
            }
        }
        private Color m_TextColor;

        /// <summary>
        /// Text color.
        /// </summary>
        public Color TextColor
        {
            get
            {
                return m_TextColor;
            }

            set
            {
                if (value != m_TextColor)
                {
                    m_TextColor = value;
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Text length in characters.
        /// </summary>
        public int Length { get { return String.Length; } }

        public override Padding Padding
        {
            get
            {
                return Padding.Zero;
            }
            set
            {
                if (value != Padding.Zero)
                {
                    throw new Exception("Attempt to change Padding of internal text control");
                }
            }
        }
		public override Margin Margin
        {
            get
            {
                return Margin.Zero;
            }
            set
            {
                if (value != Margin.Zero)
				{
					throw new Exception("Attempt to change Margin of internal text control");
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Text(Controls.ControlBase parent)
            : base(parent)
        {
            m_Font = Skin.DefaultFont;

            m_String = String.Empty;
            TextColor = Skin.Colors.Text.Foreground;
            MouseInputEnabled = false;
            KeyboardInputEnabled = false;
            AutoSizeToContents = true;
            SizeToContents();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            if (Length == 0 || Font == null) return;

            skin.Renderer.DrawColor = TextColor;

            skin.Renderer.RenderText(Font, Point.Empty, String);
        }

        /// <summary>
        /// Handler invoked when control's scale changes.
        /// </summary>
        protected override void OnScaleChanged()
        {
            Invalidate();
        }
        public override Size GetSizeToFitContents()
		{
			if (String == null)
                return new Size(Width,Height);

			if (Font == null)
			{
				throw new InvalidOperationException("Text.SizeToContents() - No Font!!\n");
			}
			Point p = Skin.Renderer.MeasureText(Font, String);
            return new Size(p.X, p.Y);
        }
        /// <summary>
        /// Sizes the control to its contents.
        /// </summary>
        public void SizeToContents()
        {
            SizeToChildren(true,true);
        }
        public override bool SizeToChildren(bool width = true, bool height = true)
        {
            return base.SizeToChildren(width, height);
        }
        /// <summary>
        /// Gets the coordinates of specified character in the text.
        /// </summary>
        /// <param name="index">Character index.</param>
        /// <returns>Character position in local coordinates.</returns>
        public Point GetCharacterPosition(int index)
        {
            if (Length == 0 || index == 0)
            {
                return new Point(0, 0);
            }

			string sub = String.Substring(0, index);
			Point p = Skin.Renderer.MeasureText(Font, sub);
			p.Y = 0;

			return p;
        }

        /// <summary>
        /// Searches for a character closest to given point.
        /// </summary>
        /// <param name="p">Point.</param>
        /// <returns>Character index.</returns>
        public int GetClosestCharacter(Point p)
        {
            int distance = MaxCoord;
            int c = 0;

            for (int i = 0; i < String.Length + 1; i++)
            {
                Point cp = GetCharacterPosition(i);
                int dist = Math.Abs(cp.X - p.X) + Math.Abs(cp.Y - p.Y); // this isn't proper // [omeg] todo: sqrt

                if (dist > distance)
                    continue;

                distance = dist;
                c = i;
            }

            return c;
        }
	}
}
