using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gwen
{
    /// <summary>
    /// Represents font resource.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Font"/> class.
    /// </remarks>
    /// <param name="renderer">Renderer to use.</param>
    /// <param name="faceName">Face name.</param>
    /// <param name="size">Font size.</param>
    public class Font(Renderer.RendererBase renderer, string faceName, int size = 10) : IDisposable
    {
        /// <summary>
        /// Font face name. Exact meaning depends on renderer.
        /// </summary>
        public string FaceName { get; set; } = faceName;

        /// <summary>
        /// Font size.
        /// </summary>
        public int Size { get; set; } = size;

        /// <summary>
        /// Enables or disables font smoothing (default: disabled).
        /// </summary>
        public bool Smooth { get; set; } = false;

        //public bool Bold { get; set; }
        //public bool DropShadow { get; set; }

        /// <summary>
        /// This should be set by the renderer if it tries to use a font where it's null.
        /// </summary>
        public object RendererData { get; set; }
        public virtual int LineHeight => Size;

        /// <summary>
        /// This is the real font size, after it's been scaled by Renderer.Scale()
        /// </summary>
        public float RealSize { get; set; }

        private readonly Renderer.RendererBase m_Renderer = renderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Font"/> class.
        /// </summary>
        public Font(Renderer.RendererBase renderer)
            : this(renderer, "Arial", 10)
        {

        }

        public virtual List<string> WordWrap(string input, int maxpx) => [input];

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            m_Renderer.FreeFont(this);
            GC.SuppressFinalize(this);
        }

#if DEBUG
        ~Font()
        {
            Debug.WriteLine(string.Format("IDisposable object finalized: {0}", GetType()));
        }
#endif

        /// <summary>
        /// Duplicates font data (except renderer data which must be reinitialized).
        /// </summary>
        /// <returns></returns>
        public Font Copy()
        {
            Font f = new(m_Renderer, FaceName)
            {
                Size = Size,
                RealSize = RealSize,
                RendererData = null // Must be reinitialized
            };
            //f.Bold = Bold;
            //f.DropShadow = DropShadow;

            return f;
        }
    }
}
