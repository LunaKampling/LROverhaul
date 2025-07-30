using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gwen.Controls
{
    /// <summary>
    /// Base control class.
    /// </summary>
    public partial class ControlBase : IDisposable
    {
        /// <summary>
        /// Delegate used for all control event handlers.
        /// </summary>
        /// <param name="control">Event source.</param>
        /// <param name="args" >Additional arguments. May be empty (EventArgs.Empty).</param>
        public delegate void GwenEventHandler<in T>(ControlBase sender, T arguments) where T : EventArgs;

        private bool m_Disposed;

        private ControlBase m_Parent;

        private Skin.SkinBase m_Skin;

        private Rectangle m_Bounds;
        private Padding m_Padding;
        private Margin m_Margin;
        private bool m_Disabled;
        private bool m_Hidden;
        private bool m_MouseInputEnabled;
        private Dock m_Dock;
        private bool m_CacheTextureDirty;
        private Package m_DragAndDrop_Package;
        private bool m_DrawDebugOutlines;

        /// <summary>
        /// Real list of children.
        /// </summary>
        private readonly ControlCollection m_Children;

        /// <summary>
        /// Accelerator map.
        /// </summary>
        private readonly Dictionary<string, GwenEventHandler<EventArgs>> m_Accelerators;

        public const int MaxCoord = 4096; // Added here from various places in code

        /// <summary>
        /// Logical list of children. If InnerPanel is not null, returns InnerPanel's children.
        /// </summary>
        public virtual ControlCollection Children => m_Children;

        /// <summary>
        /// The logical parent. It's usually what you expect, the control you've parented it to.
        /// </summary>
        public ControlBase Parent
        {
            get => m_Parent;
            set
            {
                if (m_Parent == value)
                    return;

                m_Parent?.RemoveChild(this, false);

                if (value != null && value.Parent == this)
                    throw new InvalidOperationException("Cannot assign a parent that is a child of this control");
                if (value == this)
                    throw new InvalidOperationException("A control cannot parent itself self");
                m_Parent = value;

                if (m_Parent != null)
                {
                    m_Parent.AddChild(this);
                    if (m_Parent.DrawDebugOutlines)
                        DrawDebugOutlines = true;
                }
            }
        }

        // TODO: ParentChanged event?

        /// <summary>
        /// Dock position.
        /// </summary>
        public virtual Dock Dock
        {
            get => m_Dock;
            set
            {
                if (m_Dock == value)
                    return;
                switch (value)
                {
                    case Dock.Right:
                    case Dock.Top:
                    case Dock.Bottom:
                    case Dock.Fill:
                    case Dock.None:
                    case Dock.Left:
                        m_Dock = value;
                        Invalidate();
                        InvalidateParent();
                        break;
                    default:
                        throw new Exception("Unsupported Dock type " + value);
                }
            }
        }

        /// <summary>
        /// Current skin.
        /// </summary>
        public Skin.SkinBase Skin => m_Skin ?? (m_Parent != null ? m_Parent.Skin : Gwen.Skin.SkinBase.DefaultSkin ?? throw new InvalidOperationException("GetSkin: null"));

        /// <summary>
        /// Indicates whether this control is a menu component.
        /// </summary>
        internal virtual bool IsMenuComponent => m_Parent != null && m_Parent.IsMenuComponent;

        /// <summary>
        /// Determines whether the control should be clipped to its bounds while rendering.
        /// </summary>
        protected virtual bool ShouldClip => true;

        /// <summary>
        /// Current padding - inner spacing.
        /// </summary>
        public virtual Padding Padding
        {
            get => m_Padding;
            set
            {
                if (m_Padding == value)
                    return;

                m_Padding = value;
                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Current margin - outer spacing.
        /// </summary>
        public virtual Margin Margin
        {
            get => m_Margin;
            set
            {
                if (m_Margin == value)
                    return;

                m_Margin = value;
                Invalidate();
                InvalidateParent();
            }
        }

        private string _tooltip = null;

        public virtual string Tooltip
        {
            get => _tooltip;
            set => _tooltip = value;
        }

        /// <summary>
        /// Indicates whether the control is on top of its parent's children.
        /// </summary>
        public virtual bool IsOnTop => m_Parent != null && m_Parent.Children.Count != 0 && m_Parent.Children[^1] == this;

        /// <summary>
        /// User data associated with the control.
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public virtual bool IsHovered => InputHandler.HoveredControl == this;

        /// <summary>
        /// Indicates whether the control has focus.
        /// </summary>
        public bool HasFocus => InputHandler.KeyboardFocus == this;

        /// <summary>
        /// Indicates whether the control is disabled.
        /// </summary>
        public bool IsDisabled
        {
            get => m_Disabled || (m_Parent != null && m_Parent.IsDisabled);
            set => m_Disabled = value;
        }

        /// <summary>
        /// Indicates whether the control is hidden.
        /// </summary>
        public virtual bool IsHidden
        {
            get => m_Hidden;
            set
            {
                if (value == m_Hidden)
                    return;
                m_Hidden = value;
                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Determines whether the control's position should be restricted to parent's bounds.
        /// </summary>
        public bool RestrictToParent { get; set; }

        /// <summary>
        /// Determines whether the control receives mouse input events.
        /// </summary>
        public virtual bool MouseInputEnabled { get => m_MouseInputEnabled; set => m_MouseInputEnabled = value; }

        /// <summary>
        /// Determines whether the control receives keyboard input events.
        /// </summary>
        public bool KeyboardInputEnabled { get; set; }

        /// <summary>
        /// Gets or sets the mouse cursor when the cursor is hovering the control.
        /// </summary>
        public Cursor Cursor { get; set; }

        /// <summary>
        /// Indicates whether the control is tabable (can be focused by pressing Tab).
        /// </summary>
        public bool IsTabable { get; set; }

        /// <summary>
        /// Indicates whether control's background should be drawn during rendering.
        /// </summary>
        public bool ShouldDrawBackground { get; set; }

        /// <summary>
        /// Indicates whether the renderer should cache drawing to a texture to improve performance (at the cost of memory).
        /// </summary>
        public bool ShouldCacheToTexture { get; set; }

        /// <summary>
        /// Gets or sets the control's internal name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Control's size and position relative to the parent.
        /// </summary>
        public Rectangle Bounds => m_Bounds;

        /// <summary>
        /// Size restriction.
        /// </summary>
        public Size MinimumSize
        {
            get => m_MinimumSize;
            set
            {
                m_MinimumSize = value;
                if (Width < m_MinimumSize.Width || Height < m_MinimumSize.Height)
                {
                    _ = SetSize(
                        Math.Max(m_MinimumSize.Width, Width),
                        Math.Max(m_MinimumSize.Height, Height));
                }
            }
        }

        /// <summary>
        /// Size restriction.
        /// </summary>
        public Size MaximumSize
        {
            get => m_MaximumSize;
            set
            {
                m_MaximumSize = value;
                if (Width > m_MaximumSize.Width || Height > m_MaximumSize.Height)
                {
                    _ = SetSize(
                        Math.Min(m_MaximumSize.Width, Width),
                        Math.Min(m_MaximumSize.Height, Height));
                }
            }
        }
        public event GwenEventHandler<EventArgs> OnThink;
        private Size m_MinimumSize = new(1, 1);
        private Size m_MaximumSize = new(int.MaxValue, int.MaxValue);

        /// <summary>
        /// Determines whether hover should be drawn during rendering.
        /// </summary>
        protected bool ShouldDrawHover => InputHandler.MouseFocus == this || InputHandler.MouseFocus == null;

        protected virtual bool AccelOnlyFocus => false;
        protected virtual bool NeedsInputChars => false;

        /// <summary>
        /// Indicates whether the control and its parents are visible.
        /// </summary>
        public bool IsVisible => !IsHidden && (Parent == null || Parent.IsVisible);

        /// <summary>
        /// Leftmost coordinate of the control.
        /// </summary>
        public int X { get => m_Bounds.X; set => SetPosition(value, Y); }

        /// <summary>
        /// Topmost coordinate of the control.
        /// </summary>
        public int Y { get => m_Bounds.Y; set => SetPosition(X, value); }

        public int Width { get => m_Bounds.Width; set => SetSize(value, Height); }
        public int Height { get => m_Bounds.Height; set => SetSize(Width, value); }
        public Size Size { get => m_Bounds.Size; set => SetSize(value.Width, value.Height); }
        /// <summary>
        /// The size of the control as understood by its children
        /// This is for containers to implement
        /// TODO: this is clientsize
        /// </summary>
        public virtual Size InnerSize => m_Bounds.Size;
        public int Bottom => m_Bounds.Bottom;
        public int Right => m_Bounds.Right;

        /// <summary>
        /// Determines whether margin, padding and bounds outlines for the control will be drawn. Applied recursively to all children.
        /// </summary>
        public bool DrawDebugOutlines
        {
            get => m_DrawDebugOutlines;
            set
            {
                if (m_DrawDebugOutlines == value)
                    return;
                m_DrawDebugOutlines = value;
                foreach (ControlBase child in Children)
                {
                    child.DrawDebugOutlines = value;
                }
            }
        }
        public Color PaddingOutlineColor { get; set; }
        public Color MarginOutlineColor { get; set; }
        public Color BoundsOutlineColor { get; set; }
        public virtual bool ToolTipProvider { get; set; } = true;
        /// <summary>
        /// Delay in milliseconds until a tooltip can pop up
        /// </summary>
        /// <returns></returns>
        public virtual int TooltipDelay { get; set; } = 400;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlBase"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ControlBase(ControlBase parent = null)
        {
            m_Children = new ControlCollection(this);
            m_Accelerators = [];
            Parent = parent;

            m_Hidden = false;
            m_Bounds = new Rectangle(0, 0, 10, 10);
            m_Padding = Padding.Zero;
            m_Margin = Margin.Zero;

            RestrictToParent = false;

            m_MouseInputEnabled = true;
            KeyboardInputEnabled = false;

            Cursor = Cursors.Default;
            //ToolTip = null;
            IsTabable = false;
            ShouldDrawBackground = true;
            m_Disabled = false;
            m_CacheTextureDirty = true;
            ShouldCacheToTexture = false;
            NeedsLayout = true;

            BoundsOutlineColor = Color.Red;
            MarginOutlineColor = Color.Green;
            PaddingOutlineColor = Color.Blue;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            //Debug.Print("Control.Base: Disposing {0} {1:X}", this, GetHashCode());
            if (m_Disposed)
            {
#if DEBUG
                throw new ObjectDisposedException(string.Format("Control.Base [{1:X}] disposed twice: {0}", this, GetHashCode()));
#else
                return;
#endif
            }

            if (InputHandler.HoveredControl == this)
                InputHandler.HoveredControl = null;
            if (InputHandler.KeyboardFocus == this)
                InputHandler.KeyboardFocus = null;
            if (InputHandler.MouseFocus == this)
                InputHandler.MouseFocus = null;

            DragAndDrop.ControlDeleted(this);
            Animation.Cancel(this);

            foreach (ControlBase child in m_Children)
                child.Dispose();

            m_Children.Clear();

            m_Disposed = true;
            GC.SuppressFinalize(this);
        }

#if DEBUG

        ~ControlBase()
        {
            Debug.WriteLine(string.Format("IDisposable object finalized: {0}", GetType()));
        }

#endif

        /// <summary>
        /// Detaches the control from canvas and adds to the deletion queue (processed in Canvas.DoThink).
        /// </summary>
        public void DelayedDelete() => GetCanvas().AddDelayedDelete(this);

        public override string ToString() => this is Label
                ? "[" + GetType().ToString() + ": " + (this as Label).Text + "]"
                : this is ControlInternal.Text ? "[Text: " + (this as ControlInternal.Text).String + "]" : GetType().ToString();

        /// <summary>
        /// Gets the canvas (root parent) of the control.
        /// </summary>
        /// <returns></returns>
        public virtual Canvas GetCanvas() => m_Parent?.GetCanvas();

        public string GetTooltip() => ToolTipProvider ? Tooltip : (Parent?.GetTooltip());
        public int GetTooltipDelay() => ToolTipProvider || Parent == null ? TooltipDelay : Parent.TooltipDelay;
        /// <summary>
        /// Enables the control.
        /// </summary>
        public void Enable() => IsDisabled = false;

        /// <summary>
        /// Disables the control.
        /// </summary>
        public virtual void Disable() => IsDisabled = true;

        /// <summary>
        /// Hides the control.
        /// </summary>
        public virtual void Hide() => IsHidden = true;

        /// <summary>
        /// Shows the control.
        /// </summary>
        public virtual void Show() => IsHidden = false;

        /// <summary>
        /// Creates a tooltip for the control.
        /// </summary>
        /// <param name="text">Tooltip text.</param>
        public virtual void SetToolTipText(string text) => Tooltip = text;

        /// <summary>
        /// Called during rendering.
        /// </summary>
        public virtual void Think()
        {
            foreach (ControlBase child in m_Children)
            {
                OnThink?.Invoke(this, EventArgs.Empty);
                // Ignore parent hidden values, as we are recursing down
                if (!child.m_Hidden && !child.m_Disabled)
                {
                    child.Think();
                }
            }
        }
        /// <summary>
        /// Finds a child by name.
        /// </summary>
        /// <param name="name">Child name.</param>
        /// <param name="recursive">Determines whether the search should be recursive.</param>
        /// <returns>Found control or null.</returns>
        public virtual ControlBase FindChildByName(string name, bool recursive = false)
        {
            foreach (ControlBase child in Children)
            {
                if (child.Name == name)
                    return child;
            }

            if (recursive)
            {
                foreach (ControlBase child in Children)
                {
                    ControlBase b = child.FindChildByName(name, true);
                    if (b != null)
                        return b;
                }
            }
            return null;
        }

        /// <summary>
        /// Handler invoked when control's scale changes.
        /// </summary>
        protected virtual void OnScaleChanged()
        {
            foreach (ControlBase child in m_Children)
            {
                child.OnScaleChanged();
            }
        }

        /// <summary>
        /// Gets a child by its coordinates.
        /// </summary>
        /// <param name="x">Child X.</param>
        /// <param name="y">Child Y.</param>
        /// <returns>Control or null if not found.</returns>
        public virtual ControlBase GetControlAt(int x, int y)
        {
            if (IsHidden)
                return null;

            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return null;

            for (int i = m_Children.Count - 1; i >= 0; i--)
            {
                ControlBase child = m_Children[i];
                ControlBase found = child.GetControlAt(x - child.X, y - child.Y);
                if (found != null)
                    return found;
            }

            return !MouseInputEnabled ? null : this;
        }

        /// <summary>
        /// Converts local coordinates to canvas coordinates.
        /// </summary>
        /// <param name="pnt">Local coordinates.</param>
        /// <returns>Canvas coordinates.</returns>
        public virtual Point LocalPosToCanvas(Point pnt)
        {
            if (m_Parent != null)
            {
                int x = pnt.X + X;
                int y = pnt.Y + Y;

                // If our parent is a container and we're a child of it
                // add its offset onto us.
                //
                if (m_Parent is Container container && container.Children.Contains(this))
                {
                    x += container.PanelBounds.X;
                    y += container.PanelBounds.Y;
                }

                return m_Parent.LocalPosToCanvas(new Point(x, y));
            }

            return pnt;
        }

        /// <summary>
        /// Converts canvas coordinates to local coordinates.
        /// </summary>
        /// <param name="pnt">Canvas coordinates.</param>
        /// <returns>Local coordinates.</returns>
        public virtual Point CanvasPosToLocal(Point pnt)
        {
            if (m_Parent != null)
            {
                int x = pnt.X - X;
                int y = pnt.Y - Y;

                // If our parent is a container and we're a child of it
                // add its offset onto us.
                //
                if (m_Parent is Container container && container.Children.Contains(this))
                {
                    x -= container.PanelBounds.X;
                    y -= container.PanelBounds.Y;
                }

                return m_Parent.CanvasPosToLocal(new Point(x, y));
            }

            return pnt;
        }

        /// <summary>
        /// Sets mouse cursor to current cursor.
        /// </summary>
        public virtual void UpdateCursor() => Platform.Neutral.SetCursor(Cursor);

        public virtual void Anim_WidthIn(float length, float delay = 0.0f, float ease = 1.0f)
        {
            Animation.Add(this, new Anim.Size.Width(0, Width, length, false, delay, ease));
            Width = 0;
        }

        public virtual void Anim_HeightIn(float length, float delay, float ease)
        {
            Animation.Add(this, new Anim.Size.Height(0, Height, length, false, delay, ease));
            Height = 0;
        }

        public virtual void Anim_WidthOut(float length, bool hide, float delay, float ease) => Animation.Add(this, new Anim.Size.Width(Width, 0, length, hide, delay, ease));

        public virtual void Anim_HeightOut(float length, bool hide, float delay, float ease) => Animation.Add(this, new Anim.Size.Height(Height, 0, length, hide, delay, ease));
    }
}