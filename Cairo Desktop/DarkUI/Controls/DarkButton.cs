using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public class DarkButton : Button
    {
        #region Field Region

        private DarkButtonStyle _style = DarkButtonStyle.Normal;
        private DarkControlState _buttonState = DarkControlState.Normal;

        private bool _isDefault;
        private bool _spacePressed;

        private int _padding = Consts.Padding / 2;
        private int _imagePadding = 5; // Consts.Padding / 2

        #endregion

        #region Designer Property Region

        public new string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines the style of the button.")]
        [DefaultValue(DarkButtonStyle.Normal)]
        public DarkButtonStyle ButtonStyle
        {
            get { return _style; }
            set
            {
                _style = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines the amount of padding between the image and text.")]
        [DefaultValue(5)]
        public int ImagePadding
        {
            get { return _imagePadding; }
            set
            {
                _imagePadding = value;
                Invalidate();
            }
        }

        #endregion

        #region Code Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AutoEllipsis
        {
            get { return false; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkControlState ButtonState
        {
            get { return _buttonState; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment ImageAlign
        {
            get { return base.ImageAlign; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool FlatAppearance
        {
            get { return false; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle
        {
            get { return base.FlatStyle; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment TextAlign
        {
            get { return base.TextAlign; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseCompatibleTextRendering
        {
            get { return false; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseVisualStyleBackColor
        {
            get { return false; }
        }

        #endregion

        #region Constructor Region

        public DarkButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            base.UseVisualStyleBackColor = false;
            base.UseCompatibleTextRendering = false;

            SetButtonState(DarkControlState.Normal);
            Padding = new Padding(_padding);
        }

        #endregion

        #region Method Region

        private void SetButtonState(DarkControlState buttonState)
        {
            if (_buttonState != buttonState)
            {
                _buttonState = buttonState;
                Invalidate();
            }
        }

        #endregion

        #region Event Handler Region

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            var form = FindForm();
            if (form != null)
            {
                if (form.AcceptButton == this)
                    _isDefault = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_spacePressed)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (ClientRectangle.Contains(e.Location))
                    SetButtonState(DarkControlState.Pressed);
                else
                    SetButtonState(DarkControlState.Hover);
            }
            else
            {
                SetButtonState(DarkControlState.Hover);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!ClientRectangle.Contains(e.Location))
                return;

            SetButtonState(DarkControlState.Pressed);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_spacePressed)
                return;

            SetButtonState(DarkControlState.Normal);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_spacePressed)
                return;

            SetButtonState(DarkControlState.Normal);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (_spacePressed)
                return;

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetButtonState(DarkControlState.Normal);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            _spacePressed = false;

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetButtonState(DarkControlState.Normal);
            else
                SetButtonState(DarkControlState.Hover);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = true;
                SetButtonState(DarkControlState.Pressed);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = false;

                var location = Cursor.Position;

                if (!ClientRectangle.Contains(location))
                    SetButtonState(DarkControlState.Normal);
                else
                    SetButtonState(DarkControlState.Hover);
            }
        }

        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(value);

            if (!DesignMode)
                return;

            _isDefault = value;
            Invalidate();
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            var textColor = Colors.LightText;
            var borderColor = Colors.GreySelection;
            var fillColor = _isDefault ? Colors.DarkBlueBackground : Colors.LightBackground;

            if (Enabled)
            {
                if (ButtonStyle == DarkButtonStyle.Normal)
                {
                    if (Focused && TabStop)
                        borderColor = Colors.BlueHighlight;

                    switch (ButtonState)
                    {
                        case DarkControlState.Hover:
                            fillColor = _isDefault ? Colors.BlueBackground : Colors.LighterBackground;
                            break;
                        case DarkControlState.Pressed:
                            fillColor = _isDefault ? Colors.DarkBackground : Colors.DarkBackground;
                            break;
                    }
                }
                else if (ButtonStyle == DarkButtonStyle.Flat)
                {
                    switch (ButtonState)
                    {
                        case DarkControlState.Normal:
                            fillColor = Colors.GreyBackground;
                            break;
                        case DarkControlState.Hover:
                            fillColor = Colors.MediumBackground;
                            break;
                        case DarkControlState.Pressed:
                            fillColor = Colors.DarkBackground;
                            break;
                    }
                }
            }
            else
            {
                textColor = Colors.DisabledText;
                fillColor = Colors.DarkGreySelection;
            }

            using (var b = new SolidBrush(fillColor))
            {
                g.FillRectangle(b, rect);
            }

            if (ButtonStyle == DarkButtonStyle.Normal)
            {
                using (var p = new Pen(borderColor, 1))
                {
                    var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);

                    g.DrawRectangle(p, modRect);
                }
            }

            var textOffsetX = 0;
            var textOffsetY = 0;

            if (Image != null)
            {
                var stringSize = g.MeasureString(Text, Font, rect.Size);

                var x = (ClientSize.Width / 2) - (Image.Size.Width / 2);
                var y = (ClientSize.Height / 2) - (Image.Size.Height / 2);

                switch (TextImageRelation)
                {
                    case TextImageRelation.ImageAboveText:
                        textOffsetY = (Image.Size.Height / 2) + (ImagePadding / 2);
                        y = y - ((int)(stringSize.Height / 2) + (ImagePadding / 2));
                        break;
                    case TextImageRelation.TextAboveImage:
                        textOffsetY = ((Image.Size.Height / 2) + (ImagePadding / 2)) * -1;
                        y = y + ((int)(stringSize.Height / 2) + (ImagePadding / 2));
                        break;
                    case TextImageRelation.ImageBeforeText:
                        textOffsetX = Image.Size.Width + (ImagePadding * 2);
                        x = ImagePadding;
                        break;
                    case TextImageRelation.TextBeforeImage:
                        x = x + (int)stringSize.Width;
                        break;
                }

                g.DrawImageUnscaled(Image, x, y);
            }

            using (var b = new SolidBrush(textColor))
            {
                var modRect = new Rectangle(rect.Left + textOffsetX + Padding.Left,
                                            rect.Top + textOffsetY + Padding.Top, rect.Width - Padding.Horizontal,
                                            rect.Height - Padding.Vertical);

                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(Text, Font, b, modRect, stringFormat);
            }
        }

        #endregion
    }
}
