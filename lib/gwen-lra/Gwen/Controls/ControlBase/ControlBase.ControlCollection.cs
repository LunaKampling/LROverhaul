using System;
using System.Collections;
using System.Collections.Generic;

namespace Gwen.Controls
{
    public partial class ControlBase
    {
        public class ControlCollection : IList<ControlBase>
        {
            private readonly ControlBase _owner;
            private readonly List<ControlBase> _controls;
            public ControlBase this[int index]
            {
                get => _controls[index];
                set => throw new Exception("Cannot use setter for ControlBase.Children, use AddChild instead.");
            }

            public int Count => _controls.Count;

            public bool IsReadOnly => false;

            public ControlCollection(ControlBase parent)
            {
                _owner = parent;
                _controls = new List<ControlBase>();
            }

            public void SendToBack(ControlBase child)
            {
                int index = IndexOf(child);
                SendToBack(index);
            }
            internal void SendToBack(int index)
            {
                if (index == -1)
                    throw new Exception("Child not found in parent who called SendToBack");
                if (Count != 0 && index != 0)
                {
                    ControlBase control = _controls[index];
                    _controls.RemoveAt(index);
                    _controls.Insert(0, control);
                    _owner.Invalidate();
                }
            }
            internal void BringToFront(int index)
            {
                if (index == -1)
                    throw new Exception("Child not found in parent who called BringToFront");
                if (Count != 0 && index != Count - 1)
                {
                    ControlBase control = _controls[index];
                    _controls.RemoveAt(index);
                    _controls.Insert(_controls.Count, control);
                    _owner.Invalidate();
                }
            }
            internal void Move(ControlBase child, int index)
            {
                int childindex = IndexOf(child);
                if (childindex == -1)
                    throw new Exception("Child not found in parent who called Move");
                if (index >= Count || index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _controls.RemoveAt(childindex);
                _controls.Insert(index, child);
                _owner.Invalidate();
            }
            public void BringToFront(ControlBase child)
            {
                int index = IndexOf(child);
                BringToFront(index);
            }

            public void Add(ControlBase item) => Insert(Count, item);

            public void Clear()
            {
                while (Count > 0)
                    RemoveAt(Count - 1);
            }

            public bool Contains(ControlBase item) => _controls.Contains(item);

            public void CopyTo(ControlBase[] array, int arrayIndex) => _controls.CopyTo(array, arrayIndex);

            public IEnumerator<ControlBase> GetEnumerator() => _controls.GetEnumerator();

            public int IndexOf(ControlBase item) => _controls.IndexOf(item);

            public void Insert(int index, ControlBase item)
            {
                if (!Contains(item))
                {
                    if (item.m_Parent != null && item.m_Parent != _owner)
                    {
                        // Remove previous parent
                        item.m_Parent.RemoveChild(item, false);
                    }
                    _controls.Insert(index, item);
                    item.m_Parent = _owner;
                    _owner.OnChildAdded(item);
                    _owner.Invalidate();
                }
            }

            public bool Remove(ControlBase item)
            {
                int idx = _controls.IndexOf(item);
                if (idx == -1)
                    return false;
                RemoveAt(idx);
                return true;
            }

            public void RemoveAt(int index)
            {
                ControlBase toremove = _controls[index];
                toremove.m_Parent = null;
                _controls.RemoveAt(index);
                _owner.OnChildRemoved(toremove);
                _owner.Invalidate();
            }
            public ControlBase[] ToArray()
            {
                ControlBase[] copy = new ControlBase[Count];
                CopyTo(copy, 0);
                return copy;
            }
            IEnumerator IEnumerable.GetEnumerator() => ((IList)_controls).GetEnumerator();
        }
    }
}