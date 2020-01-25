using DarkUI.Config;
using DarkUI.Extensions;
using DarkUI.Icons;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Renderers
{
    public class DarkToolStripRenderer : DarkMenuRenderer
    {
        #region Initialisation Region

        protected override void InitializeItem(ToolStripItem item)
        {
            base.InitializeItem(item);

            if (item.GetType() == typeof(ToolStripSeparator))
            {
                var castItem = (ToolStripSeparator)item;
                if (!castItem.IsOnDropDown)
                    item.Margin = new Padding(0, 0, 2, 0);
            }

            if (item.GetType() == typeof(ToolStripButton))
            {
                item.AutoSize = false;
                item.Size = new Size(24, 24);
            }
        }

        #endregion

        #region Render Region

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            base.OnRenderToolStripBackground(e);

            var g = e.Graphics;

            if (e.ToolStrip.GetType() == typeof(ToolStripOverflow))
            {
                using (var p = new Pen(Colors.GreyBackground))
                {
                    var rect = new Rectangle(e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
                    g.DrawRectangle(p, rect);
                }
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip.GetType() != typeof(ToolStrip))
                base.OnRenderToolStripBorder(e);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var g = e.Graphics;

            var rect = new Rectangle(0, 1, e.Item.Width, e.Item.Height - 2);

            if (e.Item.Selected || e.Item.Pressed)
            {
                using (var b = new SolidBrush(Colors.GreySelection))
                {
                    g.FillRectangle(b, rect);
                }
            }

            if (e.Item.GetType() == typeof(ToolStripButton))
            {
                var castItem = (ToolStripButton)e.Item;

                if (castItem.Checked)
                {
                    using (var b = new SolidBrush(Colors.GreySelection))
                    {
                        g.FillRectangle(b, rect);
                    }
                }

                if (castItem.Checked && castItem.Selected)
                {
                    var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                    using (var p = new Pen(Colors.GreyHighlight))
                    {
                        g.DrawRectangle(p, modRect);
                    }
                }
            }
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var g = e.Graphics;

            var rect = new Rectangle(0, 1, e.Item.Width, e.Item.Height - 2);

            if (e.Item.Selected || e.Item.Pressed)
            {
                using (var b = new SolidBrush(Colors.GreySelection))
                {
                    g.FillRectangle(b, rect);
                }
            }
        }

        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
        {
            if (e.GripStyle == ToolStripGripStyle.Hidden)
                return;

            var g = e.Graphics;

            using (var img = MenuIcons.grip.SetColor(Colors.LightBorder))
            {
                g.DrawImageUnscaled(img, new Point(e.AffectedBounds.Left, e.AffectedBounds.Top));
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var g = e.Graphics;

            var castItem = (ToolStripSeparator)e.Item;
            if (castItem.IsOnDropDown)
            {
                base.OnRenderSeparator(e);
                return;
            }

            var rect = new Rectangle(3, 3, 2, e.Item.Height - 4);

            using (var p = new Pen(Colors.DarkBorder))
            {
                g.DrawLine(p, rect.Left, rect.Top, rect.Left, rect.Height);
            }

            using (var p = new Pen(Colors.LightBorder))
            {
                g.DrawLine(p, rect.Left + 1, rect.Top, rect.Left + 1, rect.Height);
            }
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;

            if (e.Image == null)
                return;

            if (e.Item.Enabled)
                g.DrawImageUnscaled(e.Image, new Point(e.ImageRectangle.Left, e.ImageRectangle.Top));
            else
                ControlPaint.DrawImageDisabled(g, e.Image, e.ImageRectangle.Left, e.ImageRectangle.Top, Color.Transparent);
        }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
        {
            /*var g = e.Graphics;

            var rect = new Rectangle(1, 0, e.Item.Width - 5, e.Item.Height);

            var castItem = (ToolStripOverflowButton)e.Item;

            var bgColor = BasicColors.White;
            if (castItem.Selected)
                bgColor = StyleColors.Weak(style);
            if (castItem.Pressed)
                bgColor = StyleColors.Medium(style);

            using (var b = new SolidBrush(bgColor))
            {
                g.FillRectangle(b, rect);
            }

            var fgColor = BasicColors.Grey;
            if (castItem.Selected)
                fgColor = StyleColors.Medium(style);
            if (castItem.Pressed)
                fgColor = StyleColors.Strong(style);

            using (var p = new Pen(fgColor))
            {
                var modRect = new Rectangle(1, 0, e.Item.Width - 6, e.Item.Height - 1);
                g.DrawRectangle(p, modRect);
            }

            using (var img = MenuIcons.overflow.SetColor(BasicColors.MediumGrey))
            {
                g.DrawImageUnscaled(img, e.Item.Width - 13, e.Item.Height - 9);
            }*/
        }

        #endregion
    }
}