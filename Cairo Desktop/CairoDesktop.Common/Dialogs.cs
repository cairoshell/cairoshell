using System.Linq;
using System.Windows.Forms;

namespace CairoDesktop.SupportingClasses
{
    public sealed class Dialogs
    {
        static Dialogs() { }
        private Dialogs() { }

        public static DialogResult OpenFileDialog(string filter, out string filename)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = filter
            };

            DialogResult result;
            filename = null;

            try
            {
                result = dlg.ShowDialog();
            }
            catch
            {
                // show retro dialog if the better one fails to load
                dlg.AutoUpgradeEnabled = false;
                result = dlg.ShowDialog();
            }

            filename = dlg.FileName;

            return result;
        }
    }
}
