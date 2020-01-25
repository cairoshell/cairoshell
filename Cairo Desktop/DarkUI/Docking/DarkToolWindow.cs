using DarkUI.Config;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;

namespace DarkUI.Docking
{
    [ToolboxItem(false)]
    public class DarkToolWindow : DarkDockContent
    {
        #region Field Region

        private Rectangle _closeButtonRect;
        private bool _closeButtonHot = false;
        private bool _closeButtonPressed = false;

        private Rectangle _headerRect;
        private bool _shouldDrag;

        #endregion

        #region Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get { return base.Padding; }
        }

        #endregion

        #region Constructor Region

        public DarkToolWindow()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            BackColor = Colors.GreyBackground;
            base.Padding = new Padding(0, Consts.ToolWindowHeaderSize, 0, 0);

            UpdateCloseButton();
        }

        #endregion

        #region Method Region

        private bool IsActive()
        {
            if (DockPanel == null)
                return false;

            return DockPanel.ActiveContent == this;
        }

        private void UpdateCloseButton()
        {
            _headerRect = new Rectangle
            {
                X = ClientRectangle.Left,
                Y = ClientRectangle.Top,
                Width = ClientRectangle.Width,
                Height = Consts.ToolWindowHeaderSize
            };

            _closeButtonRect = new Rectangle
            {
                X = ClientRectangle.Right - DockIcons.tw_close.Width - 5 - 3,
                Y = ClientRectangle.Top + (Consts.ToolWindowHeaderSize / 2) - (DockIcons.tw_close.Height / 2),
                Width = DockIcons.tw_close.Width,
                Height = DockIcons.tw_close.Height
            };
        }

        #endregion

        #region Event Handler Region

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateCloseButton();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_closeButtonRect.Contains(e.Location) || _closeButtonPressed)
            {
                if (!_closeButtonHot)
                {
                    _closeButtonHot = true;
                    Invalidate();
                }
            }
            else
            {
                if (_closeButtonHot)
                {
                    _closeButtonHot = false;
                    Invalidate();
                }

                if (_shouldDrag)
                {
                    DockPanel.DragContent(this);
                    return;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_closeButtonRect.Contains(e.Location))
            {
                _closeButtonPressed = true;
                _closeButtonHot = true;
                Invalidate();
                return;
            }

            if (_headerRect.Contains(e.Location))
            {
                _shouldDrag = true;
                return;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_closeButtonRect.Contains(e.Location) && _closeButtonPressed)
                Close();

            _closeButtonPressed = false;
            _closeButtonHot = false;

            _shouldDrag = false;

            Invalidate();
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            // Fill body
            using (var b = new SolidBrush(Colors.GreyBackground))
            {
                g.FillRectangle(b, ClientRectangle);
            }

            var isActive = IsActive();

            // Draw header
            var bgColor = isActive ? Colors.BlueBackground : Colors.HeaderBackground;
            var darkColor = isActive ? Colors.DarkBlueBorder : Colors.DarkBorder;
            var lightColor = isActive ? Colors.LightBlueBorder : Colors.LightBorder;

            using (var b = new SolidBrush(bgColor))
            {
                var bgRect = new Rectangle(0, 0, ClientRectangle.Width, Consts.ToolWindowHeaderSize);
                g.FillRectangle(b, bgRect);
            }

            using (var p = new Pen(darkColor))
            {
                g.DrawLine(p, ClientRectangle.Left, 0, ClientRectangle.Right, 0);
                g.DrawLine(p, ClientRectangle.Left, Consts.ToolWindowHeaderSize - 1, ClientRectangle.Right, Consts.ToolWindowHeaderSize - 1);
            }

            using (var p = new Pen(lightColor))
            {
                g.DrawLine(p, ClientRectangle.Left, 1, ClientRectangle.Right, 1);
            }

            var xOffset = 2;

            // Draw icon
            if (Icon != null)
            {
                g.DrawImageUnscaled(Icon, ClientRectangle.Left + 5, ClientRectangle.Top + (Consts.ToolWindowHeaderSize / 2) - (Icon.Height / 2) + 1);
                xOffset = Icon.Width + 8;
            }

            // Draw text
            using (var b = new SolidBrush(Colors.LightText))
            {
                var textRect = new Rectangle(xOffset, 0, ClientRectangle.Width - 4 - xOffset, Consts.ToolWindowHeaderSize);

                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(DockText, Font, b, textRect, format);
            }

            // Close button
            var img = _closeButtonHot ? DockIcons.tw_close_selected : DockIcons.tw_close;

            if (isActive)
                img = _closeButtonHot ? DockIcons.tw_active_close_selected : DockIcons.tw_active_close;

            g.DrawImageUnscaled(img, _closeButtonRect.Left, _closeButtonRect.Top);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Absorb event
        }

        #endregion
    }
}
