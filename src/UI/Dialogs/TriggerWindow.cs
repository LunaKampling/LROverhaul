using Gwen;
using Gwen.Controls;
using linerider.Game;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace linerider.UI
{
    public class TriggerWindow : DialogBase
    {
        private TriggerType triggerSelected;
        private Panel _triggeroptions;

        private const int FramePadding = 0;
        private HorizontalSlider _slider;
        private ListBox _lbtriggers;

        private ControlBase _zoomoptions;
        private Spinner _spinnerStart;
        private Spinner _spinnerDuration;
        private ComboBox _triggertype;
        private Spinner _zoomtarget;

        private ControlBase _bgoptions;
        private ControlBase _bgtraget;

        private ControlBase _lineoptions;
        private ControlBase _linetarget;

        private int SliderFrames => TriggerDuration + FramePadding * 2;
        private int TriggerDuration => (int)_spinnerDuration.Value;
        private int SelectedTrigger => _lbtriggers.SelectedRowIndex;
        private readonly List<GameTrigger> _triggers_copy = new List<GameTrigger>();
        private GameTrigger _trigger_copy = null;
        private bool _changemade = false;
        private bool _closing = false;
        private readonly bool _shouldrevertpreview = false;

        public TriggerWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Timeline Triggers";
            Size = new Size(400, 340);
            Padding = Padding.Zero;
            MakeModal(true);
            using (TrackWriter trk = editor.CreateTrackWriter())
            {
                foreach (GameTrigger trigger in trk.Triggers)
                {
                    _triggers_copy.Add(trigger.Clone());
                }
            }
            Setup();
            ToggleDisable(true);
            MinimumSize = new Size(250, MinimumSize.Height);
            DisableResizing();
            _shouldrevertpreview = !Settings.PreviewMode;
            Settings.PreviewMode = true;
        }

        protected override void CloseButtonPressed(
            ControlBase control,
            EventArgs args)
        {
            if (_closing || !_changemade)
            {
                _closing = true;
                if (_shouldrevertpreview)
                    Settings.PreviewMode = false;
                base.CloseButtonPressed(control, args);
            }
            else
            {
                WarnClose();
            }
        }
        public override bool Close()
        {
            if (_closing || !_changemade)
            {
                _closing = true;
                if (_shouldrevertpreview)
                    Settings.PreviewMode = false;
                return base.Close();
            }
            else
            {
                WarnClose();
                return false;
            }
        }
        private void WarnClose()
        {
            MessageBox mbox = MessageBox.Show(
                _canvas,
                "The line has been modified. Do you want to save your changes?",
                "Save Changes?",
                MessageBox.ButtonType.YesNoCancel);
            mbox.RenameButtonsYN("Save", "Discard", "Cancel");
            mbox.MakeModal(false);
            mbox.Dismissed += (o, e) =>
            {
                switch (e)
                {
                    case DialogResult.Yes:
                        FinishChange();
                        _closing = true;
                        _ = base.Close();
                        break;
                    case DialogResult.No:
                        CancelChange();
                        _closing = true;
                        _ = base.Close();
                        break;
                }
            };
        }
        private GameTrigger BeginModifyTrigger(TrackWriter trk)
        {
            int selected = SelectedTrigger;
            if (selected == -1)
                return null;
            GameTrigger trigger = trk.Triggers[selected];
            _trigger_copy = trigger.Clone();
            return trigger;
        }
        private void EndModifyTrigger(GameTrigger trigger, TrackWriter trk)
        {
            int selected = SelectedTrigger;
            if (selected == -1)
                throw new Exception(
                    "SelectedTrigger was removed during ModifyTrigger");

            _changemade = true;
            trigger.Start = (int)_spinnerStart.Value;
            trigger.End = (int)(_spinnerStart.Value + _spinnerDuration.Value);
            trk.Triggers[SelectedTrigger] = trigger;
            _editor.Timeline.TriggerChanged(_trigger_copy, trigger);
            UpdateFrame();
        }
        private void SetupZoom()
        {
            _zoomoptions = new ControlBase(null)
            {
                Margin = Margin.Zero,
                Dock = Dock.Fill
            };
            _zoomtarget = new Spinner(null)
            {
                Min = Constants.MinimumZoom,
                Max = Constants.MaxZoom,
                Value = _editor.Zoom,
            };
            _zoomtarget.ValueChanged += (o, e) =>
            {
                if (_selecting_trigger)
                    return;
                using (TrackWriter trk = _editor.CreateTrackWriter())
                {
                    GameTrigger trigger = BeginModifyTrigger(trk);
                    if (trigger != null)
                    {
                        trigger.ZoomTarget = (float)_zoomtarget.Value;
                        EndModifyTrigger(trigger, trk);
                    }
                }
            };
            GwenHelper.CreateLabeledControl(_zoomoptions, "Zoom Target:", _zoomtarget).Dock = Dock.Bottom;
        }
        private void SetupBG()
        {
            _bgoptions = new ControlBase(null)
            {
                Margin = Margin.Zero,
                Dock = Dock.Fill
            };

            _bgtraget = GwenHelper.CreateLabeledColorInput(_bgoptions, "Background", Settings.Colors.ExportBg, (o, color) =>
            {
                if (_selecting_trigger)
                    return;
                using (TrackWriter trk = _editor.CreateTrackWriter())
                {
                    GameTrigger trigger = BeginModifyTrigger(trk);
                    if (trigger != null)
                    {
                        trigger.backgroundRed = color.Value.R;
                        trigger.backgroundGreen = color.Value.G;
                        trigger.backgroundBlue = color.Value.B;
                        EndModifyTrigger(trigger, trk);
                    }
                }
            });

            _bgtraget.Dock = Dock.Bottom;
        }
        private void SetupLine()
        {
            _lineoptions = new ControlBase(null)
            {
                Margin = Margin.Zero,
                Dock = Dock.Fill
            };

            _linetarget = GwenHelper.CreateLabeledColorInput(_lineoptions, "Line", Settings.Colors.ExportLine, (o, color) =>
            {
                if (_selecting_trigger)
                    return;
                using (TrackWriter trk = _editor.CreateTrackWriter())
                {
                    GameTrigger trigger = BeginModifyTrigger(trk);
                    if (trigger != null)
                    {
                        trigger.lineRed = color.Value.R;
                        trigger.lineGreen = color.Value.G;
                        trigger.lineBlue = color.Value.B;
                        EndModifyTrigger(trigger, trk);
                    }
                }
            });

            _linetarget.Dock = Dock.Bottom;
        }
        private void SetupRight()
        {
            ControlBase rightcontainer = new ControlBase(this)
            {
                Margin = Margin.Zero,
                Dock = Dock.Right,
                AutoSizeToContents = true
            };
            ControlBase buttoncontainer = new ControlBase(rightcontainer)
            {
                Margin = Margin.Zero,
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                Children =
                {
                    new Button(null)
                    {
                        Text = "Cancel",
                        Name = "btncancel",
                        Dock = Dock.Right,
                        Margin = new Margin(5,0,0,0),
                        AutoSizeToContents = false,
                        Width = 60,
                    },
                    new Button(null)
                    {
                        Text = "Save",
                        Name = "btnsave",
                        Dock = Dock.Right,
                        AutoSizeToContents = false,
                        Width = 120,
                    }
                }
            };
            ControlBase save = buttoncontainer.FindChildByName("btnsave");
            ControlBase cancel = buttoncontainer.FindChildByName("btncancel");
            save.Clicked += (o, e) =>
            {
                FinishChange();
                _ = Close();
            };
            cancel.Clicked += (o, e) =>
            {
                CancelChange();
                _ = Close();
            };
            _triggeroptions = new Panel(rightcontainer)
            {
                Dock = Dock.Fill,
                Padding = Padding.Four,
                Margin = new Margin(0, 0, 0, 5)
            };

            SetupZoom();
            SetupBG();
            SetupLine();

            _zoomoptions.Parent = _triggeroptions;
            _triggertype = GwenHelper.CreateLabeledCombobox(
                rightcontainer, "Trigger Type:");
            _triggertype.Dock = Dock.Bottom;

            MenuItem zoom = _triggertype.AddItem("Zoom", "", TriggerType.Zoom);
            zoom.CheckChanged += (o, e) =>
            {
                _triggeroptions.Children.Clear();
                _zoomoptions.Parent = _triggeroptions;
                triggerSelected = TriggerType.Zoom;
                Debug.WriteLine("Changed to Zoom");

            };
            MenuItem bgColor = _triggertype.AddItem("BG Color", "", TriggerType.BGChange); // Tran
            bgColor.CheckChanged += (o, e) =>
            {
                _triggeroptions.Children.Clear();
                _bgoptions.Parent = _triggeroptions;
                triggerSelected = TriggerType.BGChange;
                Debug.WriteLine("Changed to Background");
            };
            MenuItem lineColor = _triggertype.AddItem("Line Color", "", TriggerType.LineColor); // Tran
            lineColor.CheckChanged += (o, e) =>
            {
                _triggeroptions.Children.Clear();
                _lineoptions.Parent = _triggeroptions;
                triggerSelected = TriggerType.LineColor;
                Debug.WriteLine("Changed to Line");
            };
            _triggertype.SelectedItem = zoom;
        }
        private void SetupLeft()
        {
            ControlBase leftcontainer = new ControlBase(this)
            {
                Margin = Margin.Zero,
                Dock = Dock.Left,
                AutoSizeToContents = true,
            };
            ControlBase panel = new ControlBase(leftcontainer)
            {
                Width = 150,
                Height = 200
            };
            ControlBase topcontainer = new ControlBase(panel)
            {
                Margin = new Margin(0, 3, 0, 3),
                Padding = Padding.Five,
                Dock = Dock.Top,

                Children =
                {
                    new Button(null)
                    {
                        Text = "-",
                        Name = "btndelete",
                        Dock = Dock.Right,
                        Margin = new Margin(2,0,2,0),
                        Height = 16,
                        Width = 16,
                        AutoSizeToContents = false
                    },
                    new Button(null)
                    {
                        Text = "+",
                        Name = "btnadd",
                        Dock = Dock.Right,
                        Margin = new Margin(2,0,2,0),
                        Width = 16,
                        Height = 16,
                        AutoSizeToContents = false
                    }
                },
                AutoSizeToContents = true,

            };
            ControlBase add = topcontainer.FindChildByName("btnadd");
            ControlBase delete = topcontainer.FindChildByName("btndelete");
            add.Clicked += (o, e) =>
            {
                GameTrigger trigger = null;
                switch (triggerSelected)
                {
                    case TriggerType.Zoom:
                        _triggeroptions.Children.Clear();
                        _zoomoptions.Parent = _triggeroptions;
                        trigger = new GameTrigger()
                        {
                            TriggerType = TriggerType.Zoom,
                            Start = _editor.Offset,
                            End = _editor.Offset + Settings.DefaultTriggerLength,
                            ZoomTarget = 4,
                        };

                        break;
                    case TriggerType.BGChange:
                        trigger = new GameTrigger()
                        {
                            TriggerType = TriggerType.BGChange,
                            Start = _editor.Offset,
                            End = _editor.Offset + Settings.DefaultTriggerLength,
                            backgroundRed = Settings.Colors.ExportBg.R,
                            backgroundGreen = Settings.Colors.ExportBg.G,
                            backgroundBlue = Settings.Colors.ExportBg.B,
                        };
                        break;
                    case TriggerType.LineColor:
                        trigger = new GameTrigger()
                        {
                            TriggerType = TriggerType.LineColor,
                            Start = _editor.Offset,
                            End = _editor.Offset + Settings.DefaultTriggerLength,
                            lineRed = Settings.Colors.ExportLine.R,
                            lineGreen = Settings.Colors.ExportLine.G,
                            lineBlue = Settings.Colors.ExportLine.B,
                        };
                        break;
                    default: // Default to the zoom trigger if something goes wrong
                        trigger = new GameTrigger()
                        {
                            TriggerType = TriggerType.Zoom,
                            Start = _editor.Offset,
                            End = _editor.Offset + Settings.DefaultTriggerLength,
                            ZoomTarget = 4,
                        };
                        break;
                }

                _changemade = true;
                using (TrackWriter trk = _editor.CreateTrackWriter())
                {
                    trk.Triggers.Add(trigger);
                    _editor.Timeline.TriggerChanged(trigger, trigger);
                    UpdateFrame();
                }
                ToggleDisable(false);
                _ = _lbtriggers.AddRow(GetTriggerLabel(trigger), "", trigger);
                _lbtriggers.SelectByUserData(trigger);
            };
            delete.Clicked += (o, e) =>
            {
                ListBoxRow row = _lbtriggers.SelectedRow;
                if (row != null)
                {
                    GameTrigger trigger = (GameTrigger)row.UserData;
                    _changemade = true;
                    using (TrackWriter trk = _editor.CreateTrackWriter())
                    {
                        try
                        {
                            trk.Triggers.RemoveAt(SelectedTrigger);
                            _editor.Timeline.TriggerChanged(trigger, trigger);
                        }
                        catch { /*Do nothing*/ }
                        UpdateFrame();
                    }
                    _ = _lbtriggers.Children.Remove(row);
                    ToggleDisable(true);
                }
            };
            _lbtriggers = new ListBox(panel)
            {
                Dock = Dock.Fill,
                Margin = new Margin(0, 0, 0, 5)
            };
            ControlBase spinnerContainer = new ControlBase(leftcontainer)
            {
                Margin = Margin.Zero,
                Padding = new Padding(0, 0, 50, 0),
                Dock = Dock.Bottom,
                AutoSizeToContents = true
            };
            _spinnerStart = new Spinner(null)
            {
                OnlyWholeNumbers = true,
                Min = 1,
                Max = int.MaxValue - 1,
                Value = _editor.Offset
                // TODO set values
            };
            _spinnerDuration = new Spinner(null)
            {
                OnlyWholeNumbers = true,
                Min = 0,
                Max = int.MaxValue - 1, // Yeah I'm crazy for wanting a more than 2 min trigger
                Value = 0
                // TODO set values
            };
            _spinnerStart.ValueChanged += OnTriggerTimeChanged;
            _spinnerDuration.ValueChanged += OnTriggerTimeChanged;
            GwenHelper.CreateLabeledControl(
                spinnerContainer,
                "Duration:  ",
                _spinnerDuration).Dock = Dock.Bottom;
            GwenHelper.CreateLabeledControl(
                spinnerContainer,
                "Start:",
                _spinnerStart).Dock = Dock.Bottom;

            _lbtriggers.RowSelected += OnTriggerSelected;
        }
        private void OnTriggerTimeChanged(object o, EventArgs e)
        {
            if (_selecting_trigger)
                return;
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                GameTrigger trigger = BeginModifyTrigger(trk);
                if (trigger != null)
                {
                    EndModifyTrigger(trigger, trk);
                    ListBoxRow selected = _lbtriggers.SelectedRow;
                    selected.Text = GetTriggerLabel(trigger);
                    selected.UserData = trigger;
                }
                UpdateFrame();
            }
        }
        private void ToggleDisable(bool disabled)
        {
            _spinnerStart.IsDisabled = disabled;
            _spinnerDuration.IsDisabled = disabled;
            _slider.IsDisabled = disabled;
        }
        private bool _selecting_trigger = false;
        private void OnTriggerSelected(object o, ItemSelectedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                ToggleDisable(true);
            }
            else
            {
                ToggleDisable(false);
            }
            GameTrigger trigger = (GameTrigger)e.SelectedItem.UserData;
            try
            {
                _selecting_trigger = true;
                _spinnerStart.Value = trigger.Start;
                _spinnerDuration.Value = trigger.End - trigger.Start;
                _triggertype.SelectByUserData(trigger.TriggerType);
                switch (trigger.TriggerType)
                {
                    case TriggerType.Zoom:
                    {
                        _triggeroptions.Children.Clear();
                        _zoomoptions.Parent = _triggeroptions;
                        Debug.WriteLine("Changed to Zoom");

                        _zoomtarget.Value = trigger.ZoomTarget;
                        triggerSelected = TriggerType.Zoom;
                        break;
                    }
                    case TriggerType.BGChange:
                    {
                        _triggeroptions.Children.Clear();
                        _bgoptions.Parent = _triggeroptions;
                        Debug.WriteLine("Changed to Background");

                        // (!) Temporary solution, will be remade later
                        TextBox bgField = (TextBox)_bgtraget.FindChildByName("input");
                        GwenHelper.ColorIndicator bgPreview = (GwenHelper.ColorIndicator)_bgtraget.FindChildByName("preview");
                        Color color = Color.FromArgb(trigger.backgroundRed, trigger.backgroundGreen, trigger.backgroundBlue);

                        bgField.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}";
                        bgPreview.Color = color;

                        triggerSelected = TriggerType.BGChange;
                        break;
                    }
                    case TriggerType.LineColor:
                    {
                        _triggeroptions.Children.Clear();
                        _lineoptions.Parent = _triggeroptions;
                        Debug.WriteLine("Changed to Line");

                        // (!) Temporary solution, will be remade later
                        TextBox lineField = (TextBox)_linetarget.FindChildByName("input");
                        GwenHelper.ColorIndicator linePreview = (GwenHelper.ColorIndicator)_bgtraget.FindChildByName("preview");
                        Color color = Color.FromArgb(trigger.lineRed, trigger.lineGreen, trigger.lineBlue);

                        lineField.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}";
                        linePreview.Color = color;
                        triggerSelected = TriggerType.LineColor;
                        break;
                    }
                    /* Add more triggers here */
                    default:
                        break;
                }
            }
            finally
            {
                _selecting_trigger = false;
            }
            UpdateFrame();
        }
        private void OnSliderValueChanged(object o, EventArgs e) => UpdateFrame();
        private void Setup()
        {
            ControlBase bottomcontainer = new ControlBase(this)
            {
                Margin = Margin.Zero,
                Dock = Dock.Bottom,
                AutoSizeToContents = true
            };
            _slider = new HorizontalSlider(bottomcontainer)
            {
                Dock = Dock.Bottom,
                Max = 1,
                Value = 0
            };
            _slider.ValueChanged += OnSliderValueChanged;
            SetupLeft();
            SetupRight();
            Populate();
        }
        private void Populate()
        {
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                foreach (GameTrigger trigger in trk.Triggers)
                {
                    _ = _lbtriggers.AddRow(
                        GetTriggerLabel(trigger), string.Empty, trigger);
                }
            }
        }
        private string GetTriggerLabel(GameTrigger trigger)
        {
            string typelabel;
            switch (trigger.TriggerType)
            {
                case TriggerType.Zoom:
                    typelabel = "[Zoom]";
                    break;
                case TriggerType.BGChange:
                    typelabel = "[BG Color]";
                    break;
                case TriggerType.LineColor:
                    typelabel = "[Line Color]";
                    break;
                default:
                    typelabel = "[No Name Found]";
                    break;
            }
            return $"{typelabel} {trigger.Start} - {trigger.End}";
        }

        private void UpdateFrame()
        {
            double start = Math.Max(_spinnerStart.Value - FramePadding, 0);
            double end = _spinnerStart.Value + _spinnerDuration.Value + FramePadding;
            int frame = (int)(start + (end - start) * _slider.Value);
            _editor.UseUserZoom = false;
            _editor.SetFrame(frame);
            _editor.UpdateCamera();
        }
        private void CancelChange()
        {
            if (_changemade)
            {
                using (TrackWriter trk = _editor.CreateTrackWriter())
                {
                    List<GameTrigger> toadd = new List<GameTrigger>();

                    foreach (GameTrigger oldtrigger in _triggers_copy)
                    {
                        bool match = false;
                        foreach (GameTrigger newtrigger in trk.Triggers)
                        {
                            if (oldtrigger.CompareTo(newtrigger))
                            {
                                match = true;
                            }
                        }
                        if (!match)
                        {
                            toadd.Add(oldtrigger);
                        }
                    }

                    foreach (GameTrigger trigger in toadd)
                    {
                        trk.Triggers.Add(trigger);
                        _editor.Timeline.TriggerChanged(trigger, trigger);
                        UpdateFrame();
                    }

                    List<GameTrigger> toremove = new List<GameTrigger>();

                    foreach (GameTrigger newtrigger in trk.Triggers)
                    {
                        bool match = false;
                        foreach (GameTrigger oldtrigger in _triggers_copy)
                        {
                            if (oldtrigger.CompareTo(newtrigger))
                            {
                                match = true;
                            }
                        }
                        if (!match)
                        {
                            toremove.Add(newtrigger);
                        }
                    }

                    foreach (GameTrigger trigger in toremove)
                    {
                        _ = trk.Triggers.Remove(trigger);
                        _editor.Timeline.TriggerChanged(trigger, trigger);
                        UpdateFrame();
                    }
                }
                _changemade = false;
            }
        }
        private void FinishChange()
        {
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                List<GameTrigger> triggers = trk.Triggers;
                if (_changemade)
                {
                    _changemade = false;
                }
            }
        }
    }
}
