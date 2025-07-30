using Gwen.Input;

namespace Gwen.Controls
{
    public partial class ControlBase
    {
        public Rectangle RenderBounds => new(0, 0, Width, Height);
        /// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected virtual void Render(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Renders the control to a cache using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        /// <param name="master">Root parent.</param>
        protected virtual void DoCacheRender(Skin.SkinBase skin, ControlBase master)
        {
            Renderer.RendererBase render = skin.Renderer;
            Renderer.ICacheToTexture cache = render.CTT;

            if (cache == null)
                return;

            Point oldRenderOffset = render.RenderOffset;
            Rectangle oldRegion = render.ClipRegion;

            if (this != master)
            {
                render.AddRenderOffset(Bounds);
                render.AddClipRegion(Bounds);
            }
            else
            {
                render.RenderOffset = Point.Empty;
                render.ClipRegion = new Rectangle(0, 0, Width, Height);
            }

            if (m_CacheTextureDirty && render.ClipRegionVisible)
            {
                render.StartClip();

                if (ShouldCacheToTexture)
                    cache.SetupCacheTexture(this);

                // Render myself first
                //var old = render.ClipRegion;
                //render.ClipRegion = Bounds;
                //var old = render.RenderOffset;
                //render.RenderOffset = new Point(Bounds.X, Bounds.Y);
                Render(skin);
                //render.RenderOffset = old;
                //render.ClipRegion = old;

                if (m_Children.Count > 0)
                {
                    // Now render my kids
                    foreach (ControlBase child in m_Children)
                    {
                        if (child.IsHidden)
                            continue;
                        child.DoCacheRender(skin, master);
                    }
                }

                if (ShouldCacheToTexture)
                {
                    cache.FinishCacheTexture(this);
                    m_CacheTextureDirty = false;
                }
            }

            render.ClipRegion = oldRegion;
            render.StartClip();
            render.RenderOffset = oldRenderOffset;

            if (ShouldCacheToTexture)
                cache.DrawCachedControlTexture(this);
        }

        /// <summary>
        /// Rendering logic implementation.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        internal virtual void DoRender(Skin.SkinBase skin)
        {
            // If this control has a different skin,
            // then so does its children.
            if (m_Skin != null)
                skin = m_Skin;

            Renderer.RendererBase render = skin.Renderer;

            if (render.CTT != null && ShouldCacheToTexture)
            {
                DoCacheRender(skin, this);
                return;
            }

            RenderRecursive(skin, Bounds);

            if (DrawDebugOutlines)
                skin.DrawDebugOutlines(this);
        }

        /// <summary>
        /// Recursive rendering logic.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        /// <param name="clipRect">Clipping rectangle.</param>
        protected virtual void RenderRecursive(Skin.SkinBase skin, Rectangle clipRect)
        {
            Renderer.RendererBase render = skin.Renderer;
            Point oldRenderOffset = render.RenderOffset;

            render.AddRenderOffset(clipRect);

            RenderUnder(skin);

            Rectangle oldRegion = render.ClipRegion;

            if (ShouldClip)
            {
                render.AddClipRegion(RenderBounds);

                if (!render.ClipRegionVisible)
                {
                    render.RenderOffset = oldRenderOffset;
                    render.ClipRegion = oldRegion;
                    return;
                }

                render.StartClip();
            }

            // Render myself first
            Render(skin);

            if (m_Children.Count > 0)
            {
                // Now render my kids
                foreach (ControlBase child in m_Children)
                {
                    if (child.IsHidden)
                        continue;
                    child.DoRender(skin);
                }
            }

            render.ClipRegion = oldRegion;
            render.StartClip();
            RenderOver(skin);

            RenderFocus(skin);

            render.RenderOffset = oldRenderOffset;
        }
        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void RenderFocus(Skin.SkinBase skin)
        {
            if (InputHandler.KeyboardFocus != this)
                return;
            if (!IsTabable)
                return;

            skin.DrawKeyboardHighlight(this, RenderBounds, 3);
        }

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void RenderUnder(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Renders over the actual control (overlays).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void RenderOver(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Sets the control's skin.
        /// </summary>
        /// <param name="skin">New skin.</param>
        /// <param name="doChildren">Deterines whether to change children skin.</param>
        public virtual void SetSkin(Skin.SkinBase skin, bool doChildren = false)
        {
            if (m_Skin == skin)
                return;
            m_Skin = skin;
            Invalidate();
            Redraw();
            OnSkinChanged(skin);

            if (doChildren)
            {
                foreach (ControlBase child in m_Children)
                {
                    child.SetSkin(skin, true);
                }
            }
        }

        /// <summary>
        /// Invalidates the control.
        /// </summary>
        /// <remarks>
        /// Causes layout, repaint, invalidates cached texture.
        /// </remarks>
        public virtual void Invalidate()
        {
            NeedsLayout = true;
            m_CacheTextureDirty = true;
            Canvas canvas = GetCanvas();
            if (canvas != null)
            {
                canvas.NeedsRedraw = true;
            }
        }
        /// <summary>
        /// Invalidates the control's children (relayout/repaint).
        /// </summary>
        /// <param name="recursive">Determines whether the operation should be carried recursively.</param>
        protected virtual void InvalidateChildren(bool recursive = false)
        {
            foreach (ControlBase child in Children)
            {
                child.Invalidate();
                if (recursive)
                    child.InvalidateChildren(true);
            }

            // If we're a container:
            if (m_Children != Children)
            {
                foreach (ControlBase child in m_Children)
                {
                    child.Invalidate();
                    if (recursive)
                        child.InvalidateChildren(true);
                }
            }
        }
        /// <summary>
        /// Invalidates control's parent.
        /// </summary>
        public void InvalidateParent() => m_Parent?.Invalidate();
        /// <summary>
        /// Re-renders the control, invalidates cached texture.
        /// </summary>
        public virtual void Redraw()
        {
            m_CacheTextureDirty = true;
            Canvas canvas = GetCanvas();
            if (canvas != null)
            {
                canvas.NeedsRedraw = true;
            }
        }

        /// <summary>
        /// Handler invoked when control's skin changes.
        /// </summary>
        /// <param name="newSkin">New skin.</param>
        protected virtual void OnSkinChanged(Skin.SkinBase newSkin)
        {
        }
    }
}