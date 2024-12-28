using Gwen;
using Gwen.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace linerider.UI.Components
{
    // This class is such a mess. I've created a monster ;_;
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
                int itemIdx = ReverseOrder ? ListItems.Count() - value - 1 : value;
                ((IPanelBoxItem<TListItem>)PanelBoxItems.ElementAt(itemIdx)).OnSelect(ListItems.ElementAt(itemIdx), UserData);

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

                    PanelBoxItems.Add(Activator.CreateInstance<TPanelItem>());
                    ((IPanelBoxItem<TListItem>)PanelBoxItems.Last()).Panel = panel;
                    ((IPanelBoxItem<TListItem>)PanelBoxItems.Last()).Build(panel);
                }
            }

            // Remove redundant panels
            else if (PanelBoxItems.Count() > ListItems.Count())
            {
                int panelsCountToRemove = PanelBoxItems.Count() - ListItems.Count();
                for (int i = 0; i < panelsCountToRemove; i++)
                {
                    int lastItemIdx = PanelBoxItems.Count() - 1;
                    Parent.RemoveChild(((IPanelBoxItem<TListItem>)PanelBoxItems.ElementAt(lastItemIdx)).Panel, true);
                    //((IPanelBoxItem<TListItem>)PanelBoxItems.ElementAt(lastItemIdx)).OnDispose(ListItems.ElementAt(lastItemIdx), UserData);
                    PanelBoxItems.RemoveAt(lastItemIdx);
                }
            }

            // Update remaining panels
            for (int i = 0; i < PanelBoxItems.Count(); i++)
            {
                int panelIdx = ReverseOrder ? PanelBoxItems.Count() - i - 1 : i;
                Children[i].ShouldDrawBackground = SelectedIdx == i;

                ((IPanelBoxItem<TListItem>)PanelBoxItems[panelIdx]).Populate(ListItems.ElementAt(i), UserData, Update);
            }

            ResumeLayout(true);
        }
    }
}
