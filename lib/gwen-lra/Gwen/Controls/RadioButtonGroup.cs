using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Radio button group.
    /// </summary>
    public class RadioButtonGroup : GroupBox
    {
        /// <summary>
        /// Selected radio button.
        /// </summary>
        public RadioButton Selected { get; private set; }

        /// <summary>
        /// Internal name of the selected radio button.
        /// </summary>
        public string SelectedName => Selected.Name;

        /// <summary>
        /// Text of the selected radio button.
        /// </summary>
        public string SelectedLabel => Selected.Text;

        /// <summary>
        /// Index of the selected radio button.
        /// </summary>
        public int SelectedIndex => Children.IndexOf(Selected);

        /// <summary>
        /// Invoked when the selected option has changed.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> SelectionChanged;

        protected override Margin PanelMargin => ShouldDrawBackground ? base.PanelMargin : Margin.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioButtonGroup"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="label">Label for the outlining GroupBox.</param>
        public RadioButtonGroup(ControlBase parent) : base(parent)
        {
            AutoSizeToContents = true;
            m_Panel.AutoSizeToContents = true;
            IsTabable = false;
            KeyboardInputEnabled = true;
            Text = string.Empty;
        }

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <param name="text">Option text.</param>
        /// <returns>Newly created control.</returns>
        public virtual RadioButton AddOption(string text) => AddOption(text, string.Empty);

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <param name="text">Option text.</param>
        /// <param name="optionName">Internal name.</param>
        /// <returns>Newly created control.</returns>
        public virtual RadioButton AddOption(string text, string optionName)
        {
            RadioButton lrb = new RadioButton(this)
            {
                Name = optionName,
                Text = text
            };
            lrb.Checked += OnRadioClicked;
            lrb.Dock = Dock.Top;
            lrb.Margin = new Margin(0, 0, 0, 1); // 1 bottom
            lrb.KeyboardInputEnabled = false; // TODO: true?
            lrb.IsTabable = true;

            Invalidate();
            return lrb;
        }

        /// <summary>
        /// Handler for the option change.
        /// </summary>
        /// <param name="fromPanel">Event source.</param>
        protected virtual void OnRadioClicked(ControlBase fromPanel, EventArgs args)
        {
            RadioButton chked = fromPanel as RadioButton;
            foreach (ControlBase child in Children)
            {
                if (child is RadioButton rb)
                {
                    if (rb == chked)
                    {
                        Selected = rb;
                        break;
                    }
                }
            }

            OnChanged(Selected);
        }
        protected virtual void OnChanged(ControlBase NewTarget) => SelectionChanged?.Invoke(this, new ItemSelectedEventArgs(NewTarget));

        /// <summary>
        /// Selects the specified option.
        /// </summary>
        /// <param name="index">Option to select.</param>
        public void SetSelection(int index)
        {
            if (index < 0 || index >= Children.Count)
                return;

            (Children[index] as RadioButton).Press();
        }
    }
}
