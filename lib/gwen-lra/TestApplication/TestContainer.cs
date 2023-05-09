using System;
using System.Diagnostics;
using Gwen;
using Gwen.Controls;
using TestApplication.Tests;
namespace TestApplication
{
    public class TestContainer : ControlBase
    {
        //todo
        // scroll control needs to be not set by percent by controls (tree control scrolls slow af)
        // tree control multi select should only be if holding ctrl, right?
        // window has no font scaling for title
        // on ishidden change, fire mouseup
        // progressbar
        // category recolor
        // dpi
        // cursors
        // horz scroll respond when hovered to wheel
        // scrollbars covering borders
        // we have to let Layout(force) work on hidden controls, (window controls start hidden) but its not right (example a collapsed treenode)
        private ControlBase _focus = null;
        public TestContainer(ControlBase parent) : base(parent)
        {
            Dock = Dock.Fill;
            Create();
        }
        public void DbgCreate()
        {
            string random = "abcdefghijklmnop";
            var panel = new Panel(this)
            {
                Height = 100,
                Dock = Dock.Top,
            };
            var label = new Label(panel)
            {
                Dock = Dock.Left,
                Text = "abcd",
            };
            Layout(true);
            Stopwatch sw = Stopwatch.StartNew();


            label.TextRequest = (o, current) =>
            {
                if (sw.ElapsedMilliseconds > 3000)
                    return random;
                return random.Substring(0, new Random().Next(0, 10));
            };
            // var test = new TreeTest(this);
        }
        public void CreateStatusbar()
        {
            StatusBar sb = new StatusBar(this);
            Label left = new Label(sb);
            left.Text = "Statusbar left";
            sb.AddControl(left, false);

            Button br = new Button(sb);
            br.Text = "Right button";
            sb.AddControl(br, true);
        }
        private void CategorySelected(object sender, ItemSelectedEventArgs e)
        {
            if (_focus != e.SelectedItem.UserData)
            {
                if (_focus != null)
                {
                    _focus.Hide();
                }
                _focus = (ControlBase)e.SelectedItem.UserData;
                _focus.Show();
            }
        }
        private ControlBase AddPage(CollapsibleCategory category, string name)
        {
            var btn = category.Add(name);
            Panel panel = new Panel(this);
            panel.Dock = Dock.Fill;
            panel.Hide();
            btn.UserData = panel;
            category.Selected += CategorySelected;
            return panel;
        }
        public void Create()
        {
            CreateStatusbar();

            CollapsibleList list = new CollapsibleList(this);
            list.Margin = new Margin(0, 0, 1, 0);
            list.Dock = Dock.Left;
            list.AutoSizeToContents = true;
            var cat = list.Add("Basic");

            var page = AddPage(cat, "Labels");
            var label = new LabelTest(page);

            page = AddPage(cat, "Buttons");
            var btn = new ButtonTest(page);
            page = AddPage(cat, "ProgressBar");
            var prog = new ProgressBarTest(page);

            page = AddPage(cat, "Layout");
            var layout = new LayoutTest(page);
            page = AddPage(cat, "Textbox");
            var textbox = new TextBoxTest(page);
            page = AddPage(cat, "Slider");
            var slider = new SliderTest(page);
            cat = list.Add("Containers");

            page = AddPage(cat, "Generic Container");
            var container = new ContainerTest(page);
            page = AddPage(cat, "Layout Container");
            var layoutcontainer = new LayoutContainerTest(page);
            page = AddPage(cat, "Notification");
            var notify = new NotificationTest(page);
            _focus = page;
            _focus.Show();
            page = AddPage(cat, "Window");
            var window = new WindowTest(page);

            page = AddPage(cat, "Tab Control");
            var tab = new TabTest(page);

            cat = list.Add("Composite controls");
            page = AddPage(cat, "ListBox");
            var listbox = new ListBoxTest(page);

            page = AddPage(cat, "TreeControl");
            var tree = new TreeTest(page);

            page = AddPage(cat, "Menu");
            var menu = new MenuTest(page);
            page = AddPage(cat, "Property");
            var prop = new PropertyTest(page);
            page = AddPage(cat, "Category");
            var collapse = new CategoryTest(page);
            //    page.FocusTab();
        }
    }
}
