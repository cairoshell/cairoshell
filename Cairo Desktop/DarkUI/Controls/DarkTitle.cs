using DarkUI.Config;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkTitle : Label
    {
        #region Constructor Region

        public DarkTitle()
        { }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            var textSize = g.MeasureString(Text, Font);

            using (var b = new SolidBrush(Colors.LightText))
            {
                g.DrawString(Text, Font, b, new PointF(-2, 0));
            }

            using (var p = new Pen(Colors.GreyHighlight))
            {
                var p1 = new PointF(textSize.Width + 5, textSize.Height / 2);
                var p2 = new PointF(rect.Width, textSize.Height / 2);
                g.DrawLine(p, p1, p2);
            }
        }

        #endregion
    }
}
