using Gwen;
using Gwen.Controls;
using linerider.UI.Components;
using linerider.Game;
using System.Collections.Generic;

namespace linerider.UI.Widgets
{
    public class LayersPanel : WidgetContainer
    {
        private readonly Editor _editor;
        private LayerContainer<Layer> _layers => _editor.GetTrack()._layers;
        private ScrollControl _scrollBox;
        private List<Panel> _layersRenderedList;

        public LayersPanel(ControlBase parent, Editor editor) : base(parent)
        {
            _editor = editor;

            _layersRenderedList = new List<Panel>();

            Setup();
        }

        private void Setup()
        {
            _ = new WidgetLabel(this)
            {
                Dock = Dock.Top,
                Alignment = Pos.Center,
                Text = "Layers",
            };

            new Separator(this);

            _scrollBox = new ScrollControl(this)
            {
                Dock = Dock.Top,
                Height = 250,
                Width = 150,
            };

            foreach(Layer layer in _layers)
            {
                AddLayer(layer);
            }

            _layers.OnAdd += (o, layer) => AddLayer(layer);
        }

        private void AddLayer(Layer layer)
        {
            Panel container = new Panel(_scrollBox)
            {
                Margin = new Margin(0, WidgetItemSpacing, 0, WidgetItemSpacing),
                ShouldDrawBackground = false,
                MouseInputEnabled = false,
                AutoSizeToContents = true,
                Dock = Dock.Top,
                UserData = layer.ID,
            };

            _layersRenderedList.Add(container);

            new WidgetLabel(container)
            {
                Dock = Dock.Left,
                Text = layer.name,
            };
        }
    }
}