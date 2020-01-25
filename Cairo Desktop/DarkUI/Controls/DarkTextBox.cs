using DarkUI.Config;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkTextBox : TextBox
    {
        #region Constructor Region

        public DarkTextBox()
        {
            BackColor = Colors.LightBackground;
            ForeColor = Colors.LightText;
            Padding = new Padding(2, 2, 2, 2);
            BorderStyle = BorderStyle.FixedSingle;
        }

        #endregion
    }
}
