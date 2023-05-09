using linerider.Rendering;
using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using linerider.Game;
using linerider.Utils;
using System.Diagnostics;

namespace linerider
{
    public class HitTestManager
    {
        private HashSet<int> _allcollisions = new HashSet<int>();
        private AutoArray<HashSet<int>> _unique_frame_collisions = new AutoArray<HashSet<int>>();
        private HashSet<int> _renderer_changelist = new HashSet<int>();
        private Dictionary<int, int> _line_framehit = new Dictionary<int, int>();
        private int _currentframe = Disabled;
        const int Disabled = -1;
        public HitTestManager()
        {
            Reset();
        }
        public void MarkFirstInvalid(int frame)
        {
            for (int i = frame; i < _unique_frame_collisions.Count; i++)
            {
                var unique = _unique_frame_collisions[i];
                foreach (var hit in unique)
                {
                    _allcollisions.Remove(hit);
                    if (Settings.Editor.HitTest)
                        _renderer_changelist.Add(hit);
                    _line_framehit.Remove(hit);
                }
            }
            _unique_frame_collisions.RemoveRange(
                frame,
                _unique_frame_collisions.Count - frame);
            _currentframe = Math.Min(frame - 1, _currentframe);

        }
        public void AddFrame(LinkedList<int> collisions)
        {
            var list = new List<LinkedList<int>>();
            list.Add(collisions);
            AddFrames(list);
        }
        public void AddFrames(List<LinkedList<int>> collisionlist)
        {
            foreach (var collisions in collisionlist)
            {
                int frameid = _unique_frame_collisions.Count;
                HashSet<int> unique = new HashSet<int>();
                foreach (var collision in collisions)
                {
                    var id = collision;
                    if (_allcollisions.Add(id))
                    {
                        if (Settings.Editor.HitTest)
                        {
                            if (_currentframe >= frameid)
                                _renderer_changelist.Add(id);
                        }
                        unique.Add(id);
                        _line_framehit.Add(id, frameid);
                    }
                }
                _unique_frame_collisions.Add(unique);

            }
        }
        /// <summary>
        /// Sets the hit test position to the new frame and returns the
        /// line ids that need updating to the renderer.
        /// </summary>
        public HashSet<int> GetChangesForFrame(int newframe)
        {
            var ret = new HashSet<int>();
            var current = _currentframe;
            foreach (var v in _renderer_changelist)
            {
                ret.Add(v);
            }
            if (!Settings.Editor.HitTest)
            {
                newframe = Disabled;
                if (current != Disabled)
                {
                    foreach (var v in _allcollisions)
                    {
                        ret.Add(v);
                    }
                    current = Disabled;
                }
            }
            else if (current != newframe)
            {
                if (current == Disabled)
                    current = 0;
                // i'm leaving this in seperate loops for now
                // it's a lot more readable this way for changes
                if (newframe < current)
                {
                    // we're moving backwards.
                    // we compare to currentframe because we may have to
                    // remove its hit lines
                    for (int i = newframe; i <= current; i++)
                    {
                        foreach (var id in _unique_frame_collisions[i])
                        {
                            var framehit = _line_framehit[id];
                            //was hit, but isnt now
                            if (framehit > newframe)
                                ret.Add(id);
                        }
                    }
                }
                else
                {
                    // we're moving forwards
                    // we ignore currentframe, its render data is
                    // established
                    for (int i = current + 1; i <= newframe; i++)
                    {
                        foreach (var id in _unique_frame_collisions[i])
                        {
                            ret.Add(id);
                        }
                    }
                }
            }
            if (_currentframe != newframe || _renderer_changelist.Count != 0)
            {
                _renderer_changelist.Clear();
                _currentframe = newframe;
            }
            return ret;
        }
        public bool IsHit(int id)
        {
            var frame = GetHitFrame(id);
            if (frame != -1)
            {
                if (_currentframe >= frame)
                    return true;
            }
            return false;

        }
        public int GetHitFrame(int id)
        {
            if (_line_framehit.TryGetValue(id, out int frameid))
            {
                return frameid;
            }
            return -1;

        }
        public bool IsHitBy(int id, int frame)
        {
            if (_line_framehit.TryGetValue(id, out int frameid))
            {
                if (frame >= frameid)
                    return true;
            }
            return false;
        }

        public bool HasUniqueCollisions(int frame)
        {
            if (frame >= _unique_frame_collisions.Count)
                throw new IndexOutOfRangeException("Frame does not have hit test calculations");
            return _unique_frame_collisions[frame].Count != 0;
        }

        public void Reset()
        {
            if (_currentframe != -1)
            {
                foreach (var v in _allcollisions)
                {
                    _renderer_changelist.Add(v);
                }
            }
            _unique_frame_collisions.Clear();
            _unique_frame_collisions.Add(new HashSet<int>());
            _line_framehit.Clear();
            _allcollisions.Clear();
            _currentframe = Disabled;
        }
    }
}