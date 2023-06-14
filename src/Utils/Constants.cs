using OpenTK.Graphics;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace linerider.Utils
{
    internal static class Constants
    {
        public static Color4 TriggerBGColor = new Color4(244, 245, 249, 255);
        public static Color4 StaticTriggerBGColor = new Color4(244, 245, 249, 255);
        public static Color TriggerLineColorChange = Color.Black;
        public static Color StaticTriggerLineColorChange = Color.Black;

        public static readonly Color BgExportColor = Color.White;
        public static readonly Color BgEditorColor = Color.FromArgb(0xF9, 0xF9, 0xF9);
        public static readonly Color BgEditorNightColor = Color.FromArgb(0x33, 0x33, 0x33);
        public static readonly int[] MotionArray =
        {
            1, 2, 5, 10, 20, 30, 40, 80, 160, 320, 640
        };
        public static readonly Color RedLineColor = Color.FromArgb(0xE5, 0x39, 0x35);
        public static readonly Color BlueLineColor = Color.FromArgb(0x21, 0x96, 0xF3);
        public static readonly Color SceneryLineColor = Color.FromArgb(0x43, 0xA0, 0x47);
        public static readonly Color TriggerLineColor = Color.FromArgb(0xFF, 0x95, 0x4F);
        public static readonly Color ExportLineColor = Color.Black;
        public static readonly Color DefaultLineColor = Color.FromArgb(0x33, 0x33, 0x33);
        public static readonly Color DefaultNightLineColor = Color.FromArgb(0xEE, 0xEE, 0xEE);

        public static Color ConstraintColor = Color.FromArgb(0xCC, 0x72, 0xB7);
        public static Color ConstraintRepelColor = Color.CornflowerBlue;
        public static Color ConstraintFirstBreakColor = Color.FromArgb(0xFF, 0x8C, 0x00);
        public static Color ConstraintBreakColor = Color.FromArgb(0xE6, 0x7E, 0x00);
        public static Color ContactPointColor = Color.Cyan;
        public static Color ContactPointFakieColor = Color.Blue;
        public static Color MomentumVectorColor = Color.Red;

        public static readonly string TracksDirectory = Program.UserDirectory + TracksFolderName + System.IO.Path.DirectorySeparatorChar;
        public static readonly string RendersDirectory = Program.UserDirectory + RendersFolderName + System.IO.Path.DirectorySeparatorChar;
        public const string RendersFolderName = "Renders";
        public const string TracksFolderName = "Tracks";
        public const string DefaultTrackName = "<untitled>";

        public const float DefaultZoom = 4;
        public const int PhysicsRate = 40;
        public static bool ScaleCamera = true;
        public const double MinimumZoom = 0.1;
        public const float MaxZoom = 24;
        public const float MaxSuperZoom = 200;
        public const int MaxFrames = 40 * 60 * 60 * 3; // 3 hours of frames
        public const string InternalDefaultName = "*default*";

        public static readonly Size ScreenSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        public static readonly Size WindowSize = new Size(Math.Max(1280, (int)Math.Round(ScreenSize.Width / 1.5)), Math.Max(720, (int)Math.Round(ScreenSize.Height / 1.5)));
        public static readonly float ScreenScale = Math.Max(1, (ScreenSize.Width / 1600 < ScreenSize.Height / 1080) ? (ScreenSize.Width / 1600) : (ScreenSize.Height / 1080));

        public static readonly string GithubPageHeader = "https://github.com/LunaKampling/LROverhaul";
        public static readonly string GithubRawHeader = "https://raw.githubusercontent.com/LunaKampling/LROverhaul";
        public static readonly string FfmpegHelperHeader = "https://github.com/jealouscloud/lra-ffmpeg/releases/download/ffmpeg4.0-x64/ffmpeg";
    }
}