using Gwen;
using Gwen.Controls;

namespace linerider.UI
{
    public class GameMenuWindow : DialogBase
    {
        protected override Margin PanelMargin => Margin.Ten;
        public GameMenuWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Game Menu";
            MinimumSize = new Size(200, MinimumSize.Height);
            AutoSizeToContents = true;

            DisableResizing();
            MakeModal(true);
            Setup();
        }
        private void Setup()
        {
            Margin btnmargin = new Margin(10, 5, 10, 5);
            Margin btnseparatormargin = new Margin(10, 5, 10, 20);

            Button save = new Button(this)
            {
                Text = "Save Track",
                Dock = Dock.Top,
                Margin = btnmargin
            };
            Button load = new Button(this)
            {
                Text = "Load Track",
                Dock = Dock.Top,
                Margin = btnseparatormargin
            };
            Button trackprops = new Button(this)
            {
                Text = "Track Properties",
                Dock = Dock.Top,
                Margin = btnmargin
            };
            Button triggers = new Button(this)
            {
                Text = "Triggers",
                Dock = Dock.Top,
                Margin = btnseparatormargin
            };
            Button preferences = new Button(this)
            {
                Text = "Preferences",
                Dock = Dock.Top,
                Margin = btnmargin
            };

            save.Clicked += (o, e) =>
            {
                _canvas.ShowSaveDialog();
                _ = Close();
            };
            load.Clicked += (o, e) =>
            {
                _canvas.ShowLoadDialog();
                _ = Close();
            };
            trackprops.Clicked += (o, e) =>
            {
                _canvas.ShowTrackPropertiesDialog();
                _ = Close();
            };
            triggers.Clicked += (o, e) =>
            {
                _canvas.ShowTriggerWindow();
                _ = Close();
            };
            preferences.Clicked += (o, e) =>
            {
                _canvas.ShowPreferencesDialog();
                _ = Close();
            };
        }
    }
}
