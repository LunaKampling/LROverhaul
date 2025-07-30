using linerider.Drawing;
using linerider.Game;
using linerider.IO;
using linerider.Tools;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace linerider.Rendering
{
    public class SimulationRenderer : GameService
    {
        private readonly TrackRenderer _trackrenderer;
        public RiderRenderer _riderrenderer;

        public bool RequiresUpdate
        {
            get => _trackrenderer.RequiresUpdate;
            set => _trackrenderer.RequiresUpdate = value;
        }
        public SimulationRenderer()
        {
            _trackrenderer = new TrackRenderer();
            _riderrenderer = new RiderRenderer();
        }
        public void Render(Track track, Timeline timeline, ICamera camera, DrawOptions options)
        {
            _ = options.Rider;
            if (options.OverlayFrame != -1)
            {
                Vector2d offs = camera.GetFrameCamera(options.OverlayFrame);
                Vector2d diff = offs - camera.GetFrameCamera(game.Track.Offset);
                GL.PushMatrix();
                GL.Translate(new Vector3d(-diff * game.Track.Zoom));
                DrawOptions overlayopts = new()
                {
                    Zoom = options.Zoom,
                    LineColors = false,
                    Overlay = true
                };
                _trackrenderer.Render(overlayopts, TrackRecorder.Recording);
                GL.PopMatrix();
            }
            _trackrenderer.Render(options, TrackRecorder.Recording);
            if (Settings.OnionSkinning)
            {
                for (int i = -Settings.PastOnionSkins; i <= Settings.FutureOnionSkins; i++)
                {
                    int frame = game.Track.Offset + i;
                    if (frame > 0 && frame < game.Track.FrameCount && i != 0)
                    {
                        Rider onionskin = timeline.GetFrame(frame);
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
                    options.Moment.Iteration != 6 &&
                    options.Moment.Iteration != 0 &&
                    !Settings.OnionSkinning)
                {
                    Rider frame = timeline.GetFrame(game.Track.Offset + 1, 0);
                    _riderrenderer.DrawRider(0.1f, frame);
                    if (options.ShowContactLines)
                    {
                        _riderrenderer.DrawContacts(frame, timeline.DiagnoseFrame(game.Track.Offset + 1, 0), 0.5f);
                    }
                }
            }
            if (options.ShowContactLines)
            {
                _riderrenderer.DrawContacts(options.Rider, options.RiderDiagnosis, 1, options.Moment.Iteration == 0 ? RiderConstants.Subiterations : options.Moment.Subiteration);
            }
            _riderrenderer.Scale = options.Zoom;
            _riderrenderer.Draw();
            CurrentTools.CurrentTool.Render();
            _riderrenderer.DrawLines();
            _riderrenderer.Clear();
        }
        public void AddLine(GameLine l) => _trackrenderer.AddLine(l);
        public void RedrawLine(GameLine l) => _trackrenderer.LineChanged(l);
        public void RemoveLine(GameLine l) => _trackrenderer.RemoveLine(l);
        public void RefreshTrack(Track track) => _trackrenderer.InitializeTrack(track);
    }
}