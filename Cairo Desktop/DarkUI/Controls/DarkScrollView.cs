using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public abstract class DarkScrollView : DarkScrollBase
    {
        #region Constructor Region

        protected DarkScrollView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
        }

        #endregion

        #region Paint Region

        protected abstract void PaintContent(Graphics g);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            // Draw background
            using (var b = new SolidBrush(BackColor))
            {
                g.FillRectangle(b, ClientRectangle);
            }

            // Offset the graphics based on the viewport, render the control contents, then reset it.
            g.TranslateTransform(Viewport.Left * -1, Viewport.Top * -1);

            PaintContent(g);

            g.TranslateTransform(Viewport.Left, Viewport.Top);

            // Draw the bit where the scrollbars meet
            if (_vScrollBar.Visible && _hScrollBar.Visible)
            {
                using (var b = new SolidBrush(BackColor))
                {
                    var rect = new Rectangle(_hScrollBar.Right, _vScrollBar.Bottom, _vScrollBar.Width,
                                             _hScrollBar.Height);

                    g.FillRectangle(b, rect);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Absorb event
        }

        #endregion
    }
}
