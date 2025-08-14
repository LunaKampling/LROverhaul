using OpenTK.Mathematics;

namespace linerider.Game
{
    public class ClampCamera : ICamera
    {
        protected override Vector2d StepCamera(CameraBoundingBox box, ref Vector2d prev, int frame)
        {
            CameraEntry entry = _frames[frame];
            return box.Clamp(prev + entry.CameraOffset);
        }
    }
}