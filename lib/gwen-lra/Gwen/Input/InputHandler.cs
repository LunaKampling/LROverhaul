﻿using Gwen.DragDrop;
using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Gwen.Input
{
    /// <summary>
    /// Input handling.
    /// </summary>
    public static class InputHandler
    {
        private static readonly KeyData m_KeyData = new KeyData();
        private static readonly double[] m_LastClickTime = new double[MaxMouseButtons];
        private static double m_TooltipCounter;
        private static Point m_LastClickPos;
        internal static int TooltipTime =>
                //wait 300ms for tooltip visibility since mouse move
                (int)((Platform.Neutral.GetTimeInSeconds() - m_TooltipCounter) * 1000);
        public static bool MouseCaptured = false;
        /// <summary>
        /// Control currently hovered by mouse.
        /// </summary>
        public static Controls.ControlBase HoveredControl;

        /// <summary>
        /// Control that corrently has keyboard focus.
        /// </summary>
        public static Controls.ControlBase KeyboardFocus;

        /// <summary>
        /// Control that currently has mouse focus.
        /// </summary>
        public static Controls.ControlBase MouseFocus;

        /// <summary>
        /// Maximum number of mouse buttons supported.
        /// </summary>
        public static int MaxMouseButtons => 5;

        /// <summary>
        /// Maximum time in seconds between mouse clicks to be recognized as double click.
        /// </summary>
        public static float DoubleClickSpeed => 0.5f;

        /// <summary>
        /// Time in seconds between autorepeating of keys.
        /// </summary>
        public static float KeyRepeatRate => 0.03f;

        /// <summary>
        /// Time in seconds before key starts to autorepeat.
        /// </summary>
        public static float KeyRepeatDelay => 0.5f;

        /// <summary>
        /// Indicates whether the left mouse button is down.
        /// </summary>
        public static bool IsLeftMouseDown => m_KeyData.LeftMouseDown;

        /// <summary>
        /// Indicates whether the right mouse button is down.
        /// </summary>
        public static bool IsRightMouseDown => m_KeyData.RightMouseDown;

        /// <summary>
        /// Current mouse position.
        /// </summary>
        public static Point MousePosition; // Not property to allow modification of Point fields

        /// <summary>
        /// Indicates whether the shift key is down.
        /// </summary>
        public static bool IsShiftDown => IsKeyDown(Key.Shift);

        /// <summary>
        /// Indicates whether the control key is down.
        /// </summary>
        public static bool IsControlDown => IsKeyDown(Key.Control);

        /// <summary>
        /// Checks if the given key is pressed.
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <returns>True if the key is down.</returns>
        public static bool IsKeyDown(Key key) => m_KeyData.KeyState[(int)key];

        /// <summary>
        /// Handles copy, paste etc.
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        /// <param name="chr">Input character.</param>
        /// <returns>True if the key was handled.</returns>
        public static bool DoSpecialKeys(Controls.ControlBase canvas, char chr)
        {
            if (null == KeyboardFocus)
                return false;
            if (KeyboardFocus.GetCanvas() != canvas)
                return false;
            if (!KeyboardFocus.IsVisible)
                return false;
            if (!IsControlDown)
                return false;

            if (chr == 'C' || chr == 'c')
            {
                KeyboardFocus.InputCopy(null);
                return true;
            }

            if (chr == 'V' || chr == 'v')
            {
                KeyboardFocus.InputPaste(null);
                return true;
            }

            if (chr == 'X' || chr == 'x')
            {
                KeyboardFocus.InputCut(null);
                return true;
            }

            if (chr == 'A' || chr == 'a')
            {
                KeyboardFocus.InputSelectAll(null);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles accelerator input.
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        /// <param name="chr">Input character.</param>
        /// <returns>True if the key was handled.</returns>
        public static bool HandleAccelerator(Controls.ControlBase canvas, char chr)
        {
            // Build the accelerator search string
            StringBuilder accelString = new StringBuilder();
            if (IsControlDown)
                _ = accelString.Append("CTRL+");
            if (IsShiftDown)
                _ = accelString.Append("SHIFT+");
            // [omeg] todo: alt?

            _ = accelString.Append(chr);
            string acc = accelString.ToString();

            //Debug::Msg("Accelerator string :%S\n", accelString.c_str());)

            return (KeyboardFocus != null && KeyboardFocus.HandleAccelerator(acc))
|| (MouseFocus != null && MouseFocus.HandleAccelerator(acc)) || canvas.HandleAccelerator(acc);
        }

        /// <summary>
        /// Mouse moved handler.
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public static void OnMouseMoved(Controls.ControlBase canvas, int x, int y, int dx, int dy)
        {
            // Send input to canvas for study
            UpdateTooltipCounter();
            MousePosition.X = x;
            MousePosition.Y = y;

            UpdateHoveredControl(canvas);
        }
        private static void UpdateTooltipCounter() =>
            // math.max in case someone delayed us.
            m_TooltipCounter = Math.Max(m_TooltipCounter, Platform.Neutral.GetTimeInSeconds());

        /// <summary>
        /// Handles focus updating and key autorepeats.
        /// </summary>
        /// <param name="control">Unused.</param>
        public static void OnCanvasThink(Controls.ControlBase control)
        {
            if (MouseFocus != null && !MouseFocus.IsVisible && !MouseFocus.IsDisabled)
                MouseFocus = null;

            if (KeyboardFocus != null && (!KeyboardFocus.IsVisible || !KeyboardFocus.KeyboardInputEnabled || KeyboardFocus.IsDisabled))
                KeyboardFocus = null;

            if (null == KeyboardFocus)
                return;
            if (KeyboardFocus.GetCanvas() != control)
                return;

            double time = Platform.Neutral.GetTimeInSeconds();

            //
            // Simulate Key-Repeats
            //
            for (int i = 0; i < (int)Key.Count; i++)
            {
                if (m_KeyData.KeyState[i] && m_KeyData.Target != KeyboardFocus)
                {
                    m_KeyData.KeyState[i] = false;
                    continue;
                }

                if (m_KeyData.KeyState[i] && time > m_KeyData.NextRepeat[i])
                {
                    m_KeyData.NextRepeat[i] = Platform.Neutral.GetTimeInSeconds() + KeyRepeatRate;

                    _ = (KeyboardFocus?.InputKeyPressed((Key)i));
                }
            }
        }

        /// <summary>
        /// Mouse click handler.
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        /// <param name="mouseButton">Mouse button number.</param>
        /// <param name="down">Specifies if the button is down.</param>
        /// <returns>True if handled.</returns>
        public static bool OnMouseClicked(Controls.Canvas canvas, int mouseButton, bool down, int x, int y)
        {
            MousePosition.X = x;
            MousePosition.Y = y;
            // If we click on a control that isn't a menu we want to close
            // all the open menus. Menus are children of the canvas.
            if (down && (null == HoveredControl || !HoveredControl.IsMenuComponent))
            {
                canvas.CloseMenus();
            }
            // Increase time for tooltip if the user clicked
            if (down)
                m_TooltipCounter = Platform.Neutral.GetTimeInSeconds() + 1;

            try
            {
                if (HoveredControl == null)
                    return MouseCaptured;
                if (HoveredControl.GetCanvas() != canvas)
                    return false;
                if (!HoveredControl.IsVisible)
                    return false;
                if (HoveredControl == canvas)
                    return false;

                if (mouseButton > MaxMouseButtons)
                    return false;

                if (mouseButton == 0)
                    m_KeyData.LeftMouseDown = down;
                else if (mouseButton == 1)
                    m_KeyData.RightMouseDown = down;

                // Double click.
                // Todo: Shouldn't double click if mouse has moved significantly
                bool isDoubleClick = false;

                if (down &&
                    m_LastClickPos.X == MousePosition.X &&
                    m_LastClickPos.Y == MousePosition.Y &&
                    (Platform.Neutral.GetTimeInSeconds() - m_LastClickTime[mouseButton]) < DoubleClickSpeed)
                {
                    isDoubleClick = true;
                }

                if (down && !isDoubleClick)
                {
                    m_LastClickTime[mouseButton] = Platform.Neutral.GetTimeInSeconds();
                    m_LastClickPos = MousePosition;
                }

                if (down)
                {
                    FindKeyboardFocus(HoveredControl);
                }

                HoveredControl.UpdateCursor();

                // This tells the child it has been touched, which
                // in turn tells its parents, who tell their parents.
                // This is basically so that Windows can pop themselves
                // to the top when one of their children have been clicked.
                if (down)
                    HoveredControl.Touch();
                if (!HoveredControl.IsDisabled)
                {
                    switch (mouseButton)
                    {
                        case 0:
                        {
                            if (DragAndDrop.OnMouseButton(HoveredControl, MousePosition.X, MousePosition.Y, down))
                                return true;

                            if (isDoubleClick)
                                HoveredControl.InputMouseDoubleClickedLeft(MousePosition.X, MousePosition.Y);
                            else
                                HoveredControl.InputMouseClickedLeft(MousePosition.X, MousePosition.Y, down);
                            UpdateHoveredControl(canvas);
                            return true;
                        }

                        case 1:
                        {
                            if (isDoubleClick)
                                HoveredControl.InputMouseDoubleClickedRight(MousePosition.X, MousePosition.Y);
                            else
                                HoveredControl.InputMouseClickedRight(MousePosition.X, MousePosition.Y, down);
                            return true;
                        }
                    }
                }

                return false;
            }
            finally
            {
                UpdateHoveredControl(canvas);
            }
        }

        /// <summary>
        /// Key handler.
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        /// <param name="key">Key.</param>
        /// <param name="down">True if the key is down.</param>
        /// <returns>True if handled.</returns>
        public static bool OnKeyEvent(Controls.ControlBase canvas, Key key, bool down)
        {
            if (null == KeyboardFocus)
                return false;
            if (KeyboardFocus.GetCanvas() != canvas)
                return false;
            if (!KeyboardFocus.IsVisible)
                return false;
            if (KeyboardFocus.IsDisabled)
                return false;

            int iKey = (int)key;
            if (down)
            {
                if (!m_KeyData.KeyState[iKey])
                {
                    m_KeyData.KeyState[iKey] = true;
                    m_KeyData.NextRepeat[iKey] = Platform.Neutral.GetTimeInSeconds() + KeyRepeatDelay;
                    m_KeyData.Target = KeyboardFocus;

                    return KeyboardFocus.InputKeyPressed(key);
                }
            }
            else
            {
                if (m_KeyData.KeyState[iKey])
                {
                    m_KeyData.KeyState[iKey] = false;

                    // BUG BUG. This causes shift left arrow in textboxes
                    // to not work. What is disabling it here breaking?
                    //m_KeyData.Target = NULL;

                    return KeyboardFocus.InputKeyPressed(key, false);
                }
            }

            return false;
        }

        private static void UpdateHoveredControl(Controls.ControlBase inCanvas)
        {
            Controls.ControlBase hovered;

            bool mousecaptured = false;
            hovered = MouseFocus != null && MouseFocus.GetCanvas() == inCanvas
                ? MouseFocus
                : inCanvas.GetControlAt(MousePosition.X, MousePosition.Y);
            if (hovered != null)
            {
                if (hovered.IsDisabled)
                {
                    mousecaptured = true;
                    hovered = null;
                }
                else if (!hovered.MouseInputEnabled)
                    hovered = null;
                else if (hovered != inCanvas)
                    mousecaptured = true;
            }
            if (HoveredControl != hovered)
            {
                if (HoveredControl != null)
                {
                    Controls.ControlBase oldHover = HoveredControl;
                    HoveredControl = null;
                    oldHover.Redraw();
                    oldHover.InputMouseLeft();
                    if (oldHover.IsMouseDepressed)
                        oldHover.InputMouseClickedLeft(MousePosition.X, MousePosition.Y, false);
                    if (oldHover.IsMouseRightDepressed)
                        oldHover.InputMouseClickedRight(MousePosition.X, MousePosition.Y, false);
                }
                if (hovered != null)
                {
                    hovered.InputMouseEntered();
                    hovered.UpdateCursor();
                }
                HoveredControl = hovered;
            }
            MouseCaptured = mousecaptured;
        }

        private static void FindKeyboardFocus(Controls.ControlBase control)
        {
            if (null == control)
                return;
            if (control.KeyboardInputEnabled)
            {
                // Make sure none of our children have keyboard focus first - todo recursive
                if (control.Children.Any(child => child == KeyboardFocus))
                {
                    return;
                }

                control.Focus();
                return;
            }

            FindKeyboardFocus(control.Parent);
            return;
        }
    }
}