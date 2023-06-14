using Gwen.ControlInternal;
using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Base resizable control.
    /// </summary>
    public class ResizableControl : Container
    {
        protected readonly Resizer[] m_Resizers;
        protected Resizer _resizer_top;
        protected Resizer _resizer_bottom;
        protected Resizer _resizer_left;
        protected Resizer _resizer_right;

        /// <summary>
        /// Determines whether control's position should be restricted to its parent bounds.
        /// </summary>
        public bool ClampMovement { get; set; }
        /// <summary>
        /// True if DisableResizing should hide the resizers
        /// </summary>
        protected bool HideResizersOnDisable = true;
        /// <summary>
        /// Invoked when the control has been resized.
        /// </summary>
		public event GwenEventHandler<EventArgs> Resized;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizableControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ResizableControl(ControlBase parent)
            : base(parent)
        {
            MinimumSize = new Size(5, 5);
            ClampMovement = false;

            _resizer_bottom = new Resizer(null)
            {
                Dock = Dock.Bottom,
                ResizeDir = Pos.Bottom
            };
            _resizer_bottom.Resized += OnResized;
            _resizer_bottom.Target = this;

            _resizer_top = new Resizer(null)
            {
                Dock = Dock.Top,
                ResizeDir = Pos.Top
            };
            _resizer_top.Resized += OnResized;
            _resizer_top.Target = this;

            _resizer_left = new Resizer(null)
            {
                Dock = Dock.Left,
                ResizeDir = Pos.Left
            };
            _resizer_left.Resized += OnResized;
            _resizer_left.Target = this;

            _resizer_right = new Resizer(null)
            {
                Dock = Dock.Right,
                ResizeDir = Pos.Right
            };
            _resizer_right.Resized += OnResized;
            _resizer_right.Target = this;

            Resizer sizebl = new Resizer(_resizer_bottom)
            {
                Dock = Dock.Left,
                ResizeDir = Pos.Bottom | Pos.Left
            };
            sizebl.Resized += OnResized;
            sizebl.Target = this;

            Resizer sizebr = new Resizer(_resizer_bottom)
            {
                Dock = Dock.Right,
                ResizeDir = Pos.Bottom | Pos.Right
            };
            sizebr.Resized += OnResized;
            sizebr.Target = this;

            Resizer sizetl = new Resizer(_resizer_top)
            {
                Dock = Dock.Left,
                ResizeDir = Pos.Left | Pos.Top
            };
            sizetl.Resized += OnResized;
            sizetl.Target = this;

            Resizer sizetr = new Resizer(_resizer_top)
            {
                Dock = Dock.Right,
                ResizeDir = Pos.Right | Pos.Top
            };
            sizetr.Resized += OnResized;
            sizetr.Target = this;

            PrivateChildren.Insert(0, _resizer_top);
            PrivateChildren.Insert(1, _resizer_bottom);
            PrivateChildren.Insert(2, _resizer_right);
            PrivateChildren.Insert(3, _resizer_left);
            m_Resizers = new Resizer[]
            {
                _resizer_top,
                _resizer_bottom,
                _resizer_left,
                _resizer_right,
                sizetr,
                sizetl,
                sizebr,
                sizebl
            };
        }

        /// <summary>
        /// Handler for the resized event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void OnResized(ControlBase control, EventArgs args) => Resized?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Disables resizing.
        /// </summary>
        public virtual void DisableResizing()
        {
            for (int i = 0; i < m_Resizers.Length; i++)
            {
                if (m_Resizers[i] == null)
                    continue;
                m_Resizers[i].MouseInputEnabled = false;
                if (HideResizersOnDisable)
                    m_Resizers[i].IsHidden = true;
            }
        }

        /// <summary>
        /// Enables resizing.
        /// </summary>
        public void EnableResizing()
        {
            for (int i = 0; i < m_Resizers.Length; i++)
            {
                if (m_Resizers[i] == null)
                    continue;
                m_Resizers[i].MouseInputEnabled = true;
                m_Resizers[i].IsHidden = false;
            }
        }

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>
        /// True if bounds changed.
        /// </returns>
        public override bool SetBounds(int x, int y, int width, int height)
        {
            Size minSize = MinimumSize;
            // Clamp Minimum Size
            if (width < minSize.Width)
                width = minSize.Width;
            if (height < minSize.Height)
                height = minSize.Height;

            // Clamp to parent's window
            ControlBase parent = Parent;
            if (parent != null && ClampMovement)
            {
                if (x + width > parent.Width)
                    x = parent.Width - width;
                if (x < 0)
                    x = 0;
                if (y + height > parent.Height)
                    y = parent.Height - height;
                if (y < 0)
                    y = 0;
            }

            return base.SetBounds(x, y, width, height);
        }

        /// <summary>
        /// Sets the control size.
        /// </summary>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <returns>True if bounds changed.</returns>
        public override bool SetSize(int width, int height)
        {
            bool Changed = base.SetSize(width, height);
            if (Changed)
            {
                OnResized(this, EventArgs.Empty);
            }
            return Changed;
        }
    }
}
