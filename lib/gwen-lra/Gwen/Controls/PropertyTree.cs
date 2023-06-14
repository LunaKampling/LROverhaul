using Gwen.ControlInternal;

namespace Gwen.Controls
{
    /// <summary>
    /// Property table/tree.
    /// </summary>
    public class PropertyTree : TreeControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyTree(ControlBase parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Adds a new properties node.
        /// </summary>
        /// <param name="label">Node label.</param>
        /// <returns>Newly created control</returns>
        public PropertyTable Add(string label, int startingbarposition = 80)
        {
            PropertyTreeNode node = new PropertyTreeNode(this)
            {
                Text = label
            };
            PropertyTable props = new PropertyTable(node, startingbarposition)
            {
                Dock = Dock.Fill,
                AutoSizeToContents = true
            };
            node.Table = props;
            return props;
        }
        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawCategoryHolder(this);
    }
}
