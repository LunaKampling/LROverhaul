using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms.VisualStyles;
using Gwen;
using Gwen.Controls;
using linerider.Game;
using linerider.Rendering;
using linerider.Tools;
using linerider.Utils;
using OpenTK;
using System.Windows.Forms;

namespace linerider.LRL
{
    public static class Coordinates
    {
        public static double SledTLX;
        public static double SledBLX;
        public static double SledBRX;
        public static double SledTRX;
        public static double BodyBuX;
        public static double BodyShX;
        public static double BodyHLX;
        public static double BodyHRX;
        public static double BodyFLX;
        public static double BodyFRX;
        public static double SledTLY;
        public static double SledBLY;
        public static double SledBRY;
        public static double SledTRY;
        public static double BodyBuY;
        public static double BodyShY;
        public static double BodyHLY;
        public static double BodyHRY;
        public static double BodyFLY;
        public static double BodyFRY;

        public static double[] CoordsX = new double[] { SledTLX, SledBLX, SledBRX, SledTRX, BodyBuX, BodyShX, BodyHLX, BodyHRX, BodyFLX, BodyFRX };
        public static double[] CoordsY = new double[] { SledTLY, SledBLY, SledBRY, SledTRY, BodyBuY, BodyShY, BodyHLY, BodyHRY, BodyFLY, BodyFRY };
        public static string[] ConPName = new string[] { "SledTL", "SledBR", "SledBR", "SledTR", "BodyBu", "BodySh", "BodyHL", "BodyHR", "BodyFL", "BodyFR" };
        public static string[] CoordsData = new string[] { "", "", "", "", "", "", "", "", "", "" }; 

        public static int frame;
        public static int iteration;

        public static bool xClipboard;
        public static bool yClipboard;
        public static int integerClipboard;

        public static void CoordsUpdate()
        {
            var game = GameRenderer.Game;
            frame = game.Track.Offset;
            iteration = game.Track.IterationsOffset;
            Rider rider = game.Track.Timeline.GetFrame(frame, iteration);

            for (var i = 0; i < ConPName.Length; i++)
            {
                CoordsX[i] = rider.Body[i].Location.X;
                CoordsY[i] = rider.Body[i].Location.Y;

                CoordsData[i] = ConPName[i] + " " + CoordsX[i].ToString("G17") + "X " + CoordsY[i].ToString("G17") + "Y";
            }
        }
        public static void SaveToClipboard()
        {
            var game = GameRenderer.Game;
            frame = game.Track.Offset;
            iteration = game.Track.IterationsOffset;
            Rider rider = game.Track.Timeline.GetFrame(frame, iteration);

            if (xClipboard)
            {
                Clipboard.SetText(rider.Body[integerClipboard].Location.X.ToString("G17"));
            }
            if (yClipboard)
            {
                    Clipboard.SetText(rider.Body[integerClipboard].Location.Y.ToString("G17"));
            }
        }
    };
}