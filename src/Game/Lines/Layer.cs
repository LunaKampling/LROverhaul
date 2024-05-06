using System.Drawing;
using OpenTK;
using System;
using System.Collections.Generic;
using linerider.Game;
using linerider.Rendering;
using linerider.Utils;

namespace linerider.Game
{
    public class Layer
    {
        public int? parentID = null; //folder id
        public int? ID = null; //layer id
        public bool Invisible = false; //whether lines in the layer should be rendered
        public Dictionary<int, GameLine> lines = new Dictionary<int, GameLine>(); //lines objects contained in layer
        public string name = "Base Layer";
        private static string colorhex = "ffffaa";
        private static int hex = int.Parse("ff" + colorhex, System.Globalization.NumberStyles.HexNumber);
        private Color _color = Color.FromArgb(hex);

        public Layer()
        {
        }
        /// <summary>
        /// Rerenders every line in the layer
        /// </summary>
        /// <param name="Track">Editor object (contains track)</param>
        public void Rerender(Editor Track)
        {
            foreach(GameLine line in lines.Values)
            {
                Track.RedrawLine(line);
            }
        }
        public Color GetColor()
        {
            return _color;
        }
        public void SetColor(Color color)
        {
            _color = color;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer">Layer to clone from</param>
        public void CloneLayer(Layer layer)
        {
            _color = layer.GetColor();
        }
    }
}
