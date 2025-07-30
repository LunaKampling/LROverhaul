using System.Collections.Generic;
using System.Linq;

namespace Gwen.Controls
{
    /// <summary>
    /// ListBox control.
    /// </summary>
    public class ListBox : ScrollControl
    {
        private bool m_MultiSelect;
        private bool m_AlternateColors;

        /// <summary>
        /// Determines whether multiple rows can be selected at once.
        /// </summary>
        public bool AllowMultiSelect
        {
            get => m_MultiSelect;
            set
            {
                m_MultiSelect = value;
                if (value)
                    IsToggle = true;
            }
        }
        public bool AlternateColors
        {

            get => m_AlternateColors;
            set
            {
                if (m_AlternateColors != value)
                {
                    m_AlternateColors = value;
                    if (!m_AlternateColors)
                    {
                        foreach (ControlBase child in Children)
                        {
                            if (child is ListBoxRow row)
                            {
                                row.EvenRow = false;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Determines whether rows can be unselected by clicking on them again.
        /// </summary>
        public bool IsToggle { get; set; }

        /// <summary>
        /// Returns specific row of the ListBox.
        /// </summary>
        /// <param name="index">Row index.</param>
        /// <returns>Row at the specified index.</returns>
        public ListBoxRow this[int index] => Children[index] as ListBoxRow;

        /// <summary>
        /// List of selected rows.
        /// </summary>
        public List<ListBoxRow> SelectedRows { get; }

        /// <summary>
        /// First selected row (and only if list is not multiselectable).
        /// </summary>
        public ListBoxRow SelectedRow
        {
            get => SelectedRows.Count == 0 ? null : SelectedRows[0];
            set
            {
                if (Children.Contains(value))
                {
                    if (AllowMultiSelect)
                    {
                        SelectRow(value, false);
                    }
                    else
                    {
                        SelectRow(value, true);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the selected row number.
        /// </summary>
        public int SelectedRowIndex
        {
            get
            {
                ListBoxRow selected = SelectedRow;
                return selected == null ? -1 : Children.IndexOf(selected);
            }
            set => SelectRow(value);
        }

        /// <summary>
        /// Invoked when a row has been selected.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> RowSelected;

        /// <summary>
        /// Invoked whan a row has beed unselected.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> RowUnselected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ListBox(ControlBase parent)
            : base(parent)
        {
            SelectedRows = [];

            MouseInputEnabled = true;
            // EnableScroll(false, true);
            AutoHideBars = true;
            Margin = Margin.One;

            m_MultiSelect = false;
            IsToggle = false;
            OnThink += (o, e) =>
            {
                if (AlternateColors)
                {
                    bool even = false;

                    foreach (ControlBase child in Children)
                    {
                        if (child is ListBoxRow row)
                        {
                            row.EvenRow = even;
                            even = !even;
                        }
                    }
                }
            };
        }
        /// <summary>
        /// Selects the specified row by index.
        /// </summary>
        /// <param name="index">Row to select.</param>
        /// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
        public void SelectRow(int index, bool clearOthers = false)
        {
            if (index < 0 || index >= Children.Count)
                return;

            SelectRow(Children[index], clearOthers);
        }

        /// <summary>
        /// Selects the specified row(s) by text.
        /// </summary>
        /// <param name="rowText">Text to search for (exact match).</param>
        /// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
        public void SelectRows(string rowText, bool clearOthers = false)
        {
            IEnumerable<ListBoxRow> rows = Children.OfType<ListBoxRow>().Where(x => x.Text == rowText);
            foreach (ListBoxRow row in rows)
            {
                SelectRow(row, clearOthers);
            }
        }

        /// <summary>
        /// Slelects the specified row.
        /// </summary>
        /// <param name="control">Row to select.</param>
        /// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
        public void SelectRow(ControlBase control, bool clearOthers = false)
        {
            if (!AllowMultiSelect || clearOthers)
                UnselectAll();

            if (!(control is ListBoxRow row))
                return;

            // TODO: make sure this is one of our rows!
            row.IsSelected = true;
            SelectedRows.Add(row);
            RowSelected?.Invoke(this, new ItemSelectedEventArgs(row));
        }

        /// <summary>
        /// Removes the all rows from the ListBox
        /// </summary>
        /// <param name="idx">Row index.</param>
        public void RemoveAllRows() => DeleteAll();

        /// <summary>
        /// Removes the specified row by index.
        /// </summary>
        /// <param name="idx">Row index.</param>
        public void RemoveRow(int idx)
        {
            RemoveChild(Children[idx], true);
            ; // this calls Dispose()
        }

        /// <summary>
        /// Adds a new row.
        /// </summary>
        /// <param name="label">Row text.</param>
        /// <returns>Newly created control.</returns>
        public ListBoxRow AddRow(string label) => AddRow(label, string.Empty);

        /// <summary>
        /// Adds a new row.
        /// </summary>
        /// <param name="label">Row text.</param>
        /// <param name="name">Internal control name.</param>
        /// <returns>Newly created control.</returns>
        public ListBoxRow AddRow(string label, string name) => AddRow(label, name, null);

        /// <summary>
        /// Adds a new row.
        /// </summary>
        /// <param name="label">Row text.</param>
        /// <param name="name">Internal control name.</param>
        /// <param name="UserData">User data for newly created row</param>
        /// <returns>Newly created control.</returns>
        public ListBoxRow AddRow(string label, string name, object UserData)
        {
            ListBoxRow row = new(this)
            {
                Dock = Dock.Top,
                Text = label,
                Name = name,
                UserData = UserData
            };

            row.Selected += OnRowSelected;
            return row;
        }
        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawListBox(this);

        /// <summary>
        /// Deselects all rows.
        /// </summary>
        public virtual void UnselectAll()
        {
            foreach (ListBoxRow row in SelectedRows)
            {
                row.IsSelected = false;
                RowUnselected?.Invoke(this, new ItemSelectedEventArgs(row));
            }
            SelectedRows.Clear();
        }

        /// <summary>
        /// Unselects the specified row.
        /// </summary>
        /// <param name="row">Row to unselect.</param>
        public void UnselectRow(ListBoxRow row)
        {
            row.IsSelected = false;
            _ = SelectedRows.Remove(row);

            RowUnselected?.Invoke(this, new ItemSelectedEventArgs(row));
        }
        protected override void OnChildRemoved(ControlBase child)
        {
            base.OnChildRemoved(child);
            if (child is ListBoxRow row)
            {
                row.Selected -= OnRowSelected;
            }
        }
        /// <summary>
        /// Handler for the row selection event.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void OnRowSelected(ControlBase control, ItemSelectedEventArgs args)
        {
            // [omeg] changed default behavior
            bool clear = false;// !InputHandler.InputHandler.IsShiftDown;
            if (!(args.SelectedItem is ListBoxRow row))
                return;

            if (row.IsSelected)
            {
                if (IsToggle)
                    UnselectRow(row);
            }
            else
            {
                SelectRow(row, clear);
            }
        }
        /// <summary>
        /// Selects the first menu item with the given text it finds. 
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="label">The label to look for, this is what is shown to the user.</param>
        public void SelectByText(string text)
        {
            foreach (ControlBase c in Children)
            {
                if (c is ListBoxRow item)
                {
                    if (item.Text == text)
                    {
                        SelectedRow = item;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Selects the first menu item with the given internal name it finds.
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="name">The internal name to look for. To select by what is displayed to the user, use "SelectByText".</param>
        public void SelectByName(string name)
        {
            foreach (ControlBase c in Children)
            {
                if (c is ListBoxRow item)
                {
                    if (item.Name == name)
                    {
                        SelectedRow = item;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Selects the first menu item with the given user data it finds.
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="userdata">The UserData to look for. The equivalency check uses "param.Equals(item.UserData)".
        /// If null is passed in, it will look for null/unset UserData.</param>
        public void SelectByUserData(object userdata)
        {
            foreach (ControlBase c in Children)
            {
                if (c is ListBoxRow item)
                {
                    if (userdata == null)
                    {
                        if (item.UserData == null)
                        {
                            SelectedRow = item;
                            return;
                        }
                    }
                    else if (userdata.Equals(item.UserData))
                    {
                        SelectedRow = item;
                        return;
                    }
                }
            }
        }
    }
}
