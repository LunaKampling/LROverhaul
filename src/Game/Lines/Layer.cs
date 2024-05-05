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
        public int? parentID = null;
        public int? ID = null;
        public LinkedList<GameLine> lines = new LinkedList<GameLine>();
        public string name = "Base Layer";
        private static string colorhex = "ffffaa";
        private static int hex = int.Parse("ff" + colorhex, System.Globalization.NumberStyles.HexNumber);
        private Color _color = Color.FromArgb(hex);

        public Layer()
        {
        }

        public void Rerender(Editor Track)
        {
            LinkedListNode<GameLine> node = lines.First;
            while (node != null)
            {
                Track.RedrawLine(node.Value);
                node = node.Next;
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

        public void CloneLayer(Layer layer)
        {
            _color = layer.GetColor();
        }
    }
}
