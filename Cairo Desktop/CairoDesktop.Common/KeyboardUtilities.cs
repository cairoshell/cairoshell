using CairoDesktop.Interop;

namespace CairoDesktop.Common
{
    /// <summary>
    /// Provides methods for working with the keyboard.
    /// </summary>
    /// <remarks>This class is a part of the Carbon Framework.</remarks>
    [System.Diagnostics.DebuggerStepThrough()]
    public static class KeyboardUtilities
    {
        /// <summary>
        /// Determines if a particular key is depressed on the keyboard
        /// </summary>
        /// <param name="key">The key whos state will be determined to be down or not</param>
        /// <returns></returns>
        public static bool IsKeyDown(System.Windows.Forms.Keys key)
        {
            if (NativeMethods.GetAsyncKeyState((int)key) < 0)
                return true;

            return false;
        }
    }
}