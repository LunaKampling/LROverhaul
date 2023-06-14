using Gwen.Skin;

namespace Gwen.Controls
{
    /// <summary>
    /// Label property.
    /// </summary>
    public class LabelProperty : PropertyBase
    {
        protected readonly Label m_text;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelProperty"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public LabelProperty(ControlBase parent) : base(parent)
        {
            m_text = new Label(this)
            {
                Dock = Dock.Fill,
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                Alignment = Pos.CenterV | Pos.Left,
                TextPadding = new Padding(2, 2, 2, 2),
            };
            AutoSizeToContents = true;
        }

        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get => m_text.Text;
            set
            {
                base.Value = value;
                m_text.SetText(value);
            }
        }
        public Font Font
        {
            get => m_text.Font;
            set => m_text.Font = value;
        }
        protected override void Render(SkinBase skin)
        {
            if (ShouldDrawBackground)
            {
                skin.Renderer.DrawColor = Skin.Colors.ForegroundHighlight;
                System.Drawing.Rectangle r = RenderBounds;
                skin.Renderer.DrawFilledRect(r);
            }
            base.Render(skin);
        }
        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void SetValue(string value, bool fireEvents = false) => m_text.SetText(value, fireEvents);

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing => m_text.HasFocus;

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered => base.IsHovered | m_text.IsHovered;
    }
}
