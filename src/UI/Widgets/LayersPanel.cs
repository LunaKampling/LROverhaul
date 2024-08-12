using Gwen;
using Gwen.Controls;
using linerider.UI.Components;
using linerider.Game;
using System.Linq;

namespace linerider.UI.Widgets
{
    public class LayersPanel : WidgetContainer
    {
        private readonly Editor _editor;
        private LayerContainer<Layer> _layers => _editor.GetTrack()._layers;

        public LayersPanel(ControlBase parent, Editor editor) : base(parent)
        {
            _editor = editor;

            Setup();
        }

        private void Setup()
        {

            _ = new TrackLabel(this)
            {
                Dock = Dock.Top,
                TextRequest = (o, e) => $"Count: {_layers.Count}",
            };

            _ = new TrackLabel(this)
            {
                Dock = Dock.Top,
                TextRequest = (o, e) =>
                {
                    var strings = from i in _layers
                                  select i.name;

                    return $"Layers:\n{string.Join("\n", strings)}";
                }
            };
        }
    }
}