using Gwen.Skin.Texturing;
using System;
using System.Drawing;
using System.IO;
using Single = Gwen.Skin.Texturing.Single;

namespace Gwen.Skin
{
    #region UI element textures

    public struct SkinTextures
    {
        #region Fields

        public _CategoryList CategoryList;
        public _CheckBox CheckBox;
        public _Input Input;
        public _Menu Menu;
        public _Panel Panel;
        public _Notification Notification;
        public _ProgressBar ProgressBar;
        public _RadioButton RadioButton;
        public _Scroller Scroller;
        public Bordered Selection;
        public Bordered Shadow;
        public Bordered StatusBar;
        public _Tab Tab;
        public _TextBox TextBox;
        public Bordered Tooltip;

        public _Tree Tree;

        public _Window Window;

        #endregion Fields

        #region Structs

        public struct _CategoryList
        {
            #region Fields

            public Bordered Header;
            public Bordered Inner;
            public Bordered Outer;

            #endregion Fields
        }

        public struct _CheckBox
        {
            #region Fields

            public _Active Active;

            public _Disabled Disabled;

            #endregion Fields

            #region Structs

            public struct _Active
            {
                #region Fields

                public Single Checked;
                public Single Normal;

                #endregion Fields
            }

            public struct _Disabled
            {
                #region Fields

                public Single Checked;
                public Single Normal;

                #endregion Fields
            }

            #endregion Structs
        }

        public struct _Input
        {
            #region Fields

            public _Button Button;

            public _ComboBox ComboBox;

            public _ListBox ListBox;

            public _Slider Slider;

            public _UpDown UpDown;

            #endregion Fields

            #region Structs

            public struct _Button
            {
                #region Fields

                public Bordered Disabled;
                public Bordered Hovered;
                public Bordered Normal;
                public Bordered Pressed;

                #endregion Fields
            }

            public struct _ComboBox
            {
                #region Fields

                public _Button Button;
                public Bordered Disabled;
                public Bordered Down;
                public Bordered Hover;
                public Bordered Normal;

                #endregion Fields

                #region Structs

                public struct _Button
                {
                    #region Fields

                    public Single Disabled;
                    public Single Down;
                    public Single Hover;
                    public Single Normal;

                    #endregion Fields
                }

                #endregion Structs
            }

            public struct _ListBox
            {
                #region Fields

                public Bordered Background;
                public Bordered EvenLine;
                public Bordered EvenLineSelected;
                public Bordered Hovered;
                public Bordered OddLine;
                public Bordered OddLineSelected;

                #endregion Fields
            }

            public struct _Slider
            {
                #region Fields

                public _H H;

                public _V V;

                #endregion Fields

                #region Structs

                public struct _H
                {
                    #region Fields

                    public Single Disabled;
                    public Single Down;
                    public Single Hover;
                    public Single Normal;
                    public Bordered Back;
                    public Bordered Front;
                    #endregion Fields
                }

                public struct _V
                {
                    #region Fields

                    public Single Disabled;
                    public Single Down;
                    public Single Hover;
                    public Single Normal;
                    public Bordered Back;
                    public Bordered Front;

                    #endregion Fields
                }

                #endregion Structs
            }

            public struct _UpDown
            {
                #region Fields

                public _Down Down;

                public _Up Up;

                #endregion Fields

                #region Structs

                public struct _Down
                {
                    #region Fields

                    public Single Disabled;
                    public Single Down;
                    public Single Hover;
                    public Single Normal;

                    #endregion Fields
                }

                public struct _Up
                {
                    #region Fields

                    public Single Disabled;
                    public Single Down;
                    public Single Hover;
                    public Single Normal;

                    #endregion Fields
                }

                #endregion Structs
            }

            #endregion Structs
        }

        public struct _Menu
        {
            #region Fields

            public Bordered Background;
            public Bordered BackgroundWithMargin;
            public Single Check;
            public Bordered Hover;
            public Single RightArrow;
            public Bordered Strip;

            #endregion Fields
        }

        public struct _Panel
        {
            #region Fields

            // public Bordered Bright;
            // public Bordered Dark;
            // public Bordered Highlight;
            public Bordered Normal;

            #endregion Fields
        }
        public struct _Notification
        {
            #region Fields
            public Bordered Normal;

            #endregion Fields
        }

        public struct _ProgressBar
        {
            #region Fields

            public Bordered Back;
            public Bordered Front;

            #endregion Fields
        }

        public struct _RadioButton
        {
            #region Fields

            public _Active Active;

            public _Disabled Disabled;

            #endregion Fields

            #region Structs

            public struct _Active
            {
                #region Fields

                public Single Checked;
                public Single Normal;

                #endregion Fields
            }

            public struct _Disabled
            {
                #region Fields

                public Single Checked;
                public Single Normal;

                #endregion Fields
            }

            #endregion Structs
        }

        public struct _Scroller
        {
            #region Fields

            public _Button Button;
            public Bordered ButtonH_Disabled;
            public Bordered ButtonH_Down;
            public Bordered ButtonH_Hover;
            public Bordered ButtonH_Normal;
            public Bordered ButtonV_Disabled;
            public Bordered ButtonV_Down;
            public Bordered ButtonV_Hover;
            public Bordered ButtonV_Normal;
            public Bordered TrackH;
            public Bordered TrackV;

            #endregion Fields

            #region Structs

            public struct _Button
            {
                #region Fields

                public Bordered[] Disabled;
                public Bordered[] Down;
                public Bordered[] Hover;
                public Bordered[] Normal;

                #endregion Fields
            }

            #endregion Structs
        }

        public struct _Tab
        {
            #region Fields

            public _Bottom Bottom;

            public Bordered Control;

            public _Left Left;

            public _Right Right;

            public _Top Top;

            #endregion Fields

            #region Structs

            public struct _Bottom
            {
                #region Fields

                public Bordered Active;
                public Bordered Inactive;

                #endregion Fields
            }

            public struct _Left
            {
                #region Fields

                public Bordered Active;
                public Bordered Inactive;

                #endregion Fields
            }

            public struct _Right
            {
                #region Fields

                public Bordered Active;
                public Bordered Inactive;

                #endregion Fields
            }

            public struct _Top
            {
                #region Fields

                public Bordered Active;
                public Bordered Inactive;

                #endregion Fields
            }

            #endregion Structs
        }

        public struct _TextBox
        {
            #region Fields

            public Bordered Disabled;
            public Bordered Focus;
            public Bordered Normal;

            #endregion Fields
        }

        public struct _Tree
        {
            #region Fields

            public Bordered Background;
            public Single Minus;
            public Single Plus;

            #endregion Fields
        }

        public struct _Window
        {
            #region Fields

            public Single Close;
            public Single Close_Disabled;
            public Single Close_Down;
            public Single Close_Hover;
            public Bordered Inactive;
            public Bordered Normal;

            #endregion Fields
        }

        #endregion Structs
    }

    #endregion UI element textures

    /// <summary>
    /// Base textured skin.
    /// </summary>
    public class TexturedBase : Skin.SkinBase
    {
        #region Constructors
        public TexturedBase(Renderer.RendererBase renderer, Stream textureData, string colorxml)
            : base(renderer)
        {
            m_Texture = new Texture(Renderer);
            m_Texture.LoadStream(textureData);
            _colorxml = colorxml;
            InitializeColors();
            InitializeTextures();
        }
        public TexturedBase(Renderer.RendererBase renderer, Texture texture, string colorxml)
            : base(renderer)
        {
            m_Texture = texture;
            _colorxml = colorxml;
            InitializeColors();
            InitializeTextures();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            m_Texture.Dispose();
            base.Dispose();
        }

        #endregion Methods

        #region Fields

        protected SkinTextures Textures;

        private readonly Texture m_Texture;

        private string _colorxml = null;
        #endregion Fields

        #region Initialization
        private Color LoadColor(System.Xml.XmlDocument read, string name)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            return Color.FromArgb(255, Color.FromArgb(int.Parse(read.DocumentElement[name].InnerText, System.Globalization.NumberStyles.HexNumber, culture)));
        }
        private void InitializeColors()
        {
            System.Xml.XmlDocument read = new System.Xml.XmlDocument();

            read.LoadXml(_colorxml);
            //https://palx.jxnblk.com/07c
            Colors.Text.Contrast = LoadColor(read, "Text.Contrast");
            Colors.Text.ContrastLow = LoadColor(read, "Text.ContrastLow");
            Colors.Text.Foreground = LoadColor(read, "Text.Foreground");
            Colors.Text.Highlight = LoadColor(read, "Text.Highlight");
            Colors.Text.AccentForeground = LoadColor(read, "Text.AccentForeground");
            Colors.Text.Inactive = LoadColor(read, "Text.Inactive");
            Colors.Text.Disabled = LoadColor(read, "Text.Disabled");

            Colors.Accent = LoadColor(read, "Accent");
            Colors.AccentHigh = LoadColor(read, "AccentHigh");
            Colors.AccentLow = LoadColor(read, "AccentLow");
            Colors.Background = LoadColor(read, "Background");
            Colors.BackgroundHighlight = LoadColor(read, "BackgroundHighlight");
            Colors.Foreground = LoadColor(read, "Foreground");
            Colors.ForegroundHighlight = LoadColor(read, "ForegroundHighlight");
            Colors.ForegroundInactive = LoadColor(read, "ForegroundInactive");
            Colors.ModalBackground = Color.FromArgb(128, Colors.Background);

        }
        private void InitializeDefaultColors()
        {
            Colors.Text.Contrast = Color.FromArgb(255, Color.FromArgb(0x0077cc));       // #0077cc
            Colors.Text.ContrastLow = Color.FromArgb(255, Color.FromArgb(0x004d84));    // #004d84
            Colors.Text.Foreground = Color.FromArgb(255, Color.FromArgb(0x374047));     // #374047
            Colors.Text.Highlight = Color.FromArgb(255, Color.FromArgb(0xf8f9f9));      // #f8f9f9
            Colors.Text.Inactive = Color.FromArgb(255, Color.FromArgb(0xbec4c8));       // #bec4c8
            Colors.Text.Disabled = Color.FromArgb(255, Color.FromArgb(0x7f8a93));       // #7f8a93
            Colors.Accent = Color.FromArgb(255, Color.FromArgb(0x0077cc));              // #0077cc
            Colors.AccentHigh = Color.FromArgb(255, Color.FromArgb(0xc6e1f3));          // #c6e1f3
            Colors.AccentLow = Color.FromArgb(255, Color.FromArgb(0x004d84));           // #004d84
            Colors.Background = Color.FromArgb(255, Color.FromArgb(0xf8f9f9));          // #f8f9f9
            Colors.BackgroundHighlight = Color.FromArgb(255, Color.FromArgb(0xebedee)); // #ebedee
            Colors.Foreground = Color.FromArgb(255, Color.FromArgb(0x374047));          // #374047
            Colors.ForegroundHighlight = Color.FromArgb(255, Color.FromArgb(0xdee1e3)); // #dee1e3
            Colors.ForegroundInactive = Color.FromArgb(255, Color.FromArgb(0xbec4c8));  // #bec4c8
            Colors.Text.AccentForeground = Colors.Background;

            Colors.ModalBackground = Color.FromArgb(128, Colors.Background);
        }
        private void InitializeTextures()
        {
            Textures.Shadow = new Bordered(m_Texture, 448, 0, 31, 31, Margin.Ten);
            Textures.Tooltip = new Bordered(m_Texture, 128, 320, 127, 31, Margin.Eight);
            Textures.StatusBar = new Bordered(m_Texture, 128, 288, 127, 31, Margin.Eight);
            Textures.Selection = new Bordered(m_Texture, 384, 32, 31, 31, Margin.Four);

            Textures.Panel.Normal = new Bordered(m_Texture, 256, 0, 63, 63, new Margin(16, 16, 16, 16));
            Textures.Notification.Normal = new Bordered(m_Texture, 320, 0, 63, 63, new Margin(16, 16, 16, 16));
            // Textures.Panel.Bright = new Bordered(m_Texture, 256 + 64, 0, 63, 63, new Margin(16, 16, 16, 16));
            // Textures.Panel.Dark = new Bordered(m_Texture, 256, 64, 63, 63, new Margin(16, 16, 16, 16));
            // Textures.Panel.Highlight = new Bordered(m_Texture, 256 + 64, 64, 63, 63, new Margin(16, 16, 16, 16));

            Textures.Window.Normal = new Bordered(m_Texture, 0, 0, 127, 127, new Margin(8, 32, 8, 8));
            Textures.Window.Inactive = new Bordered(m_Texture, 128, 0, 127, 127, new Margin(8, 32, 8, 8));

            Textures.CheckBox.Active.Checked = new Single(m_Texture, 448, 32, 15, 15);
            Textures.CheckBox.Active.Normal = new Single(m_Texture, 464, 32, 15, 15);
            Textures.CheckBox.Disabled.Checked = new Single(m_Texture, 448, 64, 15, 15);
            Textures.CheckBox.Disabled.Normal = new Single(m_Texture, 464, 64, 15, 15);

            Textures.RadioButton.Active.Checked = new Single(m_Texture, 448, 80, 15, 15);
            Textures.RadioButton.Active.Normal = new Single(m_Texture, 464, 80, 15, 15);
            Textures.RadioButton.Disabled.Checked = new Single(m_Texture, 448, 112, 15, 15);
            Textures.RadioButton.Disabled.Normal = new Single(m_Texture, 464, 112, 15, 15);

            Textures.TextBox.Normal = new Bordered(m_Texture, 0, 150, 127, 21, Margin.Four);
            Textures.TextBox.Focus = new Bordered(m_Texture, 0, 172, 127, 21, Margin.Four);
            Textures.TextBox.Disabled = new Bordered(m_Texture, 0, 194, 127, 21, Margin.Four);

            Textures.Menu.Strip = new Bordered(m_Texture, 0, 128, 127, 21, Margin.One);
            Textures.Menu.BackgroundWithMargin = new Bordered(m_Texture, 128, 128, 127, 63, new Margin(24, 8, 8, 8));
            Textures.Menu.Background = new Bordered(m_Texture, 128, 192, 127, 63, Margin.Eight);
            Textures.Menu.Hover = new Bordered(m_Texture, 128, 256, 127, 31, Margin.Eight);
            Textures.Menu.RightArrow = new Single(m_Texture, 400, 80, 15, 15);
            Textures.Menu.Check = new Single(m_Texture, 384, 80, 15, 15);

            Textures.Tab.Control = new Bordered(m_Texture, 0, 256, 127, 127, Margin.Eight);
            Textures.Tab.Bottom.Active = new Bordered(m_Texture, 0, 416, 63, 31, Margin.Eight);
            Textures.Tab.Bottom.Inactive = new Bordered(m_Texture, 0 + 128, 416, 63, 31, Margin.Eight);
            Textures.Tab.Top.Active = new Bordered(m_Texture, 0, 384, 63, 31, Margin.Eight);
            Textures.Tab.Top.Inactive = new Bordered(m_Texture, 0 + 128, 384, 63, 31, Margin.Eight);
            Textures.Tab.Left.Active = new Bordered(m_Texture, 64, 384, 31, 63, Margin.Eight);
            Textures.Tab.Left.Inactive = new Bordered(m_Texture, 64 + 128, 384, 31, 63, Margin.Eight);
            Textures.Tab.Right.Active = new Bordered(m_Texture, 96, 384, 31, 63, Margin.Eight);
            Textures.Tab.Right.Inactive = new Bordered(m_Texture, 96 + 128, 384, 31, 63, Margin.Eight);

            Textures.Window.Close = new Single(m_Texture, 0, 224, 24, 24);
            Textures.Window.Close_Hover = new Single(m_Texture, 32, 224, 24, 24);
            Textures.Window.Close_Down = new Single(m_Texture, 64, 224, 24, 24);
            Textures.Window.Close_Disabled = new Single(m_Texture, 96, 224, 24, 24);

            Textures.Scroller.TrackV = new Bordered(m_Texture, 384, 208, 15, 127, Margin.Four);
            Textures.Scroller.ButtonV_Normal = new Bordered(m_Texture, 384 + 16, 208, 15, 127, Margin.Four);
            Textures.Scroller.ButtonV_Hover = new Bordered(m_Texture, 384 + 32, 208, 15, 127, Margin.Four);
            Textures.Scroller.ButtonV_Down = new Bordered(m_Texture, 384 + 48, 208, 15, 127, Margin.Four);
            Textures.Scroller.ButtonV_Disabled = new Bordered(m_Texture, 384 + 64, 208, 15, 127, Margin.Four);
            Textures.Scroller.TrackH = new Bordered(m_Texture, 384, 128, 127, 15, Margin.Four);
            Textures.Scroller.ButtonH_Normal = new Bordered(m_Texture, 384, 128 + 16, 127, 15, Margin.Four);
            Textures.Scroller.ButtonH_Hover = new Bordered(m_Texture, 384, 128 + 32, 127, 15, Margin.Four);
            Textures.Scroller.ButtonH_Down = new Bordered(m_Texture, 384, 128 + 48, 127, 15, Margin.Four);
            Textures.Scroller.ButtonH_Disabled = new Bordered(m_Texture, 384, 128 + 64, 127, 15, Margin.Four);

            Textures.Scroller.Button.Normal = new Bordered[4];
            Textures.Scroller.Button.Disabled = new Bordered[4];
            Textures.Scroller.Button.Hover = new Bordered[4];
            Textures.Scroller.Button.Down = new Bordered[4];

            Textures.Tree.Background = new Bordered(m_Texture, 256, 128, 127, 127, new Margin(16, 16, 16, 16));
            Textures.Tree.Plus = new Single(m_Texture, 384, 64, 15, 15);
            Textures.Tree.Minus = new Single(m_Texture, 400, 64, 15, 15);

            Textures.Input.Button.Normal = new Bordered(m_Texture, 480, 0, 31, 31, Margin.Eight);
            Textures.Input.Button.Hovered = new Bordered(m_Texture, 480, 32, 31, 31, Margin.Eight);
            Textures.Input.Button.Disabled = new Bordered(m_Texture, 480, 64, 31, 31, Margin.Eight);
            Textures.Input.Button.Pressed = new Bordered(m_Texture, 480, 96, 31, 31, Margin.Eight);

            for (int i = 0; i < 4; i++)
            {
                Textures.Scroller.Button.Normal[i] = new Bordered(m_Texture, 464 + 0, 208 + i * 16, 15, 15, Margin.Two);
                Textures.Scroller.Button.Hover[i] = new Bordered(m_Texture, 480, 208 + i * 16, 15, 15, Margin.Two);
                Textures.Scroller.Button.Down[i] = new Bordered(m_Texture, 464, 272 + i * 16, 15, 15, Margin.Two);
                Textures.Scroller.Button.Disabled[i] = new Bordered(m_Texture, 480, 272 + i * 16, 15, 15, Margin.Two);
            }

            Textures.Input.ListBox.Background = new Bordered(m_Texture, 256, 256, 63, 63, Margin.Eight);
            Textures.Input.ListBox.Hovered = new Bordered(m_Texture, 320, 320, 31, 31, Margin.Eight);
            Textures.Input.ListBox.EvenLine = new Bordered(m_Texture, 352, 256, 31, 31, Margin.Eight);
            Textures.Input.ListBox.OddLine = new Bordered(m_Texture, 352, 288, 31, 31, Margin.Eight);
            Textures.Input.ListBox.EvenLineSelected = new Bordered(m_Texture, 320, 270, 31, 31, Margin.Eight);
            Textures.Input.ListBox.OddLineSelected = new Bordered(m_Texture, 320, 288, 31, 31, Margin.Eight);

            Textures.Input.ComboBox.Normal = new Bordered(m_Texture, 384, 336, 127, 31, new Margin(8, 8, 32, 8));
            Textures.Input.ComboBox.Hover = new Bordered(m_Texture, 384, 336 + 32, 127, 31, new Margin(8, 8, 32, 8));
            Textures.Input.ComboBox.Down = new Bordered(m_Texture, 384, 336 + 64, 127, 31, new Margin(8, 8, 32, 8));
            Textures.Input.ComboBox.Disabled = new Bordered(m_Texture, 384, 336 + 96, 127, 31, new Margin(8, 8, 32, 8));

            Textures.Input.ComboBox.Button.Normal = new Single(m_Texture, 496, 272, 15, 15);
            Textures.Input.ComboBox.Button.Hover = new Single(m_Texture, 496, 272 + 16, 15, 15);
            Textures.Input.ComboBox.Button.Down = new Single(m_Texture, 496, 272 + 32, 15, 15);
            Textures.Input.ComboBox.Button.Disabled = new Single(m_Texture, 496, 272 + 48, 15, 15);

            Textures.Input.UpDown.Up.Normal = new Single(m_Texture, 384, 112, 7, 7);
            Textures.Input.UpDown.Up.Hover = new Single(m_Texture, 384 + 8, 112, 7, 7);
            Textures.Input.UpDown.Up.Down = new Single(m_Texture, 384 + 16, 112, 7, 7);
            Textures.Input.UpDown.Up.Disabled = new Single(m_Texture, 384 + 24, 112, 7, 7);
            Textures.Input.UpDown.Down.Normal = new Single(m_Texture, 384, 120, 7, 7);
            Textures.Input.UpDown.Down.Hover = new Single(m_Texture, 384 + 8, 120, 7, 7);
            Textures.Input.UpDown.Down.Down = new Single(m_Texture, 384 + 16, 120, 7, 7);
            Textures.Input.UpDown.Down.Disabled = new Single(m_Texture, 384 + 24, 120, 7, 7);

            Textures.ProgressBar.Back = new Bordered(m_Texture, 384, 0, 31, 31, Margin.Eight);
            Textures.ProgressBar.Front = new Bordered(m_Texture, 384 + 32, 0, 31, 31, Margin.Eight);

            Textures.Input.Slider.H.Back = new Bordered(m_Texture, 256, 64, 31, 15, Margin.Five);
            Textures.Input.Slider.H.Front = new Bordered(m_Texture, 256, 80, 31, 15, Margin.Five);
            Textures.Input.Slider.V.Back = new Bordered(m_Texture, 256, 96, 15, 31, Margin.Five);
            Textures.Input.Slider.V.Front = new Bordered(m_Texture, 272, 96, 15, 31, Margin.Five);

            Textures.Input.Slider.H.Normal = new Single(m_Texture, 416, 32, 15, 15);
            Textures.Input.Slider.H.Hover = new Single(m_Texture, 416, 48, 15, 15);
            Textures.Input.Slider.H.Down = new Single(m_Texture, 416, 64, 15, 15);
            Textures.Input.Slider.H.Disabled = new Single(m_Texture, 416, 80, 15, 15);

            Textures.Input.Slider.V.Normal = new Single(m_Texture, 432, 32, 15, 15);
            Textures.Input.Slider.V.Hover = new Single(m_Texture, 432, 48, 15, 15);
            Textures.Input.Slider.V.Down = new Single(m_Texture, 432, 64, 15, 15);
            Textures.Input.Slider.V.Disabled = new Single(m_Texture, 432, 80, 15, 15);

            Textures.CategoryList.Outer = new Bordered(m_Texture, 256, 320, 63, 63, Margin.Eight);
            Textures.CategoryList.Inner = new Bordered(m_Texture, 256 + 64, 384, 63, 63, new Margin(8, 21, 8, 8));
            Textures.CategoryList.Header = new Bordered(m_Texture, 320, 352, 63, 31, Margin.Eight);
        }

        #endregion Initialization

        #region UI elements

        public override void DrawButton(Controls.ControlBase control, bool depressed, bool hovered, bool disabled)
        {
            if (disabled)
            {
                Textures.Input.Button.Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (depressed)
            {
                Textures.Input.Button.Pressed.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (hovered)
            {
                Textures.Input.Button.Hovered.Draw(Renderer, control.RenderBounds);
                return;
            }
            Textures.Input.Button.Normal.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawCategoryHolder(Controls.ControlBase control)
        {
            Textures.CategoryList.Outer.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawCategoryInner(Controls.ControlBase control, bool collapsed)
        {
            if (collapsed)
                Textures.CategoryList.Header.Draw(Renderer, control.RenderBounds);
            else
                Textures.CategoryList.Inner.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawCheckBox(Controls.ControlBase control, bool selected, bool depressed)
        {
            if (selected)
            {
                if (control.IsDisabled)
                    Textures.CheckBox.Disabled.Checked.Draw(Renderer, control.RenderBounds);
                else
                    Textures.CheckBox.Active.Checked.Draw(Renderer, control.RenderBounds);
            }
            else
            {
                if (control.IsDisabled)
                    Textures.CheckBox.Disabled.Normal.Draw(Renderer, control.RenderBounds);
                else
                    Textures.CheckBox.Active.Normal.Draw(Renderer, control.RenderBounds);
            }
        }
        public override void DrawPanel(Controls.ControlBase control, byte panelalpha)
        {
            Textures.Panel.Normal.Draw(Renderer, control.RenderBounds, Color.FromArgb(panelalpha, Color.White));
        }
        public override void DrawNotification(Controls.ControlBase control, float fade)
        {
            Color c = Color.FromArgb((int)Math.Round(255 * fade), Color.White);
            Textures.Notification.Normal.Draw(Renderer, control.RenderBounds, c);
        }

        public override void DrawColorDisplay(Controls.ControlBase control, Color color)
        {
            Rectangle rect = control.RenderBounds;

            if (color.A != 255)
            {
                Renderer.DrawColor = Color.FromArgb(255, 255, 255, 255);
                Renderer.DrawFilledRect(rect);

                Renderer.DrawColor = Color.FromArgb(128, 128, 128, 128);

                Renderer.DrawFilledRect(Util.FloatRect(0, 0, rect.Width * 0.5f, rect.Height * 0.5f));
                Renderer.DrawFilledRect(Util.FloatRect(rect.Width * 0.5f, rect.Height * 0.5f, rect.Width * 0.5f, rect.Height * 0.5f));
            }

            Renderer.DrawColor = color;
            Renderer.DrawFilledRect(rect);

            Renderer.DrawColor = Color.Black;
            Renderer.DrawLinedRect(rect);
        }

        public override void DrawComboBox(Controls.ControlBase control, bool down, bool open)
        {
            if (control.IsDisabled)
            {
                Textures.Input.ComboBox.Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (down || open)
            {
                Textures.Input.ComboBox.Down.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsHovered)
            {
                Textures.Input.ComboBox.Hover.Draw(Renderer, control.RenderBounds);
                return;
            }

            Textures.Input.ComboBox.Normal.Draw(Renderer, control.RenderBounds);
        }
        public override void DrawDropDownArrow(Controls.ControlBase control, bool hovered, bool down, bool open, bool disabled)
        {
            var bounds = control.RenderBounds;
            bounds.Height = 15;
            bounds.Y += (control.RenderBounds.Height / 2) - (bounds.Height / 2);
            bounds.Width = 15;
            bounds.X += (control.RenderBounds.Width / 2) - (bounds.Width / 2);

            if (disabled)
            {
                Textures.Input.ComboBox.Button.Disabled.Draw(Renderer, bounds);
                return;
            }

            if (down || open)
            {
                Textures.Input.ComboBox.Button.Down.Draw(Renderer, bounds);
                return;
            }

            if (hovered)
            {
                Textures.Input.ComboBox.Button.Hover.Draw(Renderer, bounds);
                return;
            }

            Textures.Input.ComboBox.Button.Normal.Draw(Renderer, bounds);
        }

        public override void DrawGroupBox(Controls.ControlBase control, int textStart, int textHeight, int textWidth)
        {
            Rectangle rect = control.RenderBounds;

            rect.Y += (int)(textHeight * 0.5f);
            rect.Height -= (int)(textHeight * 0.5f);

            Color m_colDarker = Colors.ForegroundInactive;
            Color m_colLighter = Colors.BackgroundHighlight;

            Renderer.DrawColor = m_colLighter;

            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, textStart - 3, 1));
            Renderer.DrawFilledRect(new Rectangle(rect.X + 1 + textStart + textWidth, rect.Y + 1, rect.Width - textStart + textWidth - 2, 1));
            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, (rect.Y + rect.Height) - 1, rect.X + rect.Width - 2, 1));

            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, 1, rect.Height));
            Renderer.DrawFilledRect(new Rectangle((rect.X + rect.Width) - 2, rect.Y + 1, 1, rect.Height - 1));

            Renderer.DrawColor = m_colDarker;

            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y, textStart - 3, 1));
            Renderer.DrawFilledRect(new Rectangle(rect.X + 1 + textStart + textWidth, rect.Y, rect.Width - textStart - textWidth - 2, 1));
            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, (rect.Y + rect.Height) - 1, rect.X + rect.Width - 2, 1));

            Renderer.DrawFilledRect(new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 1));
            Renderer.DrawFilledRect(new Rectangle((rect.X + rect.Width) - 1, rect.Y + 1, 1, rect.Height - 1));
        }

        public override void DrawHighlight(Controls.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;
            Renderer.DrawColor = Color.FromArgb(255, 255, 100, 255);
            Renderer.DrawFilledRect(rect);
        }

        public override void DrawKeyboardHighlight(Controls.ControlBase control, Rectangle r, int offset)
        {
            Rectangle rect = r;

            rect.X += offset;
            rect.Y += offset;
            rect.Width -= offset * 2;
            rect.Height -= offset * 2;

            //draw the top and bottom
            bool skip = true;
            for (int i = 0; i < rect.Width * 0.5; i++)
            {
                m_Renderer.DrawColor = Color.Black;
                if (!skip)
                {
                    Renderer.DrawPixel(rect.X + (i * 2), rect.Y);
                    Renderer.DrawPixel(rect.X + (i * 2), rect.Y + rect.Height - 1);
                }
                else
                    skip = false;
            }

            for (int i = 0; i < rect.Height * 0.5; i++)
            {
                Renderer.DrawColor = Color.Black;
                Renderer.DrawPixel(rect.X, rect.Y + i * 2);
                Renderer.DrawPixel(rect.X + rect.Width - 1, rect.Y + i * 2);
            }
        }

        public override void DrawListBox(Controls.ControlBase control)
        {
            Textures.Input.ListBox.Background.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawListBoxLine(Controls.ControlBase control, bool selected, bool even)
        {
            if (selected)
            {
                if (even)
                {
                    Textures.Input.ListBox.EvenLineSelected.Draw(Renderer, control.RenderBounds);
                    return;
                }
                Textures.Input.ListBox.OddLineSelected.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsHovered)
            {
                Textures.Input.ListBox.Hovered.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (even)
            {
                Textures.Input.ListBox.EvenLine.Draw(Renderer, control.RenderBounds);
                return;
            }

            Textures.Input.ListBox.OddLine.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawMenu(Controls.ControlBase control, bool paddingDisabled)
        {
            if (!paddingDisabled)
            {
                Textures.Menu.BackgroundWithMargin.Draw(Renderer, control.RenderBounds);
                return;
            }

            Textures.Menu.Background.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawMenuDivider(Controls.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;
            Renderer.DrawColor = Color.FromArgb(100, 0, 0, 0);
            Renderer.DrawFilledRect(rect);
        }

        public override void DrawMenuItem(Controls.ControlBase control, bool submenuOpen, bool isChecked)
        {
            if (submenuOpen || control.IsHovered)
                Textures.Menu.Hover.Draw(Renderer, control.RenderBounds);

            if (isChecked)
                Textures.Menu.Check.Draw(Renderer, new Rectangle(control.RenderBounds.X + 4, control.RenderBounds.Y + 3, 15, 15));
        }

        public override void DrawMenuRightArrow(Controls.ControlBase control)
        {
            Textures.Menu.RightArrow.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawMenuStrip(Controls.ControlBase control)
        {
            Textures.Menu.Strip.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawModalControl(Controls.ControlBase control)
        {
            if (!control.ShouldDrawBackground)
                return;
            Rectangle rect = control.RenderBounds;
            Renderer.DrawColor = Colors.ModalBackground;
            Renderer.DrawFilledRect(rect);
        }

        public override void DrawNumericUpDownButton(Controls.ControlBase control, bool depressed, bool up)
        {
            if (up)
            {
                if (control.IsDisabled)
                {
                    Textures.Input.UpDown.Up.Disabled.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                if (depressed)
                {
                    Textures.Input.UpDown.Up.Down.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                if (control.IsHovered)
                {
                    Textures.Input.UpDown.Up.Hover.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                Textures.Input.UpDown.Up.Normal.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsDisabled)
            {
                Textures.Input.UpDown.Down.Disabled.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                Textures.Input.UpDown.Down.Down.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsHovered)
            {
                Textures.Input.UpDown.Down.Hover.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            Textures.Input.UpDown.Down.Normal.DrawCenter(Renderer, control.RenderBounds);
        }
        public override void DrawProgressBar(Controls.ControlBase control, bool horizontal, float progress)
        {
            Rectangle rect = control.RenderBounds;

            if (horizontal)
            {
                Textures.ProgressBar.Back.Draw(Renderer, rect);
                var prevclip = ClipArea(new Rectangle(0, 0, (int)Math.Round(rect.Width * progress), rect.Height));
                // Renderer.ClipRegion = new Rectangle(Renderer.ClipRegion.X + rect.X, Renderer.ClipRegion.Y + rect.Y, Renderer.ClipRegion.Width, Renderer.ClipRegion.Height);
                Textures.ProgressBar.Front.Draw(Renderer, rect);
                Renderer.ClipRegion = prevclip;
            }
            else
            {
                Textures.ProgressBar.Back.Draw(Renderer, rect);
                var prevclip = ClipArea(new Rectangle(0, (rect.Y + rect.Height) - (int)Math.Round(rect.Height * progress), rect.Width, (int)Math.Round(rect.Height * progress)));
                Textures.ProgressBar.Front.Draw(Renderer, rect);
                Renderer.ClipRegion = prevclip;
            }
        }

        public override void DrawRadioButton(Controls.ControlBase control, bool selected, bool depressed)
        {
            if (selected)
            {
                if (control.IsDisabled)
                    Textures.RadioButton.Disabled.Checked.Draw(Renderer, control.RenderBounds);
                else
                    Textures.RadioButton.Active.Checked.Draw(Renderer, control.RenderBounds);
            }
            else
            {
                if (control.IsDisabled)
                    Textures.RadioButton.Disabled.Normal.Draw(Renderer, control.RenderBounds);
                else
                    Textures.RadioButton.Active.Normal.Draw(Renderer, control.RenderBounds);
            }
        }

        public override void DrawScrollBar(Controls.ControlBase control, bool horizontal, bool depressed)
        {
            if (horizontal)
                Textures.Scroller.TrackH.Draw(Renderer, control.RenderBounds);
            else
                Textures.Scroller.TrackV.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawScrollBarBar(Controls.ControlBase control, bool depressed, bool hovered, bool horizontal)
        {
            if (!horizontal)
            {
                if (control.IsDisabled)
                {
                    Textures.Scroller.ButtonV_Disabled.Draw(Renderer, control.RenderBounds);
                    return;
                }

                if (depressed)
                {
                    Textures.Scroller.ButtonV_Down.Draw(Renderer, control.RenderBounds);
                    return;
                }

                if (hovered)
                {
                    Textures.Scroller.ButtonV_Hover.Draw(Renderer, control.RenderBounds);
                    return;
                }

                Textures.Scroller.ButtonV_Normal.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsDisabled)
            {
                Textures.Scroller.ButtonH_Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                Textures.Scroller.ButtonH_Down.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (hovered)
            {
                Textures.Scroller.ButtonH_Hover.Draw(Renderer, control.RenderBounds);
                return;
            }

            Textures.Scroller.ButtonH_Normal.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawScrollButton(Controls.ControlBase control, Pos direction, bool depressed, bool hovered, bool disabled)
        {
            int i = 0;
            if (direction == Pos.Top) i = 1;
            if (direction == Pos.Right) i = 2;
            if (direction == Pos.Bottom) i = 3;

            if (disabled)
            {
                Textures.Scroller.Button.Disabled[i].Draw(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                Textures.Scroller.Button.Down[i].Draw(Renderer, control.RenderBounds);
                return;
            }

            if (hovered)
            {
                Textures.Scroller.Button.Hover[i].Draw(Renderer, control.RenderBounds);
                return;
            }

            Textures.Scroller.Button.Normal[i].Draw(Renderer, control.RenderBounds);
        }

        public override void DrawShadow(Controls.ControlBase control)
        {
            Rectangle r = control.RenderBounds;
            r.X -= 5;
            r.Y -= 5;
            r.Width += 10;
            r.Height += 10;
            Textures.Shadow.Draw(Renderer, r);
        }

        public override void DrawSlider(Controls.ControlBase control, bool horizontal, int numNotches, int barSize, double val)
        {
            const int sliderbarsize = 15;
            Rectangle rect = control.RenderBounds;
            var barrect = rect;
            Renderer.DrawColor = Color.FromArgb(100, 0, 0, 0);
            if (horizontal)
            {
                barrect.Y = (int)Math.Round((rect.Height / 2.0) - (sliderbarsize / 2.0));
                barrect.Height = sliderbarsize;
                barrect.X += (int)Math.Round(sliderbarsize / 2.0);
                barrect.Width -= sliderbarsize;

                rect.X += (int)(barSize * 0.5);
                rect.Width -= barSize;
                rect.Y += (int)(rect.Height * 0.5 - 1);
                rect.Height = 1;
                DrawSliderNotchesH(rect, numNotches, barSize * 0.5f);

                Textures.Input.Slider.H.Back.Draw(Renderer, barrect);
                if (!control.IsDisabled)
                {
                    int w = (int)Math.Round(barrect.Width * val);
                    var clip = ClipArea(new Rectangle(barrect.X, barrect.Y, w, barrect.Height));
                    Textures.Input.Slider.H.Front.Draw(Renderer, barrect);
                    Renderer.ClipRegion = clip;
                }
            }
            else
            {
                barrect.X = (int)Math.Round((rect.Width / 2.0) - (sliderbarsize / 2.0));
                barrect.Width = sliderbarsize;
                barrect.Y += (int)Math.Round((sliderbarsize) / 2.0);
                barrect.Height -= sliderbarsize;

                rect.Y += (int)(barSize * 0.5);
                rect.Height -= barSize;
                rect.X += (int)(rect.Width * 0.5 - 1);
                rect.Width = 1;
                DrawSliderNotchesV(rect, numNotches, barSize * 0.5f);

                Textures.Input.Slider.V.Back.Draw(Renderer, barrect);

                if (!control.IsDisabled)
                {
                    int h = (int)Math.Round(barrect.Height * val);
                    var prevclip = ClipArea(new Rectangle(barrect.X, barrect.Y + (rect.Height - h), barrect.Width, h));
                    Textures.Input.Slider.V.Front.Draw(Renderer, barrect);
                    Renderer.ClipRegion = prevclip;
                }
            }
        }
        /// <summary>
        /// Small helper functino to apply a clip + offset
        /// always remember to set renderer.clipregion to the returned rect when finished.
        /// </summary>
        private Rectangle ClipArea(Rectangle newclip)
        {
            var currentclip = Renderer.ClipRegion;
            Renderer.AddClipRegion(newclip);
            return currentclip;
        }

        public override void DrawSliderButton(Controls.ControlBase control, bool depressed, bool horizontal)
        {
            if (!horizontal)
            {
                if (control.IsDisabled)
                {
                    Textures.Input.Slider.V.Disabled.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                if (depressed)
                {
                    Textures.Input.Slider.V.Down.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                if (control.IsHovered)
                {
                    Textures.Input.Slider.V.Hover.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                Textures.Input.Slider.V.Normal.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsDisabled)
            {
                Textures.Input.Slider.H.Disabled.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                Textures.Input.Slider.H.Down.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsHovered)
            {
                Textures.Input.Slider.H.Hover.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            Textures.Input.Slider.H.Normal.DrawCenter(Renderer, control.RenderBounds);
        }

        public void DrawSliderNotchesH(Rectangle rect, int numNotches, float dist)
        {
            if (numNotches == 0) return;

            float iSpacing = rect.Width / (float)numNotches;
            for (int i = 0; i < numNotches + 1; i++)
                Renderer.DrawFilledRect(Util.FloatRect(rect.X + iSpacing * i, rect.Y + dist - 2, 1, 5));
        }

        public void DrawSliderNotchesV(Rectangle rect, int numNotches, float dist)
        {
            if (numNotches == 0) return;

            float iSpacing = rect.Height / (float)numNotches;
            for (int i = 0; i < numNotches + 1; i++)
                Renderer.DrawFilledRect(Util.FloatRect(rect.X + dist - 2, rect.Y + iSpacing * i, 5, 1));
        }

        public override void DrawStatusBar(Controls.ControlBase control)
        {
            Textures.StatusBar.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawTabButton(Controls.ControlBase control, bool active, Pos dir)
        {
            if (active)
            {
                DrawActiveTabButton(control, dir);
                return;
            }

            if (dir == Pos.Top)
            {
                Textures.Tab.Top.Inactive.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (dir == Pos.Left)
            {
                Textures.Tab.Left.Inactive.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (dir == Pos.Bottom)
            {
                Textures.Tab.Bottom.Inactive.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (dir == Pos.Right)
            {
                Textures.Tab.Right.Inactive.Draw(Renderer, control.RenderBounds);
                return;
            }
        }

        public override void DrawTabControl(Controls.ControlBase control)
        {
            Textures.Tab.Control.Draw(Renderer, control.RenderBounds);
        }


        public override void DrawTextBox(Controls.ControlBase control, bool focus)
        {
            if (control.IsDisabled)
            {
                Textures.TextBox.Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (focus)
                Textures.TextBox.Focus.Draw(Renderer, control.RenderBounds);
            else
                Textures.TextBox.Normal.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawToolTip(Controls.ControlBase control)
        {
            Textures.Tooltip.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawTreeButton(Controls.ControlBase control, bool open)
        {
            Rectangle rect = control.RenderBounds;

            if (open)
                Textures.Tree.Minus.Draw(Renderer, rect);
            else
                Textures.Tree.Plus.Draw(Renderer, rect);
        }

        public override void DrawTreeControl(Controls.ControlBase control)
        {
            Textures.Tree.Background.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawTreeNode(Controls.ControlBase ctrl, bool open, bool selected, int labelHeight, int labelWidth, int halfWay, int lastBranch, bool isRoot)
        {
            if (selected)
            {
                Textures.Selection.Draw(Renderer, new Rectangle(17, 0, labelWidth + 2, labelHeight - 1));
            }

            base.DrawTreeNode(ctrl, open, selected, labelHeight, labelWidth, halfWay, lastBranch, isRoot);
        }

        public override void DrawWindow(Controls.ControlBase control, int topHeight, bool inFocus)
        {
            if (inFocus)
                Textures.Window.Normal.Draw(Renderer, control.RenderBounds);
            else
                Textures.Window.Inactive.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawWindowCloseButton(Controls.ControlBase control, bool depressed, bool hovered, bool disabled, bool inactive)
        {
            if (disabled)
            {
                Textures.Window.Close_Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                Textures.Window.Close_Down.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (hovered)
            {
                Textures.Window.Close_Hover.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (inactive)
            {
                Textures.Window.Close_Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            Textures.Window.Close.Draw(Renderer, control.RenderBounds);
        }

        private void DrawActiveTabButton(Controls.ControlBase control, Pos dir)
        {
            if (dir == Pos.Top)
            {
                Textures.Tab.Top.Active.Draw(Renderer, control.RenderBounds.Add(new Rectangle(0, 0, 0, 8)));
                return;
            }
            if (dir == Pos.Left)
            {
                Textures.Tab.Left.Active.Draw(Renderer, control.RenderBounds.Add(new Rectangle(0, 0, 8, 0)));
                return;
            }
            if (dir == Pos.Bottom)
            {
                Textures.Tab.Bottom.Active.Draw(Renderer, control.RenderBounds.Add(new Rectangle(0, -8, 0, 8)));
                return;
            }
            if (dir == Pos.Right)
            {
                Textures.Tab.Right.Active.Draw(Renderer, control.RenderBounds.Add(new Rectangle(-8, 0, 8, 0)));
                return;
            }
        }

        #endregion UI elements
    }
}