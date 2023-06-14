﻿using Gwen.Controls;
using Gwen.Input;
using System;
using System.Drawing;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Base for controls that can be dragged by mouse.
    /// </summary>
    public class Dragger : ControlBase
    {
        #region Events

        /// <summary>
        /// Event invoked when the control position has been changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> Dragged;
        /// <summary>
        /// Invoked when the button is released.
        /// </summary>
        public event GwenEventHandler<EventArgs> Released;
        /// <summary>
        /// Invoked when the button is pressed.
        /// </summary>
        public event GwenEventHandler<EventArgs> Pressed;

        #endregion Events

        #region Properties

        /// <summary>
        /// Indicates if the control is being dragged.
        /// </summary>
        public bool IsHeld => m_Held;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Dragger"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Dragger(ControlBase parent) : base(parent)
        {
            ToolTipProvider = false;
            MouseInputEnabled = true;
            m_Held = false;
        }

        #endregion Constructors

        internal ControlBase Target { get => m_Target; set => m_Target = value; }
        protected bool m_Held;
        protected Point m_HoldPos;
        protected ControlBase m_Target;

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            if (null == m_Target)
                return;

            if (down)
            {
                m_Held = true;
                m_HoldPos = m_Target.CanvasPosToLocal(new Point(x, y));
                InputHandler.MouseFocus = this;
                Pressed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                m_Held = false;
                InputHandler.MouseFocus = null;

                Released?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handler invoked on mouse moved event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X change.</param>
        /// <param name="dy">Y change.</param>
        protected override void OnMouseMoved(int x, int y, int dx, int dy)
        {
            if (null == m_Target)
                return;
            if (!m_Held)
                return;

            Point p = new Point(x - m_HoldPos.X, y - m_HoldPos.Y);

            // Translate to parent
            if (m_Target.Parent != null)
                p = m_Target.Parent.CanvasPosToLocal(p);

            //m_Target->SetPosition( p.x, p.y );
            _ = m_Target.MoveClampToParent(p.X, p.Y);
            Dragged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
        }
    }
}