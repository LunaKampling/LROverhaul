using Gwen;
using Gwen.Controls;
using linerider.Game;
using System;
using System.Drawing;

namespace linerider.UI
{
    public class LayerPropertiesWindow : DialogBase
    {
        private GwenEventHandler<EventArgs> _applyChanges;
        private string _name;
        private Color _color;
        private Layer _layer;
        public LayerPropertiesWindow(GameCanvas parent, Editor editor, Layer layer, GwenEventHandler<EventArgs> applyChanges) : base(parent, editor)
        {
            _layer = layer;
            _name = layer.name;
            _color = layer.GetColor();
            _applyChanges = applyChanges;

            Title = $"Layer Properties";
            _ = SetSize(175, 125);
            DisableResizing();

            MakeModal(true);

            Setup();
        }

        private void Setup()
        {
            TextBox name = new TextBox(null)
            {
                Text = _name,
                Width = 92,
            };
            name.TextChanged += (o, e) => _name = name.Text;
            GwenHelper.CreateLabeledControl(this, "Name", name);

            _ = GwenHelper.CreateLabeledColorInput(this, "Color", _color, (o, color) =>
            {
                _color = color.Value;
            });

            Panel bottomArea = new Panel(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                ShouldDrawBackground = false,
            };

            Button save = new Button(bottomArea)
            {
                Dock = Dock.Right,
                Text = "Save",
                Padding = new Padding(10, 0, 10, 0)
            };
            save.Clicked += (o, e) =>
            {
                _layer.name = _name;
                _layer.SetColor(_color);
                _layer.Rerender(_editor);

                _applyChanges(this, EventArgs.Empty);

                Close();
            };

            name.Focus();
        }
    }
}
