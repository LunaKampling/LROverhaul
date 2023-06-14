using Gwen.Controls;
using System;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Properties node.
    /// </summary>
    public class PropertyTreeNode : TreeNode
    {
        internal PropertyTable Table;
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTreeNode"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyTreeNode(ControlBase parent)
            : base(parent)
        {
            m_Title.TextColorOverride = Skin.Colors.Foreground;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawPropertyTreeNode(this, m_Panel.X, m_Panel.Y);
        protected override void OnClickName(ControlBase control, EventArgs args)
        {
            if (!m_ToggleButton.IsVisible)
                return;
            m_ToggleButton.Toggle();
        }
        protected override void OnDoubleClickName(ControlBase control, EventArgs args)
        {
            // Ignored
        }
    }
}
