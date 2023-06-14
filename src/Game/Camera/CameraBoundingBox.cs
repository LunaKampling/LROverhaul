//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using linerider.Utils;
using OpenTK;
using System;
namespace linerider.Game
{
    public class CameraBoundingBox : GameService
    {
        public const double roundness = 0.8;
        public const double legacyratio = 0.125;
        public const double maxcamratio = 0.3;
        public const double mincamratio = 0.125;
        public Vector2d RiderPosition;
        public DoubleRect Bounds { get; private set; }
        private bool _initialized = false;
        private bool _smooth = false;
        public CameraBoundingBox(Rider rider)
        {
            RiderPosition = rider.CalculateCenter();
        }
        public CameraBoundingBox(Vector2d ridercenter)
        {
            RiderPosition = ridercenter;
        }
        public static CameraBoundingBox Create(Vector2d center, float zoom)
        {
            CameraBoundingBox box = new CameraBoundingBox(center);
            if (Settings.RoundLegacyCamera || Settings.SmoothCamera)
                box.SetupSmooth(0, zoom);
            else
                box.SetupLegacy(zoom);
            return box;
        }
        public void SetupSmooth(double ppf, float zoom)
        {
            double scale = GetSmoothCamRatio(ppf);
            double width = game.RenderSize.Width;
            double height = width * (9.0 / 16.0);//16:9 camera
            height /= game.Track.Zoom;
            width /= game.Track.Zoom;
            height *= scale;
            width *= scale;
            Bounds = new DoubleRect(RiderPosition.X - width / 2, RiderPosition.Y - height / 2, width, height);
            _initialized = true;
            _smooth = true;
        }
        public void SetupLegacy(float zoom)
        {
            double width = game.RenderSize.Width;
            double height = width * (9.0 / 16.0);//16:9 camera
            height /= game.Track.Zoom;
            width /= game.Track.Zoom;
            height *= legacyratio;
            width *= legacyratio;
            Bounds = new DoubleRect(RiderPosition.X - width / 2, RiderPosition.Y - height / 2, width, height);
            _initialized = true;
            _smooth = false;
        }
        public Vector2d Clamp(Vector2d camera)
        {
            if (!_initialized)
                throw new Exception("Camera Box was not initialized properly");
            DoubleRect bounds = Bounds;
            if (_smooth)
            {
                Vector2d oval = bounds.EllipseClamp(camera);
                Vector2d square = bounds.Clamp(camera);
                return oval == square ? oval : Vector2d.Lerp(square, oval, roundness);
            }
            else
            {
                return bounds.Clamp(camera);
            }
        }
        public bool SmoothIntersects(Vector2d camera)
        {
            if (!_initialized)
                throw new Exception("Camera Box was not initialized properly");
            DoubleRect bounds = Bounds;
            return bounds.Clamp(camera) == camera && bounds.EllipseClamp(camera) == camera;
        }
        public static double GetSmoothCamRatio(double ppf)
        {
            if (!Constants.ScaleCamera)
                return maxcamratio;
            const int floor = 5;
            const int ceil = 75;

            double scale1 = MathHelper.Clamp((ppf - floor) / ceil, 0, 1);
            return maxcamratio - (maxcamratio - mincamratio) * scale1;
        }
    }
}