﻿using Gwen;
using Gwen.Controls;
using linerider.Utils;

namespace linerider.UI
{
    public class TimelineEditorWindow : DialogBase
    {
        private const int _controlWidth = 70;
        public TimelineEditorWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Timeline Exact Values";
            MinimumSize = new Size(250, MinimumSize.Height);
            AutoSizeToContents = true;
            MakeModal(true);
            Setup();
        }

        private void Setup()
        {
            Panel positionGroup = GwenHelper.CreateHeaderPanel(this, "Current Position");

            Spinner positionFrameSpinner = new(null)
            {
                Min = 0,
                Max = Constants.MaximumFrames,
                Value = _editor.Offset,
                Width = _controlWidth,
            };
            TextBox positionTimeTextBox = new(null)
            {
                Width = _controlWidth,
                Text = Utility.FrameToTime(_editor.Offset),
            };
            _ = GwenHelper.CreateLabeledControl(positionGroup, "Frames", positionFrameSpinner);
            _ = GwenHelper.CreateLabeledControl(positionGroup, "Time", positionTimeTextBox);

            Panel durationGroup = GwenHelper.CreateHeaderPanel(this, "Total Duration");

            Spinner totalFramesSpinner = new(null)
            {
                Min = Constants.MinimumFrames,
                Max = Constants.MaximumFrames,
                Value = _canvas.TrackDuration,
                Width = _controlWidth,
            };
            TextBox totalTimeTextBox = new(null)
            {
                Width = _controlWidth,
                Text = Utility.FrameToTime(_canvas.TrackDuration),
            };
            _ = GwenHelper.CreateLabeledControl(durationGroup, "Frames", totalFramesSpinner);
            _ = GwenHelper.CreateLabeledControl(durationGroup, "Time", totalTimeTextBox);

            positionFrameSpinner.ValueChanged += (o, e) =>
            {
                int frame = (int)((Spinner)o).Value;

                if (frame == _editor.Offset)
                    return;

                if (!positionTimeTextBox.HasFocus)
                    positionTimeTextBox.Text = Utility.FrameToTime(frame);

                _editor.SetFrame(frame);
                _editor.UpdateCamera();

                if (frame > totalFramesSpinner.Value)
                    totalFramesSpinner.Value = frame;
            };

            totalFramesSpinner.ValueChanged += (o, e) =>
            {
                int frame = (int)((Spinner)o).Value;

                if (frame == _canvas.TrackDuration)
                    return;

                if (!totalTimeTextBox.HasFocus)
                    totalTimeTextBox.Text = Utility.FrameToTime(frame);

                _canvas.TrackDuration = frame;

                if (frame < positionFrameSpinner.Value)
                    positionFrameSpinner.Value = frame;
            };

            positionTimeTextBox.TextChanged += (o, e) =>
            {
                TextBox control = (TextBox)o;

                if (!control.HasFocus)
                    return;

                string time = control.Text;
                int frame = Utility.TimeToFrame(time);
                if (frame == -1 || frame < positionFrameSpinner.Min)
                {
                    control.TextColorOverride = Color.Red;
                }
                else
                {
                    control.TextColorOverride = Color.Empty;
                    positionFrameSpinner.Value = frame;
                }
            };

            totalTimeTextBox.TextChanged += (o, e) =>
            {
                TextBox control = (TextBox)o;

                if (!control.HasFocus)
                    return;

                string time = ((TextBox)o).Text;
                int frame = Utility.TimeToFrame(time);
                if (frame == -1 || frame < totalFramesSpinner.Min)
                {
                    control.TextColorOverride = Color.Red;
                }
                else
                {
                    control.TextColorOverride = Color.Empty;
                    totalFramesSpinner.Value = frame;
                }
            };

            ControlBase bottomContainer = new(this)
            {
                Margin = new Margin(0, 10, 0, 0),
                Dock = Dock.Bottom,
                AutoSizeToContents = true
            };
            Button closeButton = new(bottomContainer)
            {
                Text = "Close",
                Dock = Dock.Right,
                AutoSizeToContents = true,
            };
            closeButton.Clicked += (o, e) => Close();
        }
    }
}
