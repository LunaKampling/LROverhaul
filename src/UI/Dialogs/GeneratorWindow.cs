using Gwen;
using Gwen.Controls;
using linerider.Game.LineGenerator;
using OpenTK;
using System;

namespace linerider.UI
{
    public class GeneratorWindow : DialogBase
    {
        private ControlBase _focus;
        //private int _tabscount = 0;
        private bool GeneratePressed = false;
        private static bool initialised = false;
        private static GeneratorType _currentgen;

        private ComboBox GeneratorTypeBox;
        private Panel GeneratorOptions;

        private ControlBase CircleGenOptions;
        private Spinner CircleRadius;
        private Spinner CircleLineCount;
        private Spinner CircleOffsetX;
        private Spinner CircleOffsetY;
        private Spinner CircleMultiplier;
        private Spinner CircleWidth;
        private Checkbox CircleInverse;
        private Checkbox CircleReverse;

        private ControlBase TenPCOptions;
        private Spinner TenPCX;
        private Spinner TenPCY;
        private Spinner TenPCRotation;
        private ControlBase LeftColumn;
        private ControlBase RightColumn;
        private Checkbox[] TenPCPointSelect;

        private ControlBase LineGenOptions;
        private SpinnerG17 LineX1;
        private SpinnerG17 LineY1;
        private SpinnerG17 LineX2;
        private SpinnerG17 LineY2;
        private Spinner LineMultiplier;
        private Spinner LineWidth;
        private Checkbox LineInverse;
        private Checkbox LineReverse;

        private ControlBase KramualGenOptions;
        private SpinnerG17 KramualX;
        private SpinnerG17 KramualY;
        private Spinner KramualFrame;
        private Spinner KramualIteration;
        private Spinner KramualMultiplier;
        private Checkbox KramualInverse;
        private Checkbox KramualFrameOverride;
        private Checkbox KramualIterationOverride;
        private Checkbox KramualPositionOverride;
        private Checkbox KramualReverse;

        private static CircleGenerator gen_Circle;
        private static TenPCGenerator gen_10pc;
        private static LineGenerator gen_Line;
        private static KramualGenerator gen_Kramual;

        private GeneratorType CurrentGenerator
        {
            get => _currentgen;
            set
            {
                Render_Clear();
                _currentgen = value;
                Render_Preview();
            }
        }

        public GeneratorWindow(GameCanvas parent, Vector2d pos) : base(parent, null)
        {
            Title = $"Line Generator";
            _ = SetSize(450, 500);
            DisableResizing();

            /*_typecontainer = new CollapsibleList(this)
            {
                Dock = Dock.Left,
                AutoSizeToContents = false,
                Width = 100,
                Margin = new Margin(0, 0, 5, 0)
            };*/

            MakeModal(true);

            if (!initialised)
            {
                gen_Circle = new CircleGenerator("Circle Generator", 10.0, pos, 50, false);
                gen_10pc = new TenPCGenerator("10PC Generator", new Vector2d(1.0, 1.0), 0.0);
                gen_Line = new LineGenerator("Line Generator", new Vector2d(0, 0), new Vector2d(1, 1), LineType.Acceleration);
                gen_Kramual = new KramualGenerator("Kramual Generator", new Vector2d(0, 0), LineType.Standard);
                CurrentGenerator = GeneratorType.Circle;
            }

            Setup();

            initialised = true;
        }

        protected override bool OnKeyEscape(bool down)
        {
            gen_Circle.DeleteLines();
            Console.WriteLine("ESCAPE!");
            _ = Close();
            return true;
        }
        protected override bool OnKeyReturn(bool down)
        {
            if (down)
            {
                gen_Circle.DeleteLines();
                Console.WriteLine("ESCAPE!");
                _ = Close();
            }
            return true;
        }
        private void Setup()
        {
            ControlBase top = new ControlBase(this)
            {
                Dock = Dock.Top,
                AutoSizeToContents = true,
                Margin = new Margin(0, 0, 0, 5),
            };

            ControlBase panel = new ControlBase(this)
            {
                Margin = Margin.Zero,
                Dock = Dock.Fill,
                AutoSizeToContents = true
            };

            ControlBase bottom = new ControlBase(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                Margin = new Margin(0, 5, 0, 0),
            };

            GeneratorOptions = new Panel(panel)
            {
                Dock = Dock.Fill,
                Padding = Padding.Five,
            };

            Button generate = new Button(bottom)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Generate"
            };
            generate.Clicked += (o, e) =>
            {
                Render_Final();
                GeneratePressed = true;
                _ = Close();
            };

            PopulateCircle();
            Populate10pc();
            PopulateLine();
            PopulateKramual();

            GeneratorTypeBox = GwenHelper.CreateLabeledCombobox(top, "Generator Type:");
            GeneratorTypeBox.Dock = Dock.Top;
            MenuItem kramual = GeneratorTypeBox.AddItem("Kramual", "", GeneratorType.Kramual);
            kramual.CheckChanged += (o, e) =>
            {
                GeneratorOptions.Children.Clear();
                KramualGenOptions.Parent = GeneratorOptions;
                CurrentGenerator = GeneratorType.Kramual;
            };
            MenuItem line = GeneratorTypeBox.AddItem("Line", "", GeneratorType.Line);
            line.CheckChanged += (o, e) =>
            {
                GeneratorOptions.Children.Clear();
                LineGenOptions.Parent = GeneratorOptions;
                CurrentGenerator = GeneratorType.Line;
            };
            MenuItem tenpc = GeneratorTypeBox.AddItem("10-Point Cannon", "", GeneratorType.TenPC);
            tenpc.CheckChanged += (o, e) =>
            {
                GeneratorOptions.Children.Clear();
                TenPCOptions.Parent = GeneratorOptions;
                CurrentGenerator = GeneratorType.TenPC;
            };
            MenuItem circle = GeneratorTypeBox.AddItem("Circle", "", GeneratorType.Circle);
            circle.CheckChanged += (o, e) =>
            {
                GeneratorOptions.Children.Clear();
                CircleGenOptions.Parent = GeneratorOptions;
                CurrentGenerator = GeneratorType.Circle;
            };

            GeneratorOptions.Children.Clear();

            switch (CurrentGenerator)
            {
                default:
                    GeneratorTypeBox.SelectedItem = circle;
                    CircleGenOptions.Parent = GeneratorOptions;
                    break;
                case GeneratorType.Circle:
                    GeneratorTypeBox.SelectedItem = circle;
                    CircleGenOptions.Parent = GeneratorOptions;
                    break;
                case GeneratorType.TenPC:
                    GeneratorTypeBox.SelectedItem = tenpc;
                    TenPCOptions.Parent = GeneratorOptions;
                    break;
                case GeneratorType.Line:
                    GeneratorTypeBox.SelectedItem = line;
                    LineGenOptions.Parent = GeneratorOptions;
                    break;
                case GeneratorType.Kramual:
                    GeneratorTypeBox.SelectedItem = kramual;
                    KramualGenOptions.Parent = GeneratorOptions;
                    break;
            }

            Render_Preview();
        }

        private void PopulateCircle()
        {
            CircleGenOptions = new ControlBase(null)
            {
                Margin = Margin.Zero,
                Dock = Dock.Top,
                AutoSizeToContents = true
            };
            //Panel configPanel = GwenHelper.CreateHeaderPanel(CircleGenOptions, "Configure Circle Properties");
            CircleRadius = new Spinner(null)
            {
                Min = 0.0,
                Max = 1.0e9,
                Value = gen_Circle.radius
            };
            CircleRadius.ValueChanged += (o, e) =>
            {
                gen_Circle.radius = CircleRadius.Value;
                gen_Circle.ReGenerate_Preview();
            };

            CircleLineCount = new Spinner(null)
            {
                Min = 3.0,
                Max = 1.0e7,
                Value = gen_Circle.lineCount
            };
            CircleLineCount.ValueChanged += (o, e) =>
            {
                gen_Circle.lineCount = (int)CircleLineCount.Value;
                gen_Circle.ReGenerate_Preview();
            };

            CircleOffsetX = new Spinner(null)
            {
                Min = -30000000000,
                Max = 30000000000,
                Value = gen_Circle.position.X
            };
            CircleOffsetX.ValueChanged += (o, e) =>
            {
                gen_Circle.position.X = CircleOffsetX.Value;
                gen_Circle.ReGenerate_Preview();
            };
            CircleOffsetY = new Spinner(null)
            {
                Min = -30000000000,
                Max = 30000000000,
                Value = gen_Circle.position.Y
            };
            CircleOffsetY.ValueChanged += (o, e) =>
            {
                gen_Circle.position.Y = CircleOffsetY.Value;
                gen_Circle.ReGenerate_Preview();
            };

            CircleMultiplier = new Spinner(null)
            {
                Min = 0,
                Max = 255,
                Value = gen_Circle.multiplier,
                IsDisabled = gen_Circle.lineType != LineType.Acceleration
            };
            CircleMultiplier.ValueChanged += (o, e) =>
            {
                gen_Circle.multiplier = (int)CircleMultiplier.Value;
                gen_Circle.ReGenerate_Preview();
            };

            CircleWidth = new Spinner(null)
            {
                Min = 0.1,
                Max = 25.5,
                Value = gen_Circle.width,
                IncrementSize = 0.1,
                IsDisabled = gen_Circle.lineType != LineType.Scenery
            };
            CircleWidth.ValueChanged += (o, e) =>
            {
                gen_Circle.width = (float)CircleWidth.Value;
                gen_Circle.ReGenerate_Preview();
            };

            _ = GwenHelper.CreateLabeledControl(CircleGenOptions, "Radius", CircleRadius);
            _ = GwenHelper.CreateLabeledControl(CircleGenOptions, "Line Count", CircleLineCount);
            _ = GwenHelper.CreateLabeledControl(CircleGenOptions, "Centre X", CircleOffsetX);
            _ = GwenHelper.CreateLabeledControl(CircleGenOptions, "Centre Y", CircleOffsetY);
            _ = GwenHelper.CreateLabeledControl(CircleGenOptions, "Acceleration Multiplier", CircleMultiplier);
            _ = GwenHelper.CreateLabeledControl(CircleGenOptions, "Scenery Width Multiplier", CircleWidth);

            RadioButtonGroup lineTypeRadioGroup = new RadioButtonGroup(CircleGenOptions)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false
            };
            RadioButton blueType = lineTypeRadioGroup.AddOption("Blue");
            RadioButton redType = lineTypeRadioGroup.AddOption("Red");
            RadioButton greenType = lineTypeRadioGroup.AddOption("Green");
            switch (gen_Circle.lineType)
            {
                case LineType.Standard:
                    blueType.Select();
                    break;
                case LineType.Acceleration:
                    redType.Select();
                    break;
                case LineType.Scenery:
                    greenType.Select();
                    break;
                default:
                    break;
            }
            blueType.CheckChanged += (o, e) =>
            {
                gen_Circle.lineType = LineType.Standard;
                gen_Circle.ReGenerate_Preview();
                CircleMultiplier.Disable();
                CircleWidth.Disable();
                CircleInverse.Enable();
                CircleReverse.Disable();
            };
            redType.CheckChanged += (o, e) =>
            {
                gen_Circle.lineType = LineType.Acceleration;
                gen_Circle.ReGenerate_Preview();
                CircleMultiplier.Enable();
                CircleWidth.Disable();
                CircleInverse.Enable();
                CircleReverse.Enable();
            };
            greenType.CheckChanged += (o, e) =>
            {
                gen_Circle.lineType = LineType.Scenery;
                gen_Circle.ReGenerate_Preview();
                CircleMultiplier.Disable();
                CircleWidth.Enable();
                CircleInverse.Disable();
                CircleReverse.Disable();
            };

            CircleInverse = GwenHelper.AddCheckbox(CircleGenOptions, "Invert", gen_Circle.invert, (o, e) =>
            {
                gen_Circle.invert = ((Checkbox)o).IsChecked;
                gen_Circle.ReGenerate_Preview();
            });
            CircleReverse = GwenHelper.AddCheckbox(CircleGenOptions, "Reverse", gen_Circle.reverse, (o, e) =>
            {
                gen_Circle.reverse = ((Checkbox)o).IsChecked;
                gen_Circle.ReGenerate_Preview();
            });
            if (gen_Circle.lineType == LineType.Scenery)
            {
                CircleInverse.Disable();
            }
            if (gen_Circle.lineType != LineType.Acceleration)
            {
                CircleReverse.Disable();
            }
        }

        private void Populate10pc()
        {
            TenPCOptions = new ControlBase(null)
            {
                Margin = Margin.Zero,
                Dock = Dock.Top,
                AutoSizeToContents = true
            };
            TenPCX = new Spinner(null)
            {
                Min = -1.0e9,
                Max = 1.0e9,
                Value = gen_10pc.speed.X
            };
            TenPCX.ValueChanged += (o, e) =>
            {
                gen_10pc.speed.X = TenPCX.Value;
                gen_10pc.ReGenerate_Preview();
            };
            TenPCY = new Spinner(null)
            {
                Min = -1.0e9,
                Max = 1.0e9,
                Value = gen_10pc.speed.Y
            };
            TenPCY.ValueChanged += (o, e) =>
            {
                gen_10pc.speed.Y = TenPCY.Value;
                gen_10pc.ReGenerate_Preview();
            };
            TenPCRotation = new Spinner(null)
            {
                Min = -1.0e9,
                Max = 1.0e9,
                Value = gen_10pc.rotation,
                IncrementSize = 5
            };
            TenPCRotation.ValueChanged += (o, e) =>
            {
                gen_10pc.rotation = TenPCRotation.Value;
                gen_10pc.ReGenerate_Preview();
            };

            _ = GwenHelper.CreateLabeledControl(TenPCOptions, "X Speed", TenPCX);
            _ = GwenHelper.CreateLabeledControl(TenPCOptions, "Y Speed", TenPCY);
            _ = GwenHelper.CreateLabeledControl(TenPCOptions, "Rotation Amount", TenPCRotation);

            _ = GwenHelper.CreateLabeledControl(TenPCOptions, "\nContact Point Selection", new ControlBase());

            string[] labels = { "Tail", "Peg", "Nose", "String", "Butt", "Shoulder", "Left Hand", "Right Hand", "Left Foot", "Right Foot" };

            LeftColumn = new ControlBase(TenPCOptions)
            {
                Dock = Dock.Left,
                Margin = Margin.Zero,
                AutoSizeToContents = true
            };
            RightColumn = new ControlBase(TenPCOptions)
            {
                Dock = Dock.Left,
                Margin = new Margin(10, 0, 0, 0),
                AutoSizeToContents = true
            };
            TenPCPointSelect = new Checkbox[10];

            for (int i = 0; i < 5; i++)
            {
                int currentI = i;
                TenPCPointSelect[i] = GwenHelper.AddCheckbox(LeftColumn, labels[i], gen_10pc.selected_points[i], (o, e) =>
                {
                    gen_10pc.selected_points[currentI] = ((Checkbox)o).IsChecked;
                    gen_10pc.ReGenerate_Preview();
                });
            }

            for (int i = 5; i < 10; i++)
            {
                int currentI = i;
                TenPCPointSelect[i] = GwenHelper.AddCheckbox(RightColumn, labels[i], gen_10pc.selected_points[i], (o, e) =>
                {
                    gen_10pc.selected_points[currentI] = ((Checkbox)o).IsChecked;
                    gen_10pc.ReGenerate_Preview();
                });
            }
        }

        private void PopulateLine()
        {
            LineGenOptions = new ControlBase(null)
            {
                Margin = Margin.Zero,
                Dock = Dock.Top,
                AutoSizeToContents = true
            };

            LineX1 = new SpinnerG17(null) //Start of line
            {
                Min = -30000000000,
                Max = 30000000000,
                Value = gen_Line.positionStart.X
            };
            LineX1.ValueChanged += (o, e) =>
            {
                gen_Line.positionStart.X = LineX1.Value;
                gen_Line.ReGenerate_Preview();
            };
            LineY1 = new SpinnerG17(null)
            {
                Min = -30000000000,
                Max = 30000000000,
                Value = gen_Line.positionStart.Y
            };
            LineY1.ValueChanged += (o, e) =>
            {
                gen_Line.positionStart.Y = LineY1.Value;
                gen_Line.ReGenerate_Preview();
            };

            LineX2 = new SpinnerG17(null) //End of line
            {
                Min = -30000000000,
                Max = 30000000000,
                Value = gen_Line.positionEnd.X
            };
            LineX2.ValueChanged += (o, e) =>
            {
                gen_Line.positionEnd.X = LineX2.Value;
                gen_Line.ReGenerate_Preview();
            };
            LineY2 = new SpinnerG17(null)
            {
                Min = -30000000000,
                Max = 30000000000,
                Value = gen_Line.positionEnd.Y
            };
            LineY2.ValueChanged += (o, e) =>
            {
                gen_Line.positionEnd.Y = LineY2.Value;
                gen_Line.ReGenerate_Preview();
            };

            LineMultiplier = new Spinner(null)
            {
                Min = 0,
                Max = 255,
                Value = gen_Line.multiplier,
                IsDisabled = gen_Line.lineType != LineType.Acceleration
            };
            LineMultiplier.ValueChanged += (o, e) =>
            {
                gen_Line.multiplier = (int)LineMultiplier.Value;
                gen_Line.ReGenerate_Preview();
            };
            LineWidth = new Spinner(null)
            {
                Min = 0.1,
                Max = 25.5,
                Value = gen_Line.width,
                IncrementSize = 0.1,
                IsDisabled = gen_Line.lineType != LineType.Scenery
            };
            LineWidth.ValueChanged += (o, e) =>
            {
                gen_Line.width = (float)LineWidth.Value;
                gen_Line.ReGenerate_Preview();
            };

            _ = GwenHelper.CreateLabeledControl(LineGenOptions, "Start X", LineX1);
            _ = GwenHelper.CreateLabeledControl(LineGenOptions, "Start Y", LineY1);
            _ = GwenHelper.CreateLabeledControl(LineGenOptions, "End X", LineX2);
            _ = GwenHelper.CreateLabeledControl(LineGenOptions, "End Y", LineY2);
            _ = GwenHelper.CreateLabeledControl(LineGenOptions, "Acceleration Multiplier", LineMultiplier);
            _ = GwenHelper.CreateLabeledControl(LineGenOptions, "Scenery Width Multiplier", LineWidth);

            RadioButtonGroup lineTypeRadioGroup = new RadioButtonGroup(LineGenOptions)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false
            };
            RadioButton blueType = lineTypeRadioGroup.AddOption("Blue");
            RadioButton redType = lineTypeRadioGroup.AddOption("Red");
            RadioButton greenType = lineTypeRadioGroup.AddOption("Green");
            switch (gen_Line.lineType)
            {
                case LineType.Standard:
                    blueType.Select();
                    break;
                case LineType.Acceleration:
                    redType.Select();
                    break;
                case LineType.Scenery:
                    greenType.Select();
                    break;
                default:
                    break;
            }
            blueType.CheckChanged += (o, e) =>
            {
                gen_Line.lineType = LineType.Standard;
                gen_Line.ReGenerate_Preview();
                LineMultiplier.Disable();
                LineWidth.Disable();
                LineInverse.Enable();
                LineReverse.Disable();
            };
            redType.CheckChanged += (o, e) =>
            {
                gen_Line.lineType = LineType.Acceleration;
                gen_Line.ReGenerate_Preview();
                LineMultiplier.Enable();
                LineWidth.Disable();
                LineInverse.Enable();
                LineReverse.Enable();
            };
            greenType.CheckChanged += (o, e) =>
            {
                gen_Line.lineType = LineType.Scenery;
                gen_Line.ReGenerate_Preview();
                LineMultiplier.Disable();
                LineWidth.Enable();
                LineInverse.Disable();
                LineReverse.Disable();
            };

            LineInverse = GwenHelper.AddCheckbox(LineGenOptions, "Invert", gen_Line.invert, (o, e) =>
            {
                gen_Line.invert = ((Checkbox)o).IsChecked;
                gen_Line.ReGenerate_Preview();
            });
            LineReverse = GwenHelper.AddCheckbox(LineGenOptions, "Reverse", gen_Line.reverse, (o, e) =>
            {
                gen_Line.reverse = ((Checkbox)o).IsChecked;
                gen_Line.ReGenerate_Preview();
            });
            if (gen_Line.lineType == LineType.Scenery)
            {
                LineInverse.Disable();
            }
            if (gen_Line.lineType != LineType.Acceleration)
            {
                LineReverse.Disable();
            }
        }

        private void PopulateKramual()
        {
            KramualGenOptions = new ControlBase(null)
            {
                Margin = Margin.Zero,
                Dock = Dock.Top,
                AutoSizeToContents = true
            };
            //override position
            KramualPositionOverride = GwenHelper.AddCheckbox(KramualGenOptions, "Override position", gen_Kramual.overridePosition, (o, e) =>
            {
                gen_Kramual.overridePosition = ((Checkbox)o).IsChecked; //bunch of logic to disable certain spinners
                if (((Checkbox)o).IsChecked)
                {
                    if (gen_Kramual.vert)
                    {
                        gen_Kramual.position.X = KramualX.Value;
                        KramualY.Disable();
                        KramualX.Enable();
                    }
                    else
                    {
                        gen_Kramual.position.Y = KramualY.Value;
                        KramualX.Disable();
                        KramualY.Enable();
                    }
                }
                else
                {
                    KramualX.Disable();
                    KramualY.Disable();
                }
                gen_Kramual.ReGenerate_Preview();
            });

            KramualX = new SpinnerG17(null) //coordinates for override
            {
                Min = -30000000000,
                Max = 30000000000,
                Value = gen_Kramual.position.X,
                IsDisabled = !gen_Kramual.vert | !gen_Kramual.overridePosition
            };
            KramualX.ValueChanged += (o, e) =>
            {
                gen_Kramual.position.X = KramualX.Value;
                gen_Kramual.ReGenerate_Preview();
            };
            KramualY = new SpinnerG17(null)
            {
                Min = -30000000000,
                Max = 30000000000,
                Value = gen_Kramual.position.Y,
                IsDisabled = gen_Kramual.vert | !gen_Kramual.overridePosition
            };
            KramualY.ValueChanged += (o, e) =>
            {
                gen_Kramual.position.Y = KramualY.Value;
                gen_Kramual.ReGenerate_Preview();
            };
            _ = GwenHelper.CreateLabeledControl(KramualGenOptions, "X Position", KramualX);
            _ = GwenHelper.CreateLabeledControl(KramualGenOptions, "Y Position", KramualY);

            RadioButtonGroup axisRadioGroup = new RadioButtonGroup(KramualGenOptions) //horizontal or vertical kramual?
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false
            };
            RadioButton horizontal = axisRadioGroup.AddOption("Horizontal");
            RadioButton vertical = axisRadioGroup.AddOption("Vertical");
            switch (gen_Kramual.vert)
            {
                case false:
                    horizontal.Select();
                    break;
                case true:
                    vertical.Select();
                    break;
                default:
                    break;
            }
            horizontal.CheckChanged += (o, e) =>
            {
                gen_Kramual.vert = false;
                gen_Kramual.ReGenerate_Preview();
                if (gen_Kramual.overridePosition)
                {
                    KramualY.Enable();
                    KramualX.Disable();
                }
            };
            vertical.CheckChanged += (o, e) =>
            {
                gen_Kramual.vert = true;
                gen_Kramual.ReGenerate_Preview();
                if (gen_Kramual.overridePosition)
                {
                    KramualX.Enable();
                    KramualY.Disable();
                }
            };

            KramualFrameOverride = GwenHelper.AddCheckbox(KramualGenOptions, "Override Frame", gen_Kramual.overrideFrame, (o, e) => //override frame
            {
                gen_Kramual.overrideFrame = ((Checkbox)o).IsChecked;
                KramualFrame.IsDisabled = !((Checkbox)o).IsChecked;
                gen_Kramual.ReGenerate_Preview();
            });
            KramualFrame = new Spinner(null) //End of line
            {
                Min = 0,
                Max = 30000000000,
                Value = gen_Kramual.frame,
                IsDisabled = !gen_Kramual.overrideFrame
            };
            KramualFrame.ValueChanged += (o, e) =>
            {
                gen_Kramual.frame = (int)KramualFrame.Value;
                gen_Kramual.ReGenerate_Preview();
            };
            _ = GwenHelper.CreateLabeledControl(KramualGenOptions, "Frame", KramualFrame);

            KramualIterationOverride = GwenHelper.AddCheckbox(KramualGenOptions, "Override Iteration", gen_Kramual.overrideIteration, (o, e) => //override iteration
            {
                gen_Kramual.overrideIteration = ((Checkbox)o).IsChecked;
                KramualIteration.IsDisabled = !((Checkbox)o).IsChecked;
                gen_Kramual.ReGenerate_Preview();
            });
            KramualIteration = new Spinner(null)
            {
                Min = 0,
                Max = 6,
                Value = gen_Kramual.iteration,
                IsDisabled = !gen_Kramual.overrideIteration
            };
            KramualIteration.ValueChanged += (o, e) =>
            {
                gen_Kramual.iteration = (int)KramualIteration.Value;
                gen_Kramual.ReGenerate_Preview();
            };
            _ = GwenHelper.CreateLabeledControl(KramualGenOptions, "Iteration", KramualIteration);

            KramualMultiplier = new Spinner(null) //red line multiplier
            {
                Min = 0,
                Max = 255,
                Value = gen_Kramual.multiplier,
                IsDisabled = gen_Kramual.lineType != LineType.Acceleration
            };
            KramualMultiplier.ValueChanged += (o, e) =>
            {
                gen_Kramual.multiplier = (int)KramualMultiplier.Value;
                gen_Kramual.ReGenerate_Preview();
            };
            _ = GwenHelper.CreateLabeledControl(KramualGenOptions, "Acceleration Multiplier", KramualMultiplier);

            RadioButtonGroup lineTypeRadioGroup = new RadioButtonGroup(KramualGenOptions) //linetype
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false
            };
            RadioButton blueType = lineTypeRadioGroup.AddOption("Blue");
            RadioButton redType = lineTypeRadioGroup.AddOption("Red");
            switch (gen_Kramual.lineType)
            {
                case LineType.Standard:
                    blueType.Select();
                    break;
                case LineType.Acceleration:
                    redType.Select();
                    break;
                default:
                    break;
            }
            blueType.CheckChanged += (o, e) =>
            {
                gen_Kramual.lineType = LineType.Standard;
                gen_Kramual.ReGenerate_Preview();
                KramualMultiplier.Disable();
                KramualReverse.Disable();
            };
            redType.CheckChanged += (o, e) =>
            {
                gen_Kramual.lineType = LineType.Acceleration;
                gen_Kramual.ReGenerate_Preview();
                KramualMultiplier.Enable();
                KramualReverse.Enable();
            };

            KramualReverse = GwenHelper.AddCheckbox(KramualGenOptions, "Reverse", gen_Kramual.reverse, (o, e) => //reverse mode for red lines
            {
                gen_Kramual.reverse = ((Checkbox)o).IsChecked;
                gen_Kramual.ReGenerate_Preview();
            });
            if (gen_Kramual.lineType != LineType.Acceleration)
            {
                KramualReverse.Disable();
            }
        }
        private void CategorySelected(object sender, ItemSelectedEventArgs e)
        {
            if (_focus != e.SelectedItem.UserData)
            {
                _focus?.Hide();
                _focus = (ControlBase)e.SelectedItem.UserData;
                _focus.Show();
                Settings.SettingsPane = (int)_focus.UserData;
                Settings.Save();
            }
        }

        //private ControlBase AddPage(CollapsibleCategory category, string name)
        //{
        //    Button btn = category.Add(name);
        //    Panel panel = new Panel(this)
        //    {
        //        Dock = Dock.Fill,
        //        Padding = Padding.Five
        //    };
        //    panel.Hide();
        //    panel.UserData = _tabscount;
        //    btn.UserData = panel;
        //    category.Selected += CategorySelected;
        //    if (_tabscount == Settings.SettingsPane)
        //        btn.Press();
        //    _tabscount += 1;
        //    return panel;
        //}

        public override bool Close() => base.Close();

        protected override void CloseButtonPressed(ControlBase control, EventArgs args)
        {
            if (!GeneratePressed)
            {
                Render_Clear();
            }
            base.CloseButtonPressed(control, args);
        }

        private void Render_Preview() // Renders the generator's preview lines
        {
            switch (CurrentGenerator)
            {
                default:
                    break;
                case GeneratorType.Circle:
                    gen_Circle.ReGenerate_Preview();
                    break;
                case GeneratorType.TenPC:
                    gen_10pc.ReGenerate_Preview();
                    break;
                case GeneratorType.Line:
                    gen_Line.ReGenerate_Preview();
                    break;
                case GeneratorType.Kramual:
                    gen_Kramual.ReGenerate_Preview();
                    break;
            }
        }
        private void Render_Final() // Renders the generator's final lines (which are the ones actually added to the track)
        {
            switch (CurrentGenerator)
            {
                default:
                    break;
                case GeneratorType.Circle:
                    gen_Circle.DeleteLines();
                    gen_Circle.Generate();
                    gen_Circle.Finalise();
                    break;
                case GeneratorType.TenPC:
                    gen_10pc.DeleteLines();
                    gen_10pc.Generate();
                    gen_10pc.Finalise();
                    break;
                case GeneratorType.Line:
                    gen_Line.DeleteLines();
                    gen_Line.Generate();
                    gen_Line.Finalise();
                    break;
                case GeneratorType.Kramual:
                    gen_Kramual.DeleteLines();
                    gen_Kramual.Generate();
                    gen_Kramual.Finalise();
                    break;
            }
        }
        private void Render_Clear() // Clears all lines rendered by the current generator
        {
            switch (CurrentGenerator)
            {
                default:
                    break;
                case GeneratorType.Circle:
                    gen_Circle.DeleteLines();
                    break;
                case GeneratorType.TenPC:
                    gen_10pc.DeleteLines();
                    break;
                case GeneratorType.Line:
                    gen_Line.DeleteLines();
                    break;
                case GeneratorType.Kramual:
                    gen_Kramual.DeleteLines();
                    break;
            }
        }
    }
}
