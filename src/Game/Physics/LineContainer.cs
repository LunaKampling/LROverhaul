using linerider.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace linerider
{
    /// <summary>
    /// A grid cell for the line rider simulation that puts lines
    /// with greater ids first. Newer = higher id, unless it's a scenery id
    /// Lines are guaranteed unique by ID, but collisions do not replace the 
    /// original or throw an exception
    /// </summary>
    public class LineContainer<T> : IEnumerable<T>, ICollection<T>
    where T : GameLine
    {
        private readonly LinkedList<T> _list = new LinkedList<T>();
        public int Count => _list.Count;
        bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// Return the node that would come right after line.id
        /// returns null if unsuccessful
        /// ex: if our state is 1 2 4 and line.id is 3, this function returns 4
        /// </summary>
        protected virtual LinkedListNode<T> FindNodeAfter(LinkedListNode<T> node, T line)
        {
            while (line.ID < node.Value.ID)
            {
                node = node.Next;
                if (node == null)
                {
                    return null;
                }
            }
            return node;
        }

        /// <summary>
        /// Combines all unique lines in a cell in order
        /// </summary>
        /// <param name="cell"></param>
        public void Combine(LineContainer<T> cell)
        {
            if (this == cell)
                return;
            LinkedListNode<T> node = _list.First;
            foreach (T line in cell)
            {
                if (node != null)
                {
                    node = FindNodeAfter(node, line);
                    if (node == null)
                        node = _list.AddLast(line);
                    else if (node.Value.ID != line.ID) // No redundant lines
                        _ = _list.AddBefore(node, line);
                }
                else
                {
                    node = _list.AddFirst(line);
                }
            }
        }
        public void AddLine(T line)
        {
            LinkedListNode<T> node = _list.First;
            if (node != null)
            {
                node = FindNodeAfter(node, line);
                if (node == null)
                {
                    _ = _list.AddLast(line);
                }
                else
                {
                    if (node.Value.ID != line.ID)
                        _ = _list.AddBefore(node, line);
                    else
                        Debug.WriteLine("Line ID collision in line container");
                }
            }
            else
            {
                _ = _list.AddFirst(line);
            }
        }
        public void RemoveLine(int lineid)
        {
            LinkedListNode<T> node = _list.First;
            while (node != null)
            {
                if (node.Value.ID == lineid)
                {
                    _list.Remove(node);
                    return;
                }
                node = node.Next;
            }
            throw new Exception("Line was not found in the chunk");
        }
        public LineContainer<T> Clone()
        {
            LineContainer<T> ret = new LineContainer<T>();
            foreach (T l in this)
            {
                ret.AddLine(l);
            }
            return ret;
        }
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        void ICollection<T>.Add(T item) => throw new NotImplementedException();//AddLine(item);
        void ICollection<T>.Clear() => _list.Clear();
        bool ICollection<T>.Contains(T item) => throw new NotImplementedException();// return _list.Contains(item);
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();//      _list.CopyTo(array, arrayIndex);
        bool ICollection<T>.Remove(T item) => throw new NotImplementedException();//      return _list.Remove(item);
    }
}