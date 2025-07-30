using System;
using System.Collections.Generic;
namespace Gwen.Controls
{
    /// <summary>
    /// Tree control.
    /// </summary>
    public class TreeControl : ScrollControl
    {
        /// <summary>
        /// Invoked when a node's selected state has changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> SelectionChanged;

        #region Properties

        /// <summary>
        /// Determines if multiple nodes can be selected at the same time.
        /// </summary>
        public bool AllowMultiSelect { get; set; }

        public List<TreeNode> SelectedChildren
        {
            get
            {
                List<TreeNode> Trees = [];

                foreach (ControlBase child in Children)
                {
                    if (!(child is TreeNode node))
                        continue;
                    Trees.AddRange(node.SelectedChildren);
                }

                return Trees;
            }
        }
        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TreeControl(ControlBase parent)
            : base(parent)
        {
            KeyboardInputEnabled = true;
            AllowMultiSelect = false;
            m_Panel.BoundsOutlineColor = Color.Red;
            m_Panel.AutoSizeToContents = true;
        }

        #endregion Constructors

        #region Methods
        /// <summary>
        /// Adds a new child node.
        /// </summary>
        /// <param name="label">Node's label.</param>
        /// <returns>Newly created control.</returns>
        public TreeNode AddNode(string label)
        {
            TreeNode node = new(this)
            {
                Text = label
            };

            return node;
        }

        /// <summary>
        /// Handler for node added event.
        /// </summary>
        /// <param name="node">Node added.</param>
        public virtual void OnNodeAdded(TreeNode node)
        {
        }
        protected override void OnChildAdded(ControlBase child)
        {
            base.OnChildAdded(child);
            if (child is TreeNode)
            {
                ((TreeNode)child).TreeControl = this;
            }
        }

        /// <summary>
        /// Removes all child nodes.
        /// </summary>
        public virtual void RemoveAll() => DeleteAll();
        /// <summary>
        /// Opens the node and all child nodes.
        /// </summary>
        public void ExpandAll()
        {
            foreach (ControlBase child in Children)
            {
                if (!(child is TreeNode node))
                    continue;
                node.ExpandAll();
            }
        }
        /// <summary>
        /// Clears the selection on the node and all child nodes.
        /// </summary>
        public void UnselectAll()
        {
            foreach (ControlBase child in Children)
            {
                if (!(child is TreeNode node))
                    continue;
                node.UnselectAll();
            }
        }

        internal virtual void OnSelectionChanged(ControlBase sender, EventArgs args) => SelectionChanged?.Invoke(sender, EventArgs.Empty);

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            if (ShouldDrawBackground)
                skin.DrawTreeControl(this);
        }

        #endregion Methods

        #region Fields

        #endregion Fields
    }
}