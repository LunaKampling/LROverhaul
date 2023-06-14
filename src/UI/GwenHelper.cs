using Gwen;
using Gwen.Controls;
using System;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace linerider.UI
{
    public static class GwenHelper
    {
        public class ColorEventArgs : EventArgs
        {
            public readonly Color _color;
            public ColorEventArgs(Color color)
            {
                _color = color;
            }
            public Color Value => _color;
        }
        public class ColorIndicator : ControlBase
        {
            public Color Color = Color.White;
            public bool Error = false;
            protected override void Render(Gwen.Skin.SkinBase skin)
            {
                base.Render(skin);

                skin.Renderer.DrawColor = Error ? Color.FromArgb(255, 249, 249, 249) : Color.FromArgb(255, Color);
                skin.Renderer.DrawFilledRect(new Rectangle(0, 0, Width, Height));

                skin.Renderer.DrawColor = Error ? Color.Red : Color.Black;
                skin.Renderer.DrawLinedRect(new Rectangle(0, 0, Width, Height));

                skin.Renderer.DrawColor = Color.FromArgb(16, skin.Renderer.DrawColor);
                skin.Renderer.DrawLinedRect(new Rectangle(1, 1, Width - 2, Height - 2));
            }
        }

        public static CheckProperty AddPropertyCheckbox(PropertyTable prop, string label, bool value)
        {
            CheckProperty check = new CheckProperty(null);
            _ = prop.Add(label, check);
            check.IsChecked = value;
            return check;
        }
        public static Checkbox AddCheckbox(ControlBase parent, string text, bool val, ControlBase.GwenEventHandler<EventArgs> checkedchanged, Dock dock = Dock.Top)
        {
            Checkbox check = new Checkbox(parent)
            {
                Dock = dock,
                Text = text,
                IsChecked = val,
            };
            check.CheckChanged += checkedchanged;
            return check;
        }
        public static Panel CreateHeaderPanel(ControlBase parent, string headertext)
        {
            GameCanvas canvas = (GameCanvas)parent.GetCanvas();
            Panel panel = new Panel(parent)
            {
                Dock = Dock.Top,
                Children =
                {
                    new Label(parent)
                    {
                        Dock = Dock.Top,
                        Text = headertext,
                        Alignment = Pos.Left | Pos.CenterV,
                        Font = canvas.Fonts.DefaultBold,
                        Margin = new Margin(-10, 5, 0, 5)
                    }
                },
                AutoSizeToContents = true,
                Margin = new Margin(0, 0, 0, 10),
                Padding = new Padding(10, 0, 5, 0),
                ShouldDrawBackground = false
            };
            return panel;
        }
        public static Panel CreateHintLabel(ControlBase parent, string headertext)
        {
            Panel panel = new Panel(parent)
            {
                Dock = Dock.Top,
                Children =
                {
                    new Label(parent)
                    {
                        TextColor = Color.Gray,
                        Dock = Dock.Top,
                        Text = headertext,
                        Alignment = Pos.Left | Pos.CenterV,
                        Margin = new Margin(-10, 5, 0, 0)
                    }
                },
                AutoSizeToContents = true,
                Margin = new Margin(0, 0, 0, 10),
                Padding = new Padding(10, 0, 0, 0),
                ShouldDrawBackground = false
            };
            return panel;
        }
        public static ControlBase CreateLabeledControl(ControlBase parent, string label, ControlBase[] controls)
        {
            foreach (ControlBase control in controls)
            {
                control.Dock = Dock.Right;
            }
            ControlBase container = new ControlBase(parent)
            {
                Children =
                {
                    new Label(null)
                    {
                        Text = label,
                        Dock = Dock.Left,
                        Alignment = Pos.Left | Pos.CenterV,
                        Margin = new Margin(0,0,10,0)
                    },
                },
                AutoSizeToContents = true,
                Dock = Dock.Top,
                Margin = new Margin(0, 1, 0, 1)
            };
            for (int i = controls.Length - 1; i >= 0; i--)
            {
                container.Children.Add(controls[i]);
            }
            return container;
        }
        public static ControlBase CreateLabeledControl(ControlBase parent, string label, ControlBase control) => CreateLabeledControl(parent, label, new ControlBase[1] { control });
        public static ComboBox CreateLabeledCombobox(ControlBase parent, string label)
        {
            ComboBox combobox = new ComboBox(null)
            {
                Dock = Dock.Right,
                Width = 100
            };
            _ = new ControlBase(parent)
            {
                Children =
                {
                    new Label(null)
                    {
                        Text = label,
                        Dock = Dock.Left,
                        Alignment = Pos.Left | Pos.CenterV,
                        Margin = new Margin(0,0,10,0)
                    },
                    combobox
                },
                AutoSizeToContents = true,
                Dock = Dock.Top,
                Margin = new Margin(0, 1, 0, 1)
            };
            return combobox;
        }

        /// <summary>
        /// Creates labeled input field that accepts hex color values in 1, 2, 3 or 6 digits format.
        /// </summary>
        /// <param name="valuechanged">Event that receives <see cref="Color"/>. Called only when a valid color is specified.</param>
        public static ControlBase CreateLabeledColorInput(ControlBase parent, string label, Color defaultColor, ControlBase.GwenEventHandler<ColorEventArgs> valuechanged)
        {
            ControlBase container = new ControlBase(parent)
            {
                AutoSizeToContents = true,
                Dock = Dock.Top,
                Margin = new Margin(0, 1, 0, 1)
            };

            Label description = new Label(null)
            {
                Text = label,
                Dock = Dock.Left,
                Alignment = Pos.Left | Pos.CenterV,
                Margin = new Margin(0, 0, 10, 0)
            };

            Label inputPrefix = new Label(null)
            {
                Text = "#",
                Dock = Dock.Right,
                Alignment = Pos.Left | Pos.CenterV,
                Margin = new Margin(0, 0, 5, 0)
            };

            TextBox input = new TextBox(null)
            {
                Name = "input",
                Width = 70,
                Dock = Dock.Right,
            };

            ColorIndicator preview = new ColorIndicator()
            {
                Name = "preview",
                Margin = new Margin(2, 0, 0, 0),
                Width = 20,
                Dock = Dock.Right,
            };

            string hexR = $"{defaultColor.R:X2}";
            string hexG = $"{defaultColor.G:X2}";
            string hexB = $"{defaultColor.B:X2}";
            string defaultColorHex = hexR[0] == hexR[1] && hexG[0] == hexG[1] && hexB[0] == hexB[1]
                ? hexR[0].ToString() + hexG[0].ToString() + hexB[0].ToString()
                : hexR + hexG + hexB;

            //// #F & #FF
            //if (hexR == hexG && hexG == hexB)
            //{
            //    bool isOneDigit = hexR[0] == hexR[1];
            //    defaultColorHex = isOneDigit ? hexR[0].ToString() : hexR;
            //}

            // #FFF

            input.Text = defaultColorHex;
            string oldValue = defaultColorHex;
            bool isValidating = false;

            input.TextChanged += (o, e) =>
            {
                if (isValidating)
                    return;
                isValidating = true;

                string newValue = input.Text.ToUpper();

                bool isHex = int.TryParse(newValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);
                if (!isHex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (char c in newValue)
                        if ((c >= 'A' && c <= 'F') || (c >= '0' && c <= '9'))
                            _ = sb.Append(c);
                    newValue = sb.ToString();
                }

                if (newValue.Length > 6)
                    newValue = input.Text.Substring(0, 6);

                bool isDifferentValue = newValue != oldValue;
                if (isDifferentValue)
                {
                    string colorRaw = string.Empty;
                    oldValue = newValue;

                    switch (newValue.Length)
                    {
                        case 1: colorRaw = new string(newValue[0], 6); break;
                        case 2: colorRaw = string.Concat(System.Linq.Enumerable.Repeat(newValue, 3)); break;
                        case 3: colorRaw = string.Concat(newValue[0], newValue[0], newValue[1], newValue[1], newValue[2], newValue[2]); break;
                        case 6: colorRaw = newValue; break;
                    }

                    bool isValidValue = colorRaw != string.Empty;
                    if (isValidValue)
                    {
                        int argb = int.Parse($"FF{colorRaw}", NumberStyles.HexNumber);
                        Color color = Color.FromArgb(argb);

                        ColorEventArgs colorArg = new ColorEventArgs(color);
                        preview.Color = color;

                        valuechanged(o, colorArg);
                    }

                    preview.Error = !isValidValue;
                    input.TextColorOverride = isValidValue ? Color.FromArgb(0, Color.Red) : Color.Red; // A==0, override disabled
                }

                input.Text = newValue;

                isValidating = false;
            };

            preview.Color = defaultColor;

            container.Children.Add(preview);
            container.Children.Add(input);
            container.Children.Add(inputPrefix);
            container.Children.Add(description);

            return container;
        }
    }
}