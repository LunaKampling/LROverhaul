using linerider.Utils;
using System;
using System.Collections.Generic;

namespace linerider
{
    public class HitTestManager
    {
        private readonly HashSet<int> _allcollisions = new HashSet<int>();
        private readonly AutoArray<HashSet<int>> _unique_frame_collisions = new AutoArray<HashSet<int>>();
        private readonly HashSet<int> _renderer_changelist = new HashSet<int>();
        private readonly Dictionary<int, int> _line_framehit = new Dictionary<int, int>();
        private int _currentframe = Disabled;
        private const int Disabled = -1;
        public HitTestManager()
        {
            Reset();
        }
        public void MarkFirstInvalid(int frame)
        {
            for (int i = frame; i < _unique_frame_collisions.Count; i++)
            {
                HashSet<int> unique = _unique_frame_collisions[i];
                foreach (int hit in unique)
                {
                    _ = _allcollisions.Remove(hit);
                    if (Settings.Editor.HitTest)
                        _ = _renderer_changelist.Add(hit);
                    _ = _line_framehit.Remove(hit);
                }
            }
            _unique_frame_collisions.RemoveRange(
                frame,
                _unique_frame_collisions.Count - frame);
            _currentframe = Math.Min(frame - 1, _currentframe);

        }
        public void AddFrame(LinkedList<int> collisions)
        {
            List<LinkedList<int>> list = new List<LinkedList<int>>
            {
                collisions
            };
            AddFrames(list);
        }
        public void AddFrames(List<LinkedList<int>> collisionlist)
        {
            foreach (LinkedList<int> collisions in collisionlist)
            {
                int frameid = _unique_frame_collisions.Count;
                HashSet<int> unique = new HashSet<int>();
                foreach (int collision in collisions)
                {
                    int id = collision;
                    if (_allcollisions.Add(id))
                    {
                        if (Settings.Editor.HitTest)
                        {
                            if (_currentframe >= frameid)
                                _ = _renderer_changelist.Add(id);
                        }
                        _ = unique.Add(id);
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
            HashSet<int> ret = new HashSet<int>();
            int current = _currentframe;
            foreach (int v in _renderer_changelist)
            {
                _ = ret.Add(v);
            }
            if (!Settings.Editor.HitTest)
            {
                newframe = Disabled;
                if (current != Disabled)
                {
                    foreach (int v in _allcollisions)
                    {
                        _ = ret.Add(v);
                    }
                }
            }
            else if (current != newframe)
            {
                if (current == Disabled)
                    current = 0;
                // I'm leaving this in seperate loops for now
                // it's a lot more readable this way for changes
                if (newframe < current)
                {
                    // We're moving backwards.
                    // We compare to currentframe because we may have to
                    // remove its hit lines
                    for (int i = newframe; i <= current; i++)
                    {
                        foreach (int id in _unique_frame_collisions[i])
                        {
                            int framehit = _line_framehit[id];
                            // Was hit, but isnt now
                            if (framehit > newframe)
                                _ = ret.Add(id);
                        }
                    }
                }
                else
                {
                    // We're moving forwards.
                    // We ignore currentframe, its render data is
                    // established.
                    for (int i = current + 1; i <= newframe; i++)
                    {
                        foreach (int id in _unique_frame_collisions[i])
                        {
                            _ = ret.Add(id);
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
            int frame = GetHitFrame(id);
            if (frame != -1)
            {
                if (_currentframe >= frame)
                    return true;
            }
            return false;

        }
        public int GetHitFrame(int id) => _line_framehit.TryGetValue(id, out int frameid) ? frameid : -1;
        public bool IsHitBy(int id, int frame)
        {
            if (_line_framehit.TryGetValue(id, out int frameid))
            {
                if (frame >= frameid)
                    return true;
            }
            return false;
        }

        public bool HasUniqueCollisions(int frame) => frame >= _unique_frame_collisions.Count
                ? throw new IndexOutOfRangeException("Frame does not have hit test calculations")
                : _unique_frame_collisions[frame].Count != 0;

        public void Reset()
        {
            if (_currentframe != -1)
            {
                foreach (int v in _allcollisions)
                {
                    _ = _renderer_changelist.Add(v);
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