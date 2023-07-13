﻿using Gwen;
using Gwen.Controls;
using System;
using System.Drawing;

namespace linerider.UI.Components
{
    public class WidgetButton : ImageButton
    {
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
                if (_icon != value)
                {
                    _icon = value;
                    SetImage(_icon);
                    _ = SetSize(_icon.Width, _icon.Height);
                }
            }
        }
        public Hotkey Hotkey
        {
            get => _hotkey;
            set
            {
                InputUtils.FreeHotkey(_hotkey);
                _hotkey = value;
                InputUtils.RegisterHotkey(_hotkey, _hotkeycondition, () => Action(this, EventArgs.Empty));
            }
        }
        public Hotkey TooltipHotkey
        {
            get => _tooltiphotkey == Hotkey.None ? Hotkey : _tooltiphotkey;
            set => _tooltiphotkey = value;
        }
        public Func<bool> HotkeyCondition
        {
            set
            {
                _hotkeycondition = value;
                Hotkey = _hotkey;
            }
        }
        public Color Color
        {
            get => _color == Color.Empty ? Settings.Computed.LineColor : _color;
            set => _color = value;
        }

        public new Color TextColor = Color.Black;
        private Func<bool> _hotkeycondition = () => true;
        private GwenEventHandler<EventArgs> _action;
        private Hotkey _tooltiphotkey = Hotkey.None;
        private Hotkey _hotkey = Hotkey.None;
        private Bitmap _icon;
        private Color _color = Color.Empty;

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

                if (TooltipHotkey != Hotkey.None)
                    tooltip += Settings.HotkeyToString(TooltipHotkey, addHotkeyBrackets);

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
            m_Text.TextColor = TextColor;
            skin.Renderer.DrawColor = Color.FromArgb(Alpha, Color);
            skin.Renderer.DrawTexturedRect(m_texture, RenderBounds);
        }

        public override void Dispose()
        {
            m_texture?.Dispose();
            base.Dispose();
        }
    }
}
