using linerider.Utils;
using OpenTK;
using OpenTK.Mathematics;
namespace linerider.Game
{
    public class PredictiveCamera : ClampCamera
    {
        public override Vector2d GetFrameCamera(int frame)
        {
            _ = base.GetFrameCamera(frame);

            CameraBoundingBox box = CameraBoundingBox.Create(_frames[frame].RiderCenter, _zoom);
            return box.Clamp(CameraMotionReducer(frame));
        }
        /// <summary>
        /// Reduces the amount of movement the camera has to do to capture the
        /// rider. It does so predictively
        /// </summary>
        /// <returns>The reduced position in game coords</returns>
        private Vector2d CameraMotionReducer(int frame)
        {
            const int forwardcount = Constants.PhysicsRate;
            EnsureFrame(frame + forwardcount);
            Vector2d offset = CalculateOffset(frame);
            CameraBoundingBox box = CameraBoundingBox.Create(Vector2d.Zero, _zoom);
            CameraBoundingBox framebox = CameraBoundingBox.Create(
                _frames[frame].RiderCenter,
                _zoom);

            Vector2d center = Vector2d.Zero;
            int math = 0;
            for (int i = 0; i < forwardcount; i++)
            {
                CameraEntry f = _frames[frame + i];
                offset = box.Clamp(offset + f.CameraOffset);
                center += framebox.Clamp(f.RiderCenter + offset);
                math++;
            }
            // Force the rider to center at the beginning
            // it looks awkward to predict heavily at the start.
            return frame < forwardcount
                ? Vector2d.Lerp(
                    _frames[frame].RiderCenter,
                    center / math,
                    frame / (float)forwardcount)
                : center / math;
        }
        protected override Vector2d StepCamera(CameraBoundingBox box, ref Vector2d prev, int frame)
        {
            CameraEntry entry = _frames[frame];
            return box.Clamp(prev + entry.CameraOffset);
        }
    }
}
