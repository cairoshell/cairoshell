using System.Drawing;
using ManagedShell.AppBar;
using System.Windows.Forms;

namespace CairoDesktop.Common.Helpers
{
    public static class CursorHelper
    {
        public static bool IsCursorOnScreen(Screen screen)
        {
            return IsCursorInRectangle(screen.Bounds);
        }

        public static bool IsCursorOnScreen(AppBarScreen screen)
        {
            return IsCursorInRectangle(screen.Bounds);
        }

        private static bool IsCursorInRectangle(Rectangle bounds)
        {
            var cursorPos = Cursor.Position;

            if (cursorPos.X >= bounds.Left && cursorPos.X <= bounds.Right &&
                cursorPos.Y >= bounds.Top && cursorPos.Y <= bounds.Bottom)
            {
                return true;
            }

            return false;
        }
    }
}
