﻿using Gwen;
using Gwen.Controls;
using linerider.UI.Components;
using linerider.Game;
using static linerider.UI.GwenHelper;
using System;

namespace linerider.UI.Widgets
{
    public class LayersPanel : WidgetContainer
    {
        private readonly Editor _editor;
        private LayerContainer<Layer> _layers => _editor.GetTrack()._layers;
        private PanelBox<LayerItem, Layer> _layersBox;

        public LayersPanel(ControlBase parent, Editor editor) : base(parent)
        {
            _editor = editor;

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

            _layersBox = new PanelBox<LayerItem, Layer>(this, _layers)
            {
                Dock = Dock.Top,
                Height = Utility.NumberToCurrentScale(250),
                Width = Utility.NumberToCurrentScale(150),
                ReverseOrder = true,
                UserData = _editor,
            };

            _layers.OnAdd += (o, layer) => _layersBox.Update();
            _layers.OnRemove += (o, layer) => _layersBox.Update();
            _layersBox.Update();

            new Separator(this);

            Panel buttonsBar = new Panel(this)
            {
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                Dock = Dock.Top,
                Margin = new Margin(WidgetPadding, 0, WidgetPadding, 0),
            };

            _ = new WidgetButton(buttonsBar)
            {
                Name = "Add new layer",
                Icon = GameResources.icon_layer_bar_add.Bitmap,
                Dock = Dock.Left,
                Action = (o, e) =>
                {
                    // TODO
                },
            };

            _ = new WidgetButton(buttonsBar)
            {
                Name = "Delete selected layer",
                Icon = GameResources.icon_layer_bar_delete.Bitmap,
                Dock = Dock.Right,
                Action = (o, e) =>
                {
                    // TODO
                },
            };

            _ = new WidgetButton(buttonsBar)
            {
                Name = "Selected layer properties",
                Icon = GameResources.icon_layer_bar_edit.Bitmap,
                Dock = Dock.Right,
                Action = (o, e) =>
                {
                    // TODO
                },
            };
        }
    }
    internal class LayerItem : IPanelBoxItem<Layer>
    {
        public Panel Panel { get; set; }

        private ColorIndicator Color;
        private WidgetButton VisibilityButton;
        private WidgetButton LockButton;
        private WidgetLabel LayerName;

        public void Build(Panel panel)
        {
            Color = new ColorIndicator()
            {
                Margin = new Margin(2, 0, 0, 0),
                Width = Utility.NumberToCurrentScale(5),
                Dock = Dock.Left,
            };
            panel.Children.Add(Color);

            VisibilityButton = new WidgetButton(panel)
            {
                Dock = Dock.Left,
                Icon = GameResources.icon_layer_inline_visible.Bitmap,
            };

            LayerName = new WidgetLabel(panel)
            {
                Dock = Dock.Left,
                Alignment = Pos.CenterV
            };

            LockButton = new WidgetButton(panel)
            {
                Dock = Dock.Right,
                Icon = GameResources.icon_layer_inline_unlocked.Bitmap,
            };
        }

        public void Populate(Layer layer, object data, Action refresh)
        {
            Editor editor = (Editor)data;

            Color.Color = layer.GetColor();
            LayerName.Text = layer.name;

            VisibilityButton.Icon = layer.GetVisibility()
                ? GameResources.icon_layer_inline_visible.Bitmap
                : GameResources.icon_layer_inline_hidden.Bitmap;

            LockButton.Icon = layer.GetLock()
                ? GameResources.icon_layer_inline_locked.Bitmap
                : GameResources.icon_layer_inline_unlocked.Bitmap;

            VisibilityButton.Action = (o, e) =>
            {
                // TODO
            };

            LockButton.Action = (o, e) =>
            {
                // TODO
            };
        }
    }
}