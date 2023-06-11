using Gwen.Controls;
using Gwen;
using System;
using System.Drawing;
using linerider.Tools;
using System.Collections.Generic;

namespace linerider.UI.Components
{
    public class WidgetButton : ImageButton
    {
        private GwenEventHandler<EventArgs> _action;
        private Hotkey _hotkey;
        private Bitmap _icon;

        public GwenEventHandler<EventArgs> Action
        {
            get => _action;
            set
            {
                Clicked -= _action;
                _action = value;
                Clicked += _action;
            }
        }
        public Bitmap Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                SetImage(_icon);
                SetSize(_icon.Width, _icon.Height);
            }
        }
        public Hotkey Hotkey
        {
            get => _hotkey;
            set
            {
                InputUtils.FreeHotkey(_hotkey);
                _hotkey = value;
                InputUtils.RegisterHotkey(_hotkey, () => true, () => Action(this, EventArgs.Empty));
            }
        }

        public WidgetButton(ControlBase parent) : base(parent)
        {
            Dock = Dock.Left;
            Name = "";
        }
        public override string Tooltip
        {
            get
            {
                bool addHotkeyBrackets = !string.IsNullOrEmpty(Name);
                string tooltip = Name;

                if (Hotkey != Hotkey.None)
                    tooltip += Settings.HotkeyToString(Hotkey, addHotkeyBrackets);

                return tooltip;
            }
        }
        public override void Think()
        {
            Alpha = (byte)(IsDepressed ? 64 : (IsHovered ? 128 : 255));
            base.Think();
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            skin.Renderer.DrawColor = Color.FromArgb(Alpha, Settings.Computed.LineColor);
            skin.Renderer.DrawTexturedRect(m_texture, RenderBounds);
        }
    }
}
