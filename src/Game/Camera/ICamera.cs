//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using linerider.Utils;
using OpenTK;
using System;

namespace linerider.Game
{
    public abstract class ICamera
    {
        protected AutoArray<CameraEntry> _frames = new AutoArray<CameraEntry>();
        private const int cacherate = 40;
        private readonly AutoArray<Vector2d> _framecache = new AutoArray<Vector2d>();
        private Vector2d _prevcamera = Vector2d.Zero;
        private int _prevframe = -1;
        private float _cachezoom = 1;
        private float _blend = 1;
        private Vector2d _center = Vector2d.Zero;
        private Vector2d _cachedcenter = Vector2d.Zero;
        private Vector2d _cachedprevcenter = Vector2d.Zero;
        protected Timeline _timeline;
        protected int _currentframe = 0;
        protected float _zoom = 1;
        public ICamera()
        {
            _frames.Add(new CameraEntry(Vector2d.Zero));
            _framecache.Add(Vector2d.Zero);
        }
        protected abstract Vector2d StepCamera(CameraBoundingBox box, ref Vector2d prev, int frame);
        public void SetTimeline(Timeline timeline)
        {
            if (timeline == null)
                throw new Exception("Attempt to set null timeline for camera");
            if (_timeline != timeline)
            {
                _timeline = timeline;
                InvalidateFrame(1);
            }
        }
        public void InvalidateFrame(int frame)
        {
            if (frame <= 0)
                throw new Exception("Cannot invalidate frame 0 for camera");
            if (frame < _frames.Count)
            {
                _frames.RemoveRange(
                    frame,
                    _frames.Count - frame);
                if (_prevframe <= frame)
                    _prevframe = -1;
            }
            int cachepos = frame / cacherate;
            if (frame % cacherate != 0)
                cachepos++;

            if (cachepos < _framecache.Count)
            {
                _framecache.RemoveRange(cachepos, _framecache.Count - cachepos);
            }
            if (frame == 1)
            {
                Rider firstframe = _timeline.GetFrame(0);
                CameraEntry entry = new CameraEntry(firstframe.CalculateCenter());
                _frames[0] = entry;
                _framecache[0] = Vector2d.Zero;
            }
        }
        public virtual Vector2d GetFrameCamera(int frame)
        {
            if (_zoom != _cachezoom)
            {
                _cachezoom = _zoom;
                _prevframe = -1;
                _framecache.UnsafeSetCount(1);
            }
            EnsureFrame(frame);
            Vector2d offset = CalculateOffset(frame);
            _prevframe = frame;
            _prevcamera = offset;
            return _frames[frame].RiderCenter + offset;
        }

        public Vector2d GetCenter(bool force = false)
        {
            if (_center == Vector2d.Zero)
            {
                if (_zoom != _cachezoom || force)
                    _cachedcenter = Vector2d.Zero;
                if (_cachedcenter == Vector2d.Zero)
                {
                    if (_blend != 1)
                    {
                        _cachedprevcenter = GetFrameCamera(Math.Max(0, _currentframe - 1));
                        _cachedcenter = GetFrameCamera(_currentframe);
                        _cachezoom = _zoom;
                    }
                    else
                    {
                        _cachedcenter = GetFrameCamera(_currentframe);
                        _cachedprevcenter = _cachedcenter;
                        _cachezoom = _zoom;
                    }
                }
                return Vector2d.Lerp(_cachedprevcenter, _cachedcenter, _blend);
            }
            return _center;
        }
        public void SetFrame(int frame)
        {
            _center = Vector2d.Zero;
            if (_currentframe != frame)
            {
                _currentframe = frame;
                _cachedcenter = Vector2d.Zero;
            }
        }
        public virtual void BeginFrame(float blend, float zoom)
        {
            if (_blend != blend)
            {
                _blend = blend;
                _cachedcenter = Vector2d.Zero;
            }
            _zoom = zoom;
        }
        public void SetFrameCenter(Vector2d center)
        {
            _center = center;
            _cachedcenter = Vector2d.Zero;
        }
        public DoubleRect GetViewport(
            float zoom,
            int maxwidth,
            int maxheight)
        {
            Vector2d center = GetCenter();
            Vector2d size = new Vector2d(maxwidth / zoom, maxheight / zoom);
            Vector2d origin = center - size / 2;
            return new DoubleRect(origin, size);
        }
        public void OnResize()
        {
            SetFrameCenter(GetCenter());
            InvalidateFrame(1);
        }
        public DoubleRect GetClamp(float zoom, int width, int height)
        {
            DoubleRect ret = GetViewport(zoom, width, height);
            Vector2d pos = ret.Vector + ret.Size / 2;
            CameraBoundingBox b = new CameraBoundingBox(pos);
            if (Settings.SmoothCamera || Settings.RoundLegacyCamera)
            {
                b.SetupSmooth(GetPPF(_currentframe), _zoom);
                return b.Bounds;
            }
            else
            {
                b.SetupLegacy(_zoom);
                return b.Bounds;
            }
        }
        protected void EnsureFrame(int frame)
        {
            // Ensure timeline has the frames for us.
            // Also timeline might invalidate our prev frames when calling
            // so we do this at the top so it doesnt invalidate while calculating.
            _ = _timeline.GetFrame(frame);
            if (frame >= _frames.Count)
            {
                int diff = frame - (_frames.Count - 1);
                Rider[] frames = _timeline.GetFrames(_frames.Count, diff);
                CameraEntry camoffset = _frames[_frames.Count - 1];
                for (int i = 0; i < diff; i++)
                {
                    Vector2d center = frames[i].CalculateCenter();
                    Vector2d offset = camoffset.RiderCenter - center;
                    camoffset = new CameraEntry(center, offset, Vector2d.Zero);
                    _frames.Add(camoffset);
                }
            }
        }
        protected Vector2d CalculateOffset(int frame)
        {
            CameraBoundingBox box = CameraBoundingBox.Create(Vector2d.Zero, _zoom);
            if (_prevframe != -1 &&
                _prevframe <= frame &&
                (frame - _prevframe) <= 1)
            {
                if (frame == _prevframe)
                    return _prevcamera;
                if (frame % cacherate != 0)
                    return box.Clamp(StepCamera(box, ref _prevcamera, frame));
            }
            int cachepos = Math.Min(frame / cacherate, _framecache.Count - 1);
            int framestart = cachepos * cacherate;
            Vector2d start = _framecache[cachepos];

            for (int i = framestart; i <= frame; i++)
            {
                if (i % cacherate == 0 && i / cacherate == _framecache.Count)
                {
                    _framecache.Add(start);
                }
                start = StepCamera(box, ref start, i);
            }
            // Debug.WriteLine("Calculating " + framestart + "-" + (frame) + " for legacy camera");
            return box.Clamp(start);
        }
        protected virtual double GetPPF(int frame) => 0;
    }
}
