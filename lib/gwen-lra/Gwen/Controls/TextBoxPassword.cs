namespace Gwen.Controls
{
    /// <summary>
    /// Text box with masked text.
    /// </summary>
    /// <remarks>
    /// This class doesn't prevent programatic access to the text in any way.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TextBoxPassword"/> class.
    /// </remarks>
    /// <param name="parent">Parent control.</param>
    public class TextBoxPassword(ControlBase parent) : TextBox(parent)
    {
        private string m_Mask;

        /// <summary>
        /// Character used in place of actual characters for display.
        /// </summary>
        public char MaskCharacter { get; set; } = '*';

        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected override void OnTextChanged()
        {
            m_Mask = new string(MaskCharacter, Text.Length);
            TextOverride = m_Mask;
            base.OnTextChanged();
        }
    }
}
