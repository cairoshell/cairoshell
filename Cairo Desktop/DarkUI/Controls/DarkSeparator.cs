using DarkUI.Config;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkSeparator : Control
    {
        #region Constructor Region

        public DarkSeparator()
        {
            SetStyle(ControlStyles.Selectable, false);

            Dock = DockStyle.Top;
            Size = new Size(1, 2);
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            using (var p = new Pen(Colors.DarkBorder))
            {
                g.DrawLine(p, ClientRectangle.Left, 0, ClientRectangle.Right, 0);
            }

            using (var p = new Pen(Colors.LightBorder))
            {
                g.DrawLine(p, ClientRectangle.Left, 1, ClientRectangle.Right, 1);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Absorb event
        }

        #endregion
    }
}
