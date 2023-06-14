using System;
using System.Diagnostics;

namespace Gwen.Platform
{
    /// <summary>
    /// Platform-agnostic utility functions.
    /// </summary>
    public static class Neutral
    {
        public abstract class PlatformImplementation
        {
            public abstract void SetCursor(Cursor cursor);
            public abstract bool SetClipboardText(string text);
            public abstract string GetClipboardText();
        }
        public static PlatformImplementation Implementation;
        private static readonly Stopwatch _timecounter = Stopwatch.StartNew();
        private static Cursor current = Cursors.Default;

        /// <summary>
        /// Changes the mouse cursor.
        /// </summary>
        /// <param name="cursor">Cursor type.</param>
        public static void SetCursor(Cursor cursor)
        {
            if (Implementation == null)
            {
                throw new InvalidOperationException("Platform handler not set.");
            }
            if (current != cursor)
            {
                current = cursor;
                Implementation.SetCursor(cursor);
            }
        }

        /// <summary>
        /// Gets text from clipboard.
        /// </summary>
        /// <returns>Clipboard text.</returns>
        public static string GetClipboardText() => Implementation == null ? throw new InvalidOperationException("Platform handler not set.") : Implementation.GetClipboardText();

        /// <summary>
        /// Sets the clipboard text.
        /// </summary>
        /// <param name="text">Text to set.</param>
        /// <returns>True if succeeded.</returns>
        public static bool SetClipboardText(string text) => Implementation == null
                ? throw new InvalidOperationException("Platform handler not set.")
                : Implementation.SetClipboardText(text);

        /// <summary>
        /// Gets elapsed time since this class was initalized.
        /// </summary>
        /// <returns>Time interval in seconds.</returns>
        public static double GetTimeInSeconds() => _timecounter.Elapsed.TotalSeconds;
    }
}
