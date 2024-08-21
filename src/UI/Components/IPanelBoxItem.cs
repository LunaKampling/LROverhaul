using Gwen.Controls;
using System;
namespace linerider.UI.Components
{
    public interface IPanelBoxItem<T>
    {
        Panel Panel { get; set; }
        void Build(Panel panel);
        void Populate(T item, object data, Action refresh);
    }
}
