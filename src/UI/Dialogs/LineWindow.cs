using Gwen;
using Gwen.Controls;
using linerider.Game;
using linerider.Utils;
using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Linq;

namespace linerider.UI
{
    public class LineWindow : DialogBase
    {
        private readonly PropertyTree _proptree;
        private GameLine _ownerline;
        private readonly GameLine _linecopy;
        private bool _linechangemade = false;
        private const string DefaultTitle = "Line Properties";
        private bool closing = false;

        private NumberProperty length;
        private NumberProperty angleProp;

        private NumberProperty multiplier;
        private NumberProperty multilines;
        private CheckProperty accelinverse;

        private CheckProperty leftAGW;
        private CheckProperty rightAGW;
        public LineWindow(GameCanvas parent, Editor editor, GameLine line) : base(parent, editor)
        {
            _ownerline = line;
            _linecopy = _ownerline.Clone();
            Title = DefaultTitle;
            Padding = Padding.Zero;
            AutoSizeToContents = true;
            _proptree = new PropertyTree(this)
            {
                Width = 250,
                Height = 250,
                Dock = Dock.Top
            };
            MakeModal(true);
            Setup();
            _proptree.ExpandAll();
        }
        protected override void CloseButtonPressed(ControlBase control, EventArgs args)
        {
            if (closing || !_linechangemade)
            {
                closing = true;
                base.CloseButtonPressed(control, args);
            }
            else
            {
                WarnClose();
            }
        }
        public override bool Close()
        {
            if (closing || !_linechangemade)
            {
                closing = true;
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
                        closing = true;
                        _ = base.Close();
                        break;
                    case DialogResult.No:
                        CancelChange();
                        closing = true;
                        _ = base.Close();
                        break;
                }
            };
        }
        private void Setup()
        {
            SetupLineRightClickOptions(_proptree);

            Panel bottom = new Panel(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                ShouldDrawBackground = false,
                Margin = new Margin(0, 5, 0, 0),
            };
            Button cancel = new Button(bottom)
            {
                Text = "Cancel",
                Dock = Dock.Right,
            };
            cancel.Clicked += (o, e) =>
            {
                CancelChange();
                closing = true;
                _ = Close();
            };
            Button ok = new Button(bottom)
            {
                Text = "Okay",
                Dock = Dock.Right,
                Margin = new Margin(0, 0, 5, 0)
            };
            ok.Clicked += (o, e) =>
            {
                FinishChange();
                closing = true;
                _ = Close();
            };
        }

        private void SetupSceneryOptions(PropertyTree tree, PropertyTable lineProp)
        {
            NumberProperty width = new NumberProperty(lineProp)
            {
                Min = 0.1,
                Max = 25.5,
                NumberValue = _ownerline.Width,
            };
            width.ValueChanged += (o, e) =>
            {
                ChangeWidth(width.NumberValue);
            };
            _ = lineProp.Add("Width", width);
        }

        private void SetupBlueAndRedOptions(PropertyTree tree)
        {
            double currentMultiplier = 0;
            bool inv = false;
            StandardLine.Ext lineEXT = ((StandardLine)_ownerline).Extension;
            bool leftEXT = ((StandardLine)_ownerline).Extension == StandardLine.Ext.Both || ((StandardLine)_ownerline).Extension == StandardLine.Ext.Left;
            bool rightEXT = ((StandardLine)_ownerline).Extension == StandardLine.Ext.Both || ((StandardLine)_ownerline).Extension == StandardLine.Ext.Right;

            if (_ownerline is RedLine red)
            {
                currentMultiplier = red.Multiplier;
                inv = red.inv;
            }

            PropertyTable table = tree.Add("Acceleration", 120);
            multiplier = new NumberProperty(table)
            {
                Min = -9999,
                Max = 9999,
                NumberValue = inv ? -currentMultiplier : currentMultiplier,
            };
            multiplier.ValueChanged += (o, e) =>
            {
                accelinverse.IsChecked = multiplier.NumberValue < 0;
                ChangeMultiplier(Math.Abs(multiplier.NumberValue));
            };
            _ = table.Add("Multiplier", multiplier);
            multilines = new NumberProperty(table)
            {
                Min = 1,
                Max = 9999,
                OnlyWholeNumbers = true,
                NumberValue = GetMultiLines(true).Count
            };
            multilines.ValueChanged += (o, e) =>
            {
                Multiline((int)multilines.NumberValue);
            };
            _ = table.Add("Multilines", multilines);

            accelinverse = GwenHelper.AddPropertyCheckbox(
                table,
                "Inverse",
                inv
                );
            accelinverse.ValueChanged += (o, e) =>
            {
                if (accelinverse.IsChecked)
                {
                    if (multiplier.NumberValue > 0)
                    {
                        multiplier.NumberValue = -multiplier.NumberValue;
                    }
                }
                else
                {
                    multiplier.NumberValue = Math.Abs(multiplier.NumberValue);
                }

                using (TrackWriter trk = _editor.CreateTrackWriter())
                {
                    SimulationCell multi = GetMultiLines(false);
                    foreach (StandardLine l in multi)
                    {
                        StandardLine cpy = (StandardLine)l.Clone();
                        cpy.Position1 = l.Position2;
                        cpy.Position2 = l.Position1;
                        cpy.inv = accelinverse.IsChecked;
                        UpdateLine(trk, l, cpy);
                    }

                    StandardLine owner = (StandardLine)_ownerline.Clone();
                    owner.Position1 = _ownerline.Position2;
                    owner.Position2 = _ownerline.Position1;
                    owner.inv = accelinverse.IsChecked;
                    UpdateOwnerLine(trk, owner);
                }

                Vector2d vec = _ownerline.GetVector();
                Angle angle = Angle.FromVector(vec);
                angle.Degrees += 90;
                angleProp.NumberValue = angle.Degrees;
            };

            multiplier.ValueChanged += (o, e) =>
            {
                int val = (int)multiplier.NumberValue;
                if (val == 0)
                {
                    accelinverse.Disable();
                }
                else
                {
                    accelinverse.Enable();
                }
            };

            if ((int)multiplier.NumberValue == 0)
            {
                accelinverse.Disable();
            }

            table = tree.Add("Line Extensions (AGWs)", 120);
            leftAGW = GwenHelper.AddPropertyCheckbox(table, "Starting Extension", leftEXT);
            leftAGW.ValueChanged += (o, e) =>
            {
                UpdateAGWs(leftAGW.IsChecked, rightAGW.IsChecked);
            };
            rightAGW = GwenHelper.AddPropertyCheckbox(table, "Ending Extension", rightEXT);
            rightAGW.ValueChanged += (o, e) =>
            {
                UpdateAGWs(leftAGW.IsChecked, rightAGW.IsChecked);
            };
        }
        private void SetupLineRightClickOptions(PropertyTree tree)
        {
            Vector2d vec = _ownerline.GetVector();
            double len = vec.Length;
            Angle angle = Angle.FromVector(vec);
            angle.Degrees += 90;

            PropertyTable lineProp = tree.Add("Line Properties", 120);

            if (_ownerline.Type != LineType.Scenery)
            {
                NumberProperty id = new NumberProperty(lineProp)
                {
                    Min = int.MinValue + 1,
                    Max = int.MaxValue - 1,
                    NumberValue = _ownerline.ID,
                    OnlyWholeNumbers = true,
                    IsDisabled = true
                };
                id.ValueChanged += (o, e) =>
                {
                    ChangeID((int)id.NumberValue); // TODO: Add the ability to change line IDs
                };
                _ = lineProp.Add("ID", id);
            }

            length = new NumberProperty(lineProp)
            {
                Min = 0.0000001,
                Max = double.MaxValue - 1,
                NumberValue = len,
            };
            length.ValueChanged += (o, e) =>
            {
                ChangeLength(length.NumberValue);
            };
            _ = lineProp.Add("Length", length);

            angleProp = new NumberProperty(lineProp)
            {
                Min = double.MinValue,
                Max = double.MaxValue,
                NumberValue = angle.Degrees,
            };
            angleProp.ValueChanged += (o, e) =>
            {
                ChangeAngle(angleProp.NumberValue);
            };
            _ = lineProp.Add("Angle", angleProp);

            if (_ownerline.Type != LineType.Scenery)
            {
                SetupBlueAndRedOptions(tree);
            }
            else
            {
                SetupSceneryOptions(tree, lineProp);
            }
        }
        private void UpdateOwnerLine(TrackWriter trk, GameLine replacement)
        {
            UpdateLine(trk, _ownerline, replacement);
            _ownerline = replacement;
        }
        private void UpdateLine(TrackWriter trk, GameLine current, GameLine replacement)
        {
            MakingChange();

            if (replacement is StandardLine stl)
            {
                stl.CalculateConstants();
            }
            trk.ReplaceLine(current, replacement);
            _editor.NotifyTrackChanged();
            _editor.Invalidate();
        }
        private void ChangeID(int newID)
        {

        }
        private void ChangeAngle(double numberValue)
        {
            SimulationCell multilines = GetMultiLines(false);
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                GameLine cpy = _ownerline.Clone();
                bool lineIsInverted = cpy.Start == cpy.Position2;
                Angle angle = Angle.FromDegrees(numberValue - 90);

                Vector2d posEstimate = Utility.Rotate(cpy.End, cpy.Start, angle - Angle.FromVector(cpy.GetVector()));
                Vector2d newPos = Utility.AngleLock(cpy.Start, posEstimate, angle);
                if (lineIsInverted) // Inverted line
                {
                    cpy.Position1 = newPos;
                }
                else
                {
                    cpy.Position2 = newPos;
                }

                UpdateOwnerLine(trk, cpy);

                foreach (StandardLine line in multilines)
                {
                    GameLine copy = line.Clone();
                    copy.Position1 = cpy.Position1;
                    copy.Position2 = cpy.Position2;
                    UpdateLine(trk, line, copy);
                }
            }
        }
        private void ChangeLength(double length)
        {
            SimulationCell multilines = GetMultiLines(false);
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                GameLine cpy = _ownerline.Clone();
                double angle = Angle.FromVector(cpy.GetVector()).Radians;

                double x2 = cpy.Position1.X + length * Math.Cos(angle);
                double y2 = cpy.Position1.Y + length * Math.Sin(angle);

                Vector2d newPos = new Vector2d(x2, y2);
                cpy.Position2 = newPos;
                UpdateOwnerLine(trk, cpy);

                foreach (StandardLine line in multilines)
                {
                    GameLine copy = line.Clone();
                    copy.Position2 = newPos;
                    UpdateLine(trk, line, copy);
                }
            }
        }
        private void ChangeWidth(double width)
        {
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                GameLine cpy = _ownerline.Clone();
                cpy.Width = (float)width;
                UpdateOwnerLine(trk, cpy);
            }
        }
        private void ChangeMultiplier(double mul)
        {
            SimulationCell lines = GetMultiLines(false);
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                RedLine redCpy = null;
                StandardLine blueCpy = null;
                LineType origLineType = _ownerline.Type;

                // If adding acceleration to a blue line
                if (origLineType == LineType.Standard && mul != 0)
                {
                    redCpy = RedLine.CloneFromBlue((StandardLine)_ownerline);
                    _editor._renderer.RedrawLine(_ownerline);
                    _editor._renderer.AddLine(redCpy);
                }
                // If setting acceleration to 0 of a red line
                else if (origLineType == LineType.Acceleration && mul == 0)
                {
                    _editor._renderer.RemoveLine(_ownerline);
                    blueCpy = StandardLine.CloneFromRed((RedLine)_ownerline);
                    _editor._renderer.AddLine(blueCpy);
                }
                else
                {
                    redCpy = (RedLine)_ownerline.Clone();
                }

                if (redCpy != null)
                {
                    redCpy.Multiplier = mul;
                }
                StandardLine cpy = redCpy ?? blueCpy;
                UpdateOwnerLine(trk, cpy);
                foreach (StandardLine line in lines)
                {
                    StandardLine copy = origLineType == LineType.Acceleration && _ownerline.Type == LineType.Standard
                        ? StandardLine.CloneFromRed(line)
                        : origLineType == LineType.Standard && _ownerline.Type == LineType.Acceleration
                            ? RedLine.CloneFromBlue(line)
                            : (StandardLine)line.Clone();
                    // Going from red lines to blue
                    if (copy is RedLine redCopy)
                    {
                        redCopy.Multiplier = mul;
                    }
                    UpdateLine(trk, line, copy);
                }
            }
        }

        private void UpdateAGWs(bool left, bool right)
        {
            StandardLine.Ext newExt = (StandardLine.Ext)((left ? 1 : 0) + (right ? 2 : 0));

            SimulationCell multilines = GetMultiLines(false);
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                trk.DisableExtensionUpdating();

                StandardLine cpy = (StandardLine)_ownerline.Clone();
                cpy.Extension = newExt;
                UpdateOwnerLine(trk, cpy);

                foreach (StandardLine line in multilines)
                {
                    StandardLine copy = (StandardLine)line.Clone();
                    copy.Extension = newExt;
                    UpdateLine(trk, line, copy);
                }
            }
        }
        private SimulationCell GetMultiLines(bool includeowner)
        {
            if (_ownerline.Type == LineType.Scenery)
            {
                return new SimulationCell();
            }

            SimulationCell redlines = new SimulationCell();
            using (TrackReader trk = _editor.CreateTrackReader())
            {
                StandardLine owner = (StandardLine)_ownerline;
                System.Collections.Generic.IEnumerable<GameLine> lines = trk.GetLinesInRect(new DoubleRect(owner.Position1, new Vector2d(1, 1)), false);
                foreach (GameLine red in lines)
                {
                    if (
                        red is StandardLine stl &&
                        red.Position1 == owner.Position1 &&
                        red.Position2 == owner.Position2 &&
                        (includeowner || red.ID != owner.ID))
                    {
                        redlines.AddLine(stl);
                    }
                }
            }
            return redlines;
        }
        private void Multiline(int count)
        {
            SimulationCell redlines = GetMultiLines(false);
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                StandardLine owner = (StandardLine)_ownerline;
                MakingChange();
                // Owner line doesn't count, but our min bounds is 1
                int diff = count - 1 - redlines.Count;
                if (diff < 0)
                {
                    for (int i = 0; i > diff; i--)
                    {
                        trk.RemoveLine(redlines.First());
                        redlines.RemoveLine(redlines.First().ID);
                    }
                }
                else if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        StandardLine newLine = owner is RedLine line
                            ? new RedLine(owner.Position1, owner.Position2, owner.inv) { Multiplier = line.Multiplier }
                            : new StandardLine(owner.Position1, owner.Position2, owner.inv);
                        newLine.CalculateConstants();
                        trk.AddLine(newLine);
                    }
                }
            }
            _editor.NotifyTrackChanged();
        }
        private void MakingChange()
        {
            if (!_linechangemade)
            {
                _editor.UndoManager.BeginAction();
                _linechangemade = true;
                Title = DefaultTitle + " *";
            }
        }
        private void CancelChange()
        {
            if (_linechangemade)
            {
                _editor.UndoManager.CancelAction();
                _linechangemade = false;
            }
        }
        private void FinishChange()
        {
            if (_linechangemade)
            {
                _editor.UndoManager.EndAction();
                _linechangemade = false;
            }
        }
    }
}
