using Gwen.Input;
using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Text box (editable).
    /// </summary>
    public class TextBox : Label
    {
        #region Events

        /// <summary>
        /// Invoked when the submit key has been pressed.
        /// </summary>
        public event GwenEventHandler<EventArgs> SubmitPressed;

        /// <summary>
        /// Invoked when the text has changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> TextChanged;

        #endregion Events

        #region Properties

        public int CursorEnd
        {
            get => m_CursorEnd;
            set
            {
                if (m_CursorEnd == value)
                    return;

                m_CursorEnd = value;
                RefreshCursorBounds();
                ResetCaretBlink();
            }
        }

        /// <summary>
        /// Current cursor position (character index).
        /// </summary>
        public int CursorPos
        {
            get => m_CursorPos;
            set
            {
                if (m_CursorPos == value)
                    return;

                m_CursorPos = value;
                RefreshCursorBounds();
                ResetCaretBlink();
            }
        }

        /// <summary>
        /// Indicates whether the text has active selection.
        /// </summary>
        public virtual bool HasSelection => m_CursorPos != m_CursorEnd;

        /// <summary>
        /// Determines whether text should be selected when the control is focused.
        /// </summary>
        public bool SelectAllOnFocus
        {
            get => m_SelectAll; set
            {
                m_SelectAll = value;
                if (value)
                    OnSelectAll(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Font.
        /// </summary>
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                SetupDefault();
            }
        }
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                m_CursorPos = Math.Min(m_CursorPos, Text.Length);
                m_CursorEnd = Math.Min(m_CursorEnd, Text.Length);
            }
        }
        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TextBox(ControlBase parent)
            : base(parent)
        {
            AutoSizeToContents = false;
            SetupDefault();
            Width = 200;

            MouseInputEnabled = true;
            KeyboardInputEnabled = true;

            Alignment = Pos.CenterV;
            TextPadding = new Padding(4, 2, 4, 2);

            m_CursorPos = 0;
            m_CursorEnd = 0;
            m_SelectAll = false;

            TextColor = Color.FromArgb(255, 50, 50, 50); // TODO: From Skin

            IsTabable = true;

            AddAccelerator("Ctrl + C", OnCopy);
            AddAccelerator("Ctrl + X", OnCut);
            AddAccelerator("Ctrl + V", OnPaste);
            AddAccelerator("Ctrl + A", OnSelectAll);
            Cursor = Cursors.IBeam;
        }

        #endregion Constructors

        #region Methods

        private void SetupDefault()
        {
            int extra = TextHeight / 4;
            Padding textpadding = new(extra * 2, extra, extra * 2, extra);
            if (TextPadding != textpadding)
            {
                TextPadding = textpadding;
                if (GetSizeToFitContents().Height > Height)
                    _ = SizeToChildren(false, true);
            }
        }
        /// <summary>
        /// Deletes text.
        /// </summary>
        /// <param name="startPos">Starting cursor position.</param>
        /// <param name="length">Length in characters.</param>
        public virtual void DeleteText(int startPos, int length)
        {
            string str = Text;
            str = str.Remove(startPos, length);
            SetText(str);

            if (m_CursorPos > startPos)
            {
                CursorPos = m_CursorPos - length;
            }

            CursorEnd = m_CursorPos;
        }

        /// <summary>
        /// Deletes selected text.
        /// </summary>
        public virtual void EraseSelection()
        {
            int start = Math.Min(m_CursorPos, m_CursorEnd);
            int end = Math.Max(m_CursorPos, m_CursorEnd);

            DeleteText(start, end - start);

            // Move the cursor to the start of the selection,
            // since the end is probably outside of the string now.
            m_CursorPos = start;
            m_CursorEnd = start;
        }

        /// <summary>
        /// Returns currently selected text.
        /// </summary>
        /// <returns>Current selection.</returns>
        public string GetSelection()
        {
            if (!HasSelection)
                return string.Empty;

            int start = Math.Min(m_CursorPos, m_CursorEnd);
            int end = Math.Max(m_CursorPos, m_CursorEnd);

            string str = Text;
            return str.Substring(start, end - start);
        }
        public override void SetText(string str, bool doEvents = true)
        {
            base.SetText(str, doEvents);
            // Bugfix: crash when cursorpos > text length
            m_CursorPos = Math.Min(m_CursorPos, Text.Length);
            m_CursorEnd = Math.Min(m_CursorEnd, Text.Length);
        }

        #endregion Methods

        #region Fields

        protected Rectangle m_CaretBounds;
        protected double m_LastInputTime;
        protected Rectangle m_SelectionBounds;

        #endregion Fields

        protected override bool AccelOnlyFocus => true;
        protected override bool NeedsInputChars => true;

        /// <summary>
        /// Inserts text at current cursor position, erasing selection if any.
        /// </summary>
        /// <param name="text">Text to insert.</param>
        protected virtual void InsertText(string text)
        {
            // TODO: Make sure fits (implement maxlength)

            if (HasSelection)
            {
                EraseSelection();
            }

            if (m_CursorPos > TextLength)
                m_CursorPos = TextLength;

            if (!IsTextAllowed(text, m_CursorPos))
                return;

            string str = Text;
            str = str.Insert(m_CursorPos, text);
            // we adjust cursor pos ahead of time because settext can manipulate
            // Text, breaking our cursor pos.
            m_CursorPos += text.Length;
            m_CursorEnd = m_CursorPos;
            SetText(str);

            RefreshCursorBounds();
            ResetCaretBlink();
        }

        /// <summary>
        /// Determines whether the control can insert text at a given cursor position.
        /// </summary>
        /// <param name="text">Text to check.</param>
        /// <param name="position">Cursor position.</param>
        /// <returns>True if allowed.</returns>
        protected virtual bool IsTextAllowed(string text, int position) => true;

        protected override void ProcessLayout(Size size)
        {
            m_Text.AlignToEdge(Alignment, TextPadding);
            RefreshCursorBounds();
            // Do not call base.processlayout
        }

        protected virtual void MakeCaretVisible()
        {
            int caretPos = GetCharacterPosition(m_CursorPos).X - TextX;

            // If the caret is already in a semi-good position, leave it.
            {
                int realCaretPos = caretPos + TextX;
                if (realCaretPos > Width * 0.1f && realCaretPos < Width * 0.9f)
                    return;
            }

            // The ideal position is for the caret to be right in the middle
            int idealx = (int)(-caretPos + Width * 0.5f);

            // Don't show too much whitespace to the right
            if (idealx + TextWidth < Width - TextPadding.Right)
                idealx = -TextWidth + (Width - TextPadding.Right);

            // Or the left
            if (idealx > TextPadding.Left)
                idealx = TextPadding.Left;

            SetTextPosition(idealx, TextY);
        }

        /// <summary>
        /// Handler for character input event.
        /// </summary>
        /// <param name="chr">Character typed.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnChar(char chr)
        {
            _ = base.OnChar(chr);

            if (chr == '\t')
                return false;

            InsertText(chr.ToString());
            return true;
        }

        /// <summary>
        /// Handler for Copy event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected override void OnCopy(ControlBase from, EventArgs args)
        {
            if (!HasSelection)
                return;
            base.OnCopy(from, args);

            _ = Platform.Neutral.SetClipboardText(GetSelection());
        }

        /// <summary>
        /// Handler for Cut event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected override void OnCut(ControlBase from, EventArgs args)
        {
            if (!HasSelection)
                return;
            base.OnCut(from, args);

            _ = Platform.Neutral.SetClipboardText(GetSelection());
            EraseSelection();
        }

        /// <summary>
        /// Handler for Backspace keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyBackspace(bool down)
        {
            _ = base.OnKeyBackspace(down);

            if (!down)
                return true;
            if (HasSelection)
            {
                EraseSelection();
                return true;
            }

            if (m_CursorPos == 0)
                return true;

            DeleteText(m_CursorPos - 1, 1);

            return true;
        }

        /// <summary>
        /// Handler for Delete keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyDelete(bool down)
        {
            _ = base.OnKeyDelete(down);
            if (!down)
                return true;
            if (HasSelection)
            {
                EraseSelection();
                return true;
            }

            if (m_CursorPos >= TextLength)
                return true;

            DeleteText(m_CursorPos, 1);

            return true;
        }

        /// <summary>
        /// Handler for End keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyEnd(bool down)
        {
            _ = base.OnKeyEnd(down);
            m_CursorPos = TextLength;

            if (!InputHandler.IsShiftDown)
            {
                m_CursorEnd = m_CursorPos;
            }

            RefreshCursorBounds();
            return true;
        }

        /// <summary>
        /// Handler for Home keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyHome(bool down)
        {
            _ = base.OnKeyHome(down);
            if (!down)
                return true;
            m_CursorPos = 0;

            if (!InputHandler.IsShiftDown)
            {
                m_CursorEnd = m_CursorPos;
            }

            RefreshCursorBounds();
            return true;
        }

        /// <summary>
        /// Handler for Left Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyLeft(bool down)
        {
            _ = base.OnKeyLeft(down);
            if (!down)
                return true;

            if (m_CursorPos > 0)
                m_CursorPos--;

            if (!InputHandler.IsShiftDown)
            {
                m_CursorEnd = m_CursorPos;
            }
            ResetCaretBlink();
            RefreshCursorBounds();
            return true;
        }

        /// <summary>
        /// Handler for Return keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyReturn(bool down)
        {
            _ = base.OnKeyReturn(down);
            if (down)
                return true;

            OnReturn();

            // Try to move to the next control, as if tab had been pressed
            _ = OnKeyTab(true);

            // If we still have focus, blur it.
            if (HasFocus)
            {
                Blur();
            }

            return true;
        }

        /// <summary>
        /// Handler for Right Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyRight(bool down)
        {
            _ = base.OnKeyRight(down);
            if (!down)
                return true;

            if (m_CursorPos < TextLength)
                m_CursorPos++;

            if (!InputHandler.IsShiftDown)
            {
                m_CursorEnd = m_CursorPos;
            }

            ResetCaretBlink();
            RefreshCursorBounds();
            return true;
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            base.OnMouseClickedLeft(x, y, down);
            if (m_SelectAll)
            {
                OnSelectAll(this, EventArgs.Empty);
                //m_SelectAll = false;
                return;
            }

            int c = GetClosestCharacter(x, y).X;

            if (down)
            {
                CursorPos = c;

                if (!InputHandler.IsShiftDown)
                    CursorEnd = c;

                InputHandler.MouseFocus = this;
            }
            else
            {
                if (InputHandler.MouseFocus == this)
                {
                    CursorPos = c;
                    InputHandler.MouseFocus = null;
                }
            }
            ResetCaretBlink();
        }

        /// <summary>
        /// Handler invoked on mouse double click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        protected override void OnMouseDoubleClickedLeft(int x, int y)
        {
            //base.OnMouseDoubleClickedLeft(x, y);
            _ = GetClosestCharacter(x, y).X;

            OnSelectAll(this, EventArgs.Empty);
            ResetCaretBlink();
        }

        /// <summary>
        /// Handler invoked on mouse moved event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X change.</param>
        /// <param name="dy">Y change.</param>
        protected override void OnMouseMoved(int x, int y, int dx, int dy)
        {
            base.OnMouseMoved(x, y, dx, dy);
            if (InputHandler.MouseFocus != this)
                return;

            int c = GetClosestCharacter(x, y).X;

            CursorPos = c;
        }

        /// <summary>
        /// Handler for Paste event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected override void OnPaste(ControlBase from, EventArgs args)
        {
            base.OnPaste(from, args);
            InsertText(Platform.Neutral.GetClipboardText());
        }

        /// <summary>
        /// Handler for the return key.
        /// </summary>
        protected virtual void OnReturn() => SubmitPressed?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Handler for Select All event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected override void OnSelectAll(ControlBase from, EventArgs args)
        {
            //base.OnSelectAll(from);
            m_CursorEnd = 0;
            m_CursorPos = TextLength;

            RefreshCursorBounds();
            ResetCaretBlink();
        }

        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected override void OnTextChanged()
        {
            base.OnTextChanged();

            if (m_CursorPos > TextLength)
                m_CursorPos = TextLength;
            if (m_CursorEnd > TextLength)
                m_CursorEnd = TextLength;

            TextChanged?.Invoke(this, EventArgs.Empty);
            ResetCaretBlink();
        }

        private void ResetCaretBlink() => m_LastInputTime = Platform.Neutral.GetTimeInSeconds();
        protected virtual void RefreshCursorBounds()
        {
            MakeCaretVisible();

            Point pA = GetCharacterPosition(m_CursorPos);
            Point pB = GetCharacterPosition(m_CursorEnd);

            m_SelectionBounds.X = Math.Min(pA.X, pB.X);
            m_SelectionBounds.Y = TextY - 1;
            m_SelectionBounds.Width = Math.Max(pA.X, pB.X) - m_SelectionBounds.X;
            m_SelectionBounds.Height = TextHeight + 2;

            m_CaretBounds.X = pA.X;
            m_CaretBounds.Y = TextY - 1;
            m_CaretBounds.Width = 1;
            m_CaretBounds.Height = TextHeight + 2;

            Redraw();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            base.Render(skin);

            if (ShouldDrawBackground)
                skin.DrawTextBox(this, HasFocus);

            if (!HasFocus)
                return;

            // Draw selection.. if selected..
            if (m_CursorPos != m_CursorEnd)
            {
                skin.Renderer.DrawColor = Color.FromArgb(200, 50, 170, 255);
                skin.Renderer.DrawFilledRect(m_SelectionBounds);
            }

            // Draw caret
            double time = Platform.Neutral.GetTimeInSeconds() - m_LastInputTime;

            if ((time % 1.0) <= 0.5)
            {
                skin.Renderer.DrawColor = Color.Black;
                skin.Renderer.DrawFilledRect(m_CaretBounds);
            }
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderFocus(Skin.SkinBase skin)
        {
            // Nothing
        }

        private int m_CursorEnd;
        private int m_CursorPos;
        private bool m_SelectAll;
    }
}