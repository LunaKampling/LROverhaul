using linerider.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace linerider
{
    /// <summary>
    /// A linked list that contains the layers per track. A new LayerContainer is generated per track.
    /// In short this makes it so that we can load the layers first, and then proceed to assign the lines in the track to their layers.
    /// </summary>
    public class LayerContainer<T> : IEnumerable<T>, ICollection<T>
    where T : Layer
    {
        private readonly LinkedList<T> _list = new LinkedList<T>();
        public int Count => _list.Count;
        bool ICollection<T>.IsReadOnly => false;
        private Layer _layer = new Layer();
        public Layer defaultLayer;
        public Layer currentLayer;
        public LayerContainer()
        {
            _layer.name = "Base layer";
            AddLayer((T)_layer);
            defaultLayer = _layer;
            currentLayer = _layer;
        }

        /// <summary>
        /// Return the node that would come right after layer.id
        /// returns null if unsuccessful
        /// ex: if our state is 1 2 4 and layer.id is 3, this function returns 4
        /// </summary>
        protected virtual LinkedListNode<T> FindNodeAfter(LinkedListNode<T> node, T layer)
        {
            while (layer.ID < node.Value.ID)
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
        /// Combines all unique layers in a cell in order (Technically irrelevant, might comment out later)
        /// </summary>
        /// <param name="cell"></param>
        public void Combine(LayerContainer<T> cell)
        {
            if (this == cell)
                return;
            LinkedListNode<T> node = _list.First;
            foreach (T layer in cell)
            {
                if (node != null)
                {
                    node = FindNodeAfter(node, layer);
                    if (node == null)
                        node = _list.AddLast(layer);
                    else if (node.Value.ID != layer.ID) // No redundant layers :3
                        _ = _list.AddBefore(node, layer);
                }
                else
                {
                    node = _list.AddFirst(layer);
                }
            }
        }
        public void AddLayer(T layer)
        {
            LinkedListNode<T> node = _list.First;
            if (node != null)
            {
                if (layer.ID == null)
                {
                    layer.ID = _list.Last.Value.ID + 1;
                    _ = _list.AddLast(layer);
                    currentLayer = layer;
                    Debug.WriteLine(layer.ID);
                    return;
                }    
                else node = FindNodeAfter(node, layer);
                if (node == null)
                {
                    _ = _list.AddLast(layer);
                }
                else
                {
                    if (node.Value.ID != layer.ID)
                        _ = _list.AddBefore(node, layer);
                    else
                        Debug.WriteLine("Layer ID collision in layer container");
                }
            }
            else
            {
                layer.ID = 0;
                _ = _list.AddFirst(layer);
            }
        }
        public void RemoveLayer(int layerid)
        {
            LinkedListNode<T> node = _list.First;
            while (node != null)
            {
                if (node.Value.ID == layerid)
                {
                    _list.Remove(node);
                    return;
                }
                node = node.Next;
            }
            throw new Exception("Layer was not found");
        }
        //public LineContainer<T> Clone()
        //{
        //    LineContainer<T> ret = new LineContainer<T>();
        //    foreach (T l in this)
        //    {
        //        ret.AddLine(l);
        //    }
        //    return ret;
        //}
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        void ICollection<T>.Add(T item) => throw new NotImplementedException();//AddLayer(item);
        void ICollection<T>.Clear() => _list.Clear();
        bool ICollection<T>.Contains(T item) => throw new NotImplementedException();// return _list.Contains(item);
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();//      _list.CopyTo(array, arrayIndex);
        bool ICollection<T>.Remove(T item) => throw new NotImplementedException();//      return _list.Remove(item);
    }
}