namespace Gwen.Controls
{
    public class TabPage(ControlBase parent, TabButton button) : ControlBase(parent)
    {
        public TabButton TabButton { get; } = button;
        public string Text
        {
            get => TabButton.Text;
            set => TabButton.Text = value;
        }

        public void FocusTab() => TabButton.Press();
        public void RemoveTab() => TabButton.TabControl.RemoveTab(this);
        public void SetIndex(int index) => TabButton.TabControl.SetTabIndex(this, index);
    }
}
