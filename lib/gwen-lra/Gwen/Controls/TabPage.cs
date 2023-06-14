namespace Gwen.Controls
{
    public class TabPage : ControlBase
    {
        public TabButton TabButton { get; }
        public string Text
        {
            get => TabButton.Text;
            set => TabButton.Text = value;
        }
        public TabPage(ControlBase parent, TabButton button) : base(parent)
        {
            TabButton = button;
        }
        public void FocusTab() => TabButton.Press();
        public void RemoveTab() => TabButton.TabControl.RemoveTab(this);
        public void SetIndex(int index) => TabButton.TabControl.SetTabIndex(this, index);
    }
}
