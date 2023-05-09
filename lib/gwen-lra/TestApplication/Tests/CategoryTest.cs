using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class CategoryTest : ControlTest
    {
        public CategoryTest(ControlBase parent) : base(parent)
        {
            FlowLayout container = new FlowLayout(parent);
            container.Dock = Dock.Fill;
            CollapsibleList list = new CollapsibleList(container);
            list.Width = 200;
            list.Height = 200;
            var l = list.Add("Hell0 i am cat");
            l.Add("i have cats");
            l.Add("meaow");
            l = list.Add("Hi. I'm a kitten");
            l.Add("I am proffesional.");
            l.Add("Mow.");
        }
    }
}
