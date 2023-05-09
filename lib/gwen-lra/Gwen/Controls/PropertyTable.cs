using Gwen.ControlInternal;
using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Properties table.
    /// </summary>
    public class PropertyTable : Container
    {
        #region Events

        /// <summary>
        /// Invoked when a property value has been changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> ValueChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Returns the width of the first column (property names).
        /// </summary>
        public int SplitWidth
        {
            get
            {
                return m_SplitterBar.X;
            }
            set
            {
                m_SplitterBar.X = value;
            }
        }

        #endregion Properties

        #region Constructors

        // todo: rename?
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTable"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyTable(ControlBase parent, int StartingBarPosition = 80)
            : base(parent)
        {
            Width = StartingBarPosition + 50;
            m_SplitterBar = new SplitterBar(null);
            m_SplitterBar.SetPosition(StartingBarPosition, 0);
            m_SplitterBar.Cursor = Cursors.SizeWE;
            m_SplitterBar.Dragged += OnSplitterMoved;
            m_SplitterBar.ShouldDrawBackground = false;
            PrivateChildren.Add(m_SplitterBar);
            m_Panel.AutoSizeToContents = true;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Adds a new text property row.
        /// </summary>
        /// <param name="label">Property name.</param>
        /// <param name="value">Initial value.</param>
        /// <returns>Newly created row.</returns>
        public PropertyRow Add(string label, string value = "")
        {
            var tx = new TextProperty(this);
            var ret = Add(label, tx);
            tx.Value = value;
            return ret;
        }
        /// <summary>
        /// Adds a new label property row.
        /// </summary>
        /// <param name="label">Property name.</param>
        /// <param name="value">Initial value.</param>
        /// <returns>Newly created row.</returns>
        public PropertyRow AddLabel(string label, string value)
        {
            var tx = new LabelProperty(this);
            var ret = Add(label, tx);
            tx.Value = value;
            return ret;
        }

        /// <summary>
        /// Adds a new property row.
        /// </summary>
        /// <param name="label">Property name.</param>
        /// <param name="prop">Property control.</param>
        /// <returns>Newly created row.</returns>
        public PropertyRow Add(string label, PropertyBase prop)
        {
            PropertyRow row = new PropertyRow(this, prop);
            row.Dock = Dock.Top;
            row.Label = label;
            row.ValueChanged += OnRowValueChanged;
            m_SplitterBar.BringToFront();
            return row;
        }


        /// <summary>
        /// Deletes all rows.
        /// </summary>
        public void DeleteAll()
        {
            m_Panel.DeleteAllChildren();
        }

        /// <summary>
        /// Handles the splitter moved event.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void OnSplitterMoved(ControlBase control, EventArgs args)
        {
            InvalidateChildren();
        }
        protected override void ProcessLayout(System.Drawing.Size size)
        {
            m_SplitterBar.SetSize(5, Height);
            base.ProcessLayout(size);
        }
        public override Size GetSizeToFitContents()
        {
            var ret = base.GetSizeToFitContents();
            ret.Width = Math.Max(m_SplitterBar.X + 50, ret.Width);
            return ret;
        }

        #endregion Methods

        #region Fields

        private readonly SplitterBar m_SplitterBar;

        #endregion Fields

        private void OnRowValueChanged(ControlBase control, EventArgs args)
        {
            if (ValueChanged != null)
                ValueChanged.Invoke(control, EventArgs.Empty);
        }
    }
}