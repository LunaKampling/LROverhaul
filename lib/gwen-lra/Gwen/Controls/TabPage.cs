using System;
using Gwen.Input;

namespace Gwen.Controls
{
    public class TabPage : ControlBase
    {
        private TabButton m_ParentButton;
        public TabButton TabButton
        {
            get
            {
                return m_ParentButton;
            }
        }
        public string Text
        {
            get
            {
                return m_ParentButton.Text;
            }
            set
            {
                m_ParentButton.Text = value;
            }
        }
        public TabPage(ControlBase parent, TabButton button) : base(parent)
        {
            m_ParentButton = button;
        }
        public void FocusTab()
        {
            TabButton.Press();
        }
        public void RemoveTab()
        {
            TabButton.TabControl.RemoveTab(this);
        }
        public void SetIndex(int index)
        {
            TabButton.TabControl.SetTabIndex(this,index);
        }
    }
}
