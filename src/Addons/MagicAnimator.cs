using linerider.Game;
using linerider.Tools;
using linerider.Utils;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace linerider.Addons
{
    public class MagicAnimator : BaseAddon
    {
        private static void MoveFrameStatic(List<LineSelection> selectedLines, int direction, bool isCompleteAction)
        {
            // TODO
        }

        private static void MoveFrameRelative(List<LineSelection> selectedLines, int direction, bool isCompleteAction)
        {
            RiderFrame flag = window.Track.GetFlag();
            int currentFrame = window.Track.Offset;
            if (flag == null || currentFrame <= flag.FrameID)
            {
                // Behavior only defined if flag is set and current frame is ahead of it
                return;
            }

            Rider flagFrameRider = flag.State;
            Rider flagNextFrameRider = window.Track.Timeline.ExtractFrame(flag.FrameID + 1).State;

            // Where the Rider was at the flag frame, and the frame after that
            // This establishes the initial frame of reference
            //
            // flagNextFrameRider is considered to be the frame with 0 relative velocity to the reference frame
            // All frames after will look as if the rider has started falling within the reference frame, because gravity.
            // 
            // This is all if the user-configured relative speeds are (0, 0). If the user changes these speeds,
            // the lines will be drawn accordingly.
            Vector2d flagFramePos = Game.Rider.GetBounds(flagFrameRider).Vector;
            Vector2d flagNextFramePos = Game.Rider.GetBounds(flagNextFrameRider).Vector;

            // The difference between where the rider was on frames 0 and 1 establishes a reference speed to apply
            Vector2d firstFrameDiff = Vector2d.Subtract(flagNextFramePos, flagFramePos);
            // Add the user-configurable speed offsets
            firstFrameDiff = Vector2d.Add(firstFrameDiff, new Vector2d(Settings.animationRelativeVelX, Settings.animationRelativeVelY));

            int framesElapsed = currentFrame - flag.FrameID;

            // Apply the speed vector to the number of elapsed frames, and add it to the initial reference frame
            // to get an expected position for the current frame
            Vector2d currentFrameExpectedPos = Vector2d.Add(Vector2d.Multiply(firstFrameDiff, framesElapsed), flagFramePos);
            // Same for the next frame
            Vector2d nextFrameExpectedPos = Vector2d.Add(Vector2d.Multiply(firstFrameDiff, framesElapsed + direction), flagFramePos);

            if (isCompleteAction)
            {
                window.Invalidate();
                window.Track.UndoManager.BeginAction();
            }
            TrackWriter trackWriter = window.Track.CreateTrackWriter();
            List<GameLine> newLines = new List<GameLine>();
            foreach (LineSelection selection in selectedLines)
            {
                GameLine selectedLine = selection.line;
                Vector2d p1 = selectedLine.Position;
                Vector2d p2 = selectedLine.Position2;

                Vector2d diff1 = Vector2d.Subtract(p1, currentFrameExpectedPos);
                Vector2d diff2 = Vector2d.Subtract(p2, currentFrameExpectedPos);

                Vector2d nextP1 = Vector2d.Add(nextFrameExpectedPos, diff1);
                Vector2d nextP2 = Vector2d.Add(nextFrameExpectedPos, diff2);

                // Add a new line in the same position, then move the existing line to maintain the selection
                GameLine newLine;
                if (!Settings.forwardLinesAsScenery && (direction > 0 || !Settings.recededLinesAsScenery))
                {
                    switch (selection.line.Type)
                    {
                        case LineType.Red:
                            newLine = new RedLine(nextP1, nextP2, ((RedLine)selectedLine).inv);
                            break;
                        case LineType.Blue:
                            newLine = new StandardLine(nextP1, nextP2, ((StandardLine)selectedLine).inv);
                            break;
                        case LineType.Scenery:
                            newLine = new SceneryLine(nextP1, nextP2);
                            break;
                        default:
                            newLine = new SceneryLine(nextP1, nextP2);
                            break;
                    }
                }
                else
                {
                    newLine = new SceneryLine(nextP1, nextP2);
                }
                newLines.Add(newLine);
            }

            var selectTool = CurrentTools.SelectTool;
            foreach (GameLine newLine in newLines)
            {
                trackWriter.AddLine(newLine);
            }
            selectTool.SelectLines(newLines);



            if (isCompleteAction)
            {
                window.Track.UndoManager.EndAction();
                window.Track.NotifyTrackChanged();
            }
        }

        private static void MoveFrame(List<LineSelection> selectedLines, int direction = 1, bool isCompleteAction = true)
        {
            if (Settings.velocityReferenceFrameAnimation)
            {
                MoveFrameRelative(selectedLines, direction, isCompleteAction);
            }
            else
            {
                MoveFrameStatic(selectedLines, direction, isCompleteAction);
            }
        }

        private static List<LineSelection> GetLineSelections()
        {
            if (!CurrentTools.SelectedTool.Equals(CurrentTools.SelectTool))
            {
                // This tool shouldn't work mid-selection, or if the Selection tool isn't active
                return new List<LineSelection>();
            }

            return CurrentTools.SelectTool.GetLineSelections();
        }

        public static void AdvanceFrame()
        {
            List<LineSelection> selections = GetLineSelections();
            if (selections.Any())
            {
                MoveFrame(selections);
            }
        }
        public static void RecedeFrame()
        {
            List<LineSelection> selections = GetLineSelections();
            if (selections.Any())
            {
                MoveFrame(selections, -1);
            }
        }

        public static void RecedeMultiFrame()
        {
            List<LineSelection> selections = GetLineSelections();
            if (!selections.Any())
            {
                return;
            }
            RiderFrame flag = window.Track.GetFlag();
            int currentFrame = window.Track.Offset;
            int framesElapsed = currentFrame - flag.FrameID;

            window.Invalidate();
            window.Track.UndoManager.BeginAction();
            for (int i = 0; i < framesElapsed; i++)
            {
                MoveFrame(selections, -1, false);
            }
            window.Track.UndoManager.EndAction();
            window.Track.NotifyTrackChanged();
        }
    }
}
