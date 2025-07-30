using System;
using System.Diagnostics;
using System.IO;

namespace Gwen
{
    /// <summary>
    /// Represents a texture.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Texture"/> class.
    /// </remarks>
    /// <param name="renderer">Renderer to use.</param>
    public class Texture(Renderer.RendererBase renderer) : IDisposable
    {
        /// <summary>
        /// Texture name. Usually file name, but exact meaning depends on renderer.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Renderer data.
        /// </summary>
        public object RendererData { get; set; }

        /// <summary>
        /// Indicates that the texture failed to load.
        /// </summary>
        public bool Failed { get; set; } = false;

        /// <summary>
        /// Texture width.
        /// </summary>
        public int Width { get; set; } = 4;

        /// <summary>
        /// Texture height.
        /// </summary>
        public int Height { get; set; } = 4;

        private readonly Renderer.RendererBase m_Renderer = renderer;

        /// <summary>
        /// Loads the specified texture.
        /// </summary>
        /// <param name="name">Texture name.</param>
        public void Load(string name)
        {
            Name = name;
            m_Renderer.LoadTexture(this);
        }

        /// <summary>
        /// Initializes the texture from raw pixel data.
        /// </summary>
        /// <param name="width">Texture width.</param>
        /// <param name="height">Texture height.</param>
        /// <param name="pixelData">Color array in RGBA format.</param>
        public void LoadRaw(int width, int height, byte[] pixelData)
        {
            Width = width;
            Height = height;
            m_Renderer.LoadTextureRaw(this, pixelData);
        }

        public void LoadStream(Stream data) => m_Renderer.LoadTextureStream(this, data);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            m_Renderer.FreeTexture(this);
            GC.SuppressFinalize(this);
        }

#if DEBUG

        ~Texture()
        {
            Debug.WriteLine(string.Format("IDisposable object finalized: {0}", GetType()));
        }

#endif
    }
}