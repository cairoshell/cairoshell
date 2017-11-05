using System.Windows.Forms;

namespace CairoDesktop.WindowsTasks
{
    public class NativeWindowEx : System.Windows.Forms.NativeWindow
    {
        public delegate void MessageReceivedEventHandler(Message m);

        public event MessageReceivedEventHandler MessageReceived;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (MessageReceived != null) MessageReceived(m);
        }

        public override void CreateHandle(CreateParams cp)
        {
            base.CreateHandle(cp);
        }
    }
}