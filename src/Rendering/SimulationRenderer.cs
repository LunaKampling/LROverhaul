using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using linerider.Drawing;
using linerider.Game;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Threading;
using Gwen.Controls;
using linerider.Tools;
using linerider.Audio;
using linerider.Utils;
using linerider.IO;

namespace linerider.Rendering
{
    public class SimulationRenderer : GameService
    {
        private TrackRenderer _trackrenderer;
        public RiderRenderer _riderrenderer;

        public bool RequiresUpdate
        {
            get
            {
                return _trackrenderer.RequiresUpdate;
            }
            set
            {
                _trackrenderer.RequiresUpdate = value;
            }
        }
        public SimulationRenderer()
        {
            _trackrenderer = new TrackRenderer();
            _riderrenderer = new RiderRenderer();
        }
        public void Render(Track track, Timeline timeline, ICamera camera, DrawOptions options)
        {
            Rider drawrider = options.Rider;
            if (options.OverlayFrame != -1)
            {
                var offs = camera.GetFrameCamera(options.OverlayFrame);
                var diff = offs - camera.GetFrameCamera(game.Track.Offset);
                GL.PushMatrix();
                GL.Translate(new Vector3d(-diff * game.Track.Zoom));
                var overlayopts = new DrawOptions();
                overlayopts.Zoom = options.Zoom;
                overlayopts.LineColors = false;
                overlayopts.Overlay = true;
                _trackrenderer.Render(overlayopts, TrackRecorder.Recording);
                GL.PopMatrix();
            }
            _trackrenderer.Render(options, TrackRecorder.Recording);
            if (Settings.OnionSkinning)
            {
                for (int i = -Settings.PastOnionSkins; i <= Settings.FutureOnionSkins; i++)
                {
                    var frame = game.Track.Offset + i;
                    if (frame > 0 && frame < game.Track.FrameCount && i != 0)
                    {
                        var onionskin = timeline.GetFrame(frame);
                        _riderrenderer.DrawRider(
                            0.2f,
                            onionskin,
                            false);
                        if (options.ShowMomentumVectors)
                        {
                            _riderrenderer.DrawMomentum(onionskin, 0.5f);
                        }
                        if (options.ShowContactLines)
                        {
                            _riderrenderer.DrawContacts(onionskin, timeline.DiagnoseFrame(frame), 0.5f);
                        }
                    }
                }
            }
            if (options.DrawFlag)
                _riderrenderer.DrawRider(
                    0.3f,
                    options.FlagRider,
                    true);

            _riderrenderer.DrawRider(
                Settings.InvisibleRider ? 0 : options.ShowContactLines ? 0.5f : 1,
                options.Rider,
                true);
            if (options.ShowMomentumVectors)
            {
                _riderrenderer.DrawMomentum(options.Rider, 1);
                if (
                    !options.IsRunning &&
                    options.Iteration != 6 &&
                    options.Iteration != 0 &&
                    !Settings.OnionSkinning)
                {
                    var frame = timeline.GetFrame(game.Track.Offset + 1, 0);
                    _riderrenderer.DrawRider(0.1f, frame);
                    if (options.ShowContactLines)
                    {
                        _riderrenderer.DrawContacts(frame, timeline.DiagnoseFrame(game.Track.Offset + 1, 0), 0.5f);
                    }
                }
            }
            if (options.ShowContactLines)
            {
                _riderrenderer.DrawContacts(options.Rider, options.RiderDiagnosis, 1);
            }
            _riderrenderer.Scale = options.Zoom;
            _riderrenderer.Draw();
            CurrentTools.SelectedTool.Render();
            _riderrenderer.DrawLines();
            _riderrenderer.Clear();
        }
        public void AddLine(GameLine l)
        {
            _trackrenderer.AddLine(l);
        }
        public void RedrawLine(GameLine l)
        {
            _trackrenderer.LineChanged(l);
        }
        public void RemoveLine(GameLine l)
        {
            _trackrenderer.RemoveLine(l);
        }
        public void RefreshTrack(Track track)
        {
            _trackrenderer.InitializeTrack(track);
        }
    }
}