using Gwen;
using Gwen.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace linerider.UI.Components
{
    public class PanelBox<TPanelItem, TListItem> : ScrollControl
    {
        private IEnumerable<TListItem> ListItems;
        private List<TPanelItem> PanelBoxItems = new List<TPanelItem>();
        private int _selectedIdx;
        public int SelectedIdx
        {
            get => _selectedIdx;
            set
            {
                _selectedIdx = value;
                Update();
            }
        }
        public bool ReverseOrder;

        public PanelBox(ControlBase parent, IEnumerable<TListItem> list) : base(parent)
        {
            ListItems = list;
        }

        public void Update()
        {
            SuspendLayout();

            // Add missing panels
            if (PanelBoxItems.Count() < ListItems.Count())
            {
                for (int i = PanelBoxItems.Count(); i < ListItems.Count(); i++)
                {
                    Panel panel = new Panel(this)
                    {
                        Margin = new Margin(0, 1, 0, 1),
                        ShouldDrawBackground = false,
                        MouseInputEnabled = true,
                        AutoSizeToContents = true,
                        Dock = Dock.Top,
                    };

                    panel.Clicked += (o, e) =>
                    {
                        int itemIdx = ReverseOrder ? ListItems.Count() - i : i - 1;
                        ((IPanelBoxItem<TListItem>)PanelBoxItems.Last()).OnSelect(ListItems.ElementAt(itemIdx), UserData);
                        SelectedIdx = i - 1;
                    };

                    TPanelItem panelItem = Activator.CreateInstance<TPanelItem>();
                    PanelBoxItems.Add(panelItem);
                    ((IPanelBoxItem<TListItem>)PanelBoxItems.Last()).Build(panel);
                }
            }

            // Remove redundant panels
            else if (PanelBoxItems.Count() > ListItems.Count())
            {
                for (int i = PanelBoxItems.Count(); i >= ListItems.Count(); i--)
                {
                    Parent.RemoveChild(((IPanelBoxItem<TListItem>)PanelBoxItems.ElementAt(i + 1)).Panel, true);
                }
            }

            // Update remaining panels
            for (int i = 0; i < PanelBoxItems.Count(); i++)
            {
                int panelIdx = ReverseOrder ? PanelBoxItems.Count() - i - 1 : i;
                TPanelItem panel = PanelBoxItems[panelIdx];

                ((IPanelBoxItem<TListItem>)PanelBoxItems[panelIdx]).Populate(ListItems.ElementAt(i), UserData, Update);

                Children[i].ShouldDrawBackground = SelectedIdx == i; ;
            }

            ResumeLayout(true);
        }
    }
}
