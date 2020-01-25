using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Win32
{
    public class ControlScrollFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WM.MOUSEWHEEL:
                case (int)WM.MOUSEHWHEEL:
                    var hControlUnderMouse = Native.WindowFromPoint(new Point((int)m.LParam));

                    if (hControlUnderMouse == m.HWnd)
                        return false;

                    Native.SendMessage(hControlUnderMouse, (uint)m.Msg, m.WParam, m.LParam);
                    return true;
            }

            return false;
        }
    }
}
