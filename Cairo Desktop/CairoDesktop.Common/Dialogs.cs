using System.Windows.Forms;

namespace CairoDesktop.SupportingClasses
{
    public static class DialogExtensionMethods
    {
        static DialogExtensionMethods() { }
        
        public static DialogResult SafeShowDialog(this OpenFileDialog openFileDialog)
        {
            DialogResult result;

            try
            {
                result = openFileDialog.ShowDialog();
            }
            catch
            {
                // show retro dialog if the better one fails to load
                openFileDialog.AutoUpgradeEnabled = false;
                result = openFileDialog.ShowDialog();
            }

            return result;
        }
    }
}