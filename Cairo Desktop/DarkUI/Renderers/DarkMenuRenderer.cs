using DarkUI.Config;
using DarkUI.Icons;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Renderers
{
    public class DarkMenuRenderer : ToolStripRenderer
    {
        #region Initialisation Region

        protected override void Initialize(ToolStrip toolStrip)
        {
            base.Initialize(toolStrip);

            toolStrip.BackColor = Colors.GreyBackground;
            toolStrip.ForeColor = Colors.LightText;
        }

        protected override void InitializeItem(ToolStripItem item)
        {
            base.InitializeItem(item);

            item.ForeColor = Colors.LightText;

            if (item.GetType() == typeof(ToolStripSeparator))
            {
                item.Margin = new Padding(0, 0, 0, 1);
            }
        }

        #endregion

        #region Render Region

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            var g = e.Graphics;
            using (var b = new SolidBrush(Colors.GreyBackground))
            {
                g.FillRectangle(b, e.AffectedBounds);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            var g = e.Graphics;

            var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);

            using (var p = new Pen(Colors.LightBorder))
            {
                g.DrawRectangle(p, rect);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;

            var rect = new Rectangle(e.ImageRectangle.Left - 2, e.ImageRectangle.Top - 2,
                                         e.ImageRectangle.Width + 4, e.ImageRectangle.Height + 4);

            using (var b = new SolidBrush(Colors.LightBorder))
            {
                g.FillRectangle(b, rect);
            }

            using (var p = new Pen(Colors.BlueHighlight))
            {
                var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.DrawRectangle(p, modRect);
            }

            if (e.Item.ImageIndex == -1 && String.IsNullOrEmpty(e.Item.ImageKey) && e.Item.Image == null)
            {
                g.DrawImageUnscaled(MenuIcons.tick, new Point(e.ImageRectangle.Left, e.ImageRectangle.Top));
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var g = e.Graphics;

            var rect = new Rectangle(1, 3, e.Item.Width, 1);

            using (var b = new SolidBrush(Colors.LightBorder))
            {
                g.FillRectangle(b, rect);
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Colors.LightText;
            e.ArrowRectangle = new Rectangle(new Point(e.ArrowRectangle.Left, e.ArrowRectangle.Top - 1), e.ArrowRectangle.Size);

            base.OnRenderArrow(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var g = e.Graphics;

            e.Item.ForeColor = e.Item.Enabled ? Colors.LightText : Colors.DisabledText;

            if (e.Item.Enabled)
            {
                // Normal item
                if (e.Item.Selected)
                {
                    var rect = new Rectangle(2, 0, e.Item.Width - 3, e.Item.Height);

                    using (var b = new SolidBrush(Colors.GreySelection))
                    {
                        g.FillRectangle(b, rect);
                    }
                }

                // Header item on open menu
                if (e.Item.GetType() == typeof(ToolStripMenuItem))
                {
                    if (((ToolStripMenuItem)e.Item).DropDown.Visible && e.Item.IsOnDropDown == false)
                    {
                        var rect = new Rectangle(2, 0, e.Item.Width - 3, e.Item.Height);

                        using (var b = new SolidBrush(Colors.GreySelection))
                        {
                            g.FillRectangle(b, rect);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
