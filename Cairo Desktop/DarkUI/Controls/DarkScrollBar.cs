using DarkUI.Config;
using DarkUI.Icons;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkScrollBar : Control
    {
        #region Event Region

        public event EventHandler<ScrollValueEventArgs> ValueChanged;

        #endregion

        #region Field Region

        private DarkScrollOrientation _scrollOrientation;

        private int _value;
        private int _minimum = 0;
        private int _maximum = 100;

        private int _viewSize;

        private Rectangle _trackArea;
        private float _viewContentRatio;

        private Rectangle _thumbArea;
        private Rectangle _upArrowArea;
        private Rectangle _downArrowArea;

        private bool _thumbHot;
        private bool _upArrowHot;
        private bool _downArrowHot;

        private bool _thumbClicked;
        private bool _upArrowClicked;
        private bool _downArrowClicked;

        private bool _isScrolling;
        private int _initialValue;
        private Point _initialContact;

        private Timer _scrollTimer;

        #endregion

        #region Property Region

        [Category("Behavior")]
        [Description("The orientation type of the scrollbar.")]
        [DefaultValue(DarkScrollOrientation.Vertical)]
        public DarkScrollOrientation ScrollOrientation
        {
            get { return _scrollOrientation; }
            set
            {
                _scrollOrientation = value;
                UpdateScrollBar();
            }
        }

        [Category("Behavior")]
        [Description("The value that the scroll thumb position represents.")]
        [DefaultValue(0)]
        public int Value
        {
            get { return _value; }
            set
            {
                if (value < Minimum)
                    value = Minimum;

                var maximumValue = Maximum - ViewSize;
                if (value > maximumValue)
                    value = maximumValue;

                if (_value == value)
                    return;

                _value = value;

                UpdateThumb(true);

                if (ValueChanged != null)
                    ValueChanged(this, new ScrollValueEventArgs(Value));
            }
        }

        [Category("Behavior")]
        [Description("The lower limit value of the scrollable range.")]
        [DefaultValue(0)]
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                UpdateScrollBar();
            }
        }

        [Category("Behavior")]
        [Description("The upper limit value of the scrollable range.")]
        [DefaultValue(100)]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                UpdateScrollBar();
            }
        }

        [Category("Behavior")]
        [Description("The view size for the scrollable area.")]
        [DefaultValue(0)]
        public int ViewSize
        {
            get { return _viewSize; }
            set
            {
                _viewSize = value;
                UpdateScrollBar();
            }
        }

        public new bool Visible
        {
            get { return base.Visible; }
            set
            {
                if (base.Visible == value)
                    return;

                base.Visible = value;
            }
        }

        #endregion

        #region Constructor Region

        public DarkScrollBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            SetStyle(ControlStyles.Selectable, false);

            _scrollTimer = new Timer();
            _scrollTimer.Interval = 1;
            _scrollTimer.Tick += ScrollTimerTick;
        }

        #endregion

        #region Event Handler Region

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateScrollBar();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_thumbArea.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                _isScrolling = true;
                _initialContact = e.Location;

                if (_scrollOrientation == DarkScrollOrientation.Vertical)
                    _initialValue = _thumbArea.Top;
                else
                    _initialValue = _thumbArea.Left;

                Invalidate();
                return;
            }

            if (_upArrowArea.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                _upArrowClicked = true;
                _scrollTimer.Enabled = true;

                Invalidate();
                return;
            }

            if (_downArrowArea.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                _downArrowClicked = true;
                _scrollTimer.Enabled = true;

                Invalidate();
                return;
            }

            if (_trackArea.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                // Step 1. Check if our input is at least aligned with the thumb
                if (_scrollOrientation == DarkScrollOrientation.Vertical)
                {
                    var modRect = new Rectangle(_thumbArea.Left, _trackArea.Top, _thumbArea.Width, _trackArea.Height);
                    if (!modRect.Contains(e.Location))
                        return;
                }
                else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
                {
                    var modRect = new Rectangle(_trackArea.Left, _thumbArea.Top, _trackArea.Width, _thumbArea.Height);
                    if (!modRect.Contains(e.Location))
                        return;
                }

                // Step 2. Scroll to the area initially clicked.
                if (_scrollOrientation == DarkScrollOrientation.Vertical)
                {
                    var loc = e.Location.Y;
                    loc -= _upArrowArea.Bottom - 1;
                    loc -= _thumbArea.Height / 2;
                    ScrollToPhysical(loc);
                }
                else
                {
                    var loc = e.Location.X;
                    loc -= _upArrowArea.Right - 1;
                    loc -= _thumbArea.Width / 2;
                    ScrollToPhysical(loc);
                }

                // Step 3. Initiate a thumb drag.
                _isScrolling = true;
                _initialContact = e.Location;
                _thumbHot = true;

                if (_scrollOrientation == DarkScrollOrientation.Vertical)
                    _initialValue = _thumbArea.Top;
                else
                    _initialValue = _thumbArea.Left;

                Invalidate();
                return;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isScrolling = false;

            _thumbClicked = false;
            _upArrowClicked = false;
            _downArrowClicked = false;

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_isScrolling)
            {
                var thumbHot = _thumbArea.Contains(e.Location);
                if (_thumbHot != thumbHot)
                {
                    _thumbHot = thumbHot;
                    Invalidate();
                }

                var upArrowHot = _upArrowArea.Contains(e.Location);
                if (_upArrowHot != upArrowHot)
                {
                    _upArrowHot = upArrowHot;
                    Invalidate();
                }

                var downArrowHot = _downArrowArea.Contains(e.Location);
                if (_downArrowHot != downArrowHot)
                {
                    _downArrowHot = downArrowHot;
                    Invalidate();
                }
            }

            if (_isScrolling)
            {
                if (e.Button != MouseButtons.Left)
                {
                    OnMouseUp(null);
                    return;
                }

                var difference = new Point(e.Location.X - _initialContact.X, e.Location.Y - _initialContact.Y);

                if (_scrollOrientation == DarkScrollOrientation.Vertical)
                {
                    var thumbPos = (_initialValue - _trackArea.Top);
                    var newPosition = thumbPos + difference.Y;

                    ScrollToPhysical(newPosition);
                }
                else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
                {
                    var thumbPos = (_initialValue - _trackArea.Left);
                    var newPosition = thumbPos + difference.X;

                    ScrollToPhysical(newPosition);
                }

                UpdateScrollBar();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _thumbHot = false;
            _upArrowHot = false;
            _downArrowHot = false;

            Invalidate();
        }

        private void ScrollTimerTick(object sender, EventArgs e)
        {
            if (!_upArrowClicked && !_downArrowClicked)
            {
                _scrollTimer.Enabled = false;
                return;
            }

            if (_upArrowClicked)
                ScrollBy(-1);
            else if (_downArrowClicked)
                ScrollBy(1);
        }

        #endregion

        #region Method Region

        public void ScrollTo(int position)
        {
            Value = position;
        }

        public void ScrollToPhysical(int positionInPixels)
        {
            var isVert = _scrollOrientation == DarkScrollOrientation.Vertical;

            var trackAreaSize = isVert ? _trackArea.Height - _thumbArea.Height : _trackArea.Width - _thumbArea.Width;

            var positionRatio = (float)positionInPixels / (float)trackAreaSize;
            var viewScrollSize = (Maximum - ViewSize);

            var newValue = (int)(positionRatio * viewScrollSize);
            Value = newValue;
        }

        public void ScrollBy(int offset)
        {
            var newValue = Value + offset;
            ScrollTo(newValue);
        }

        public void ScrollByPhysical(int offsetInPixels)
        {
            var isVert = _scrollOrientation == DarkScrollOrientation.Vertical;

            var thumbPos = isVert ? (_thumbArea.Top - _trackArea.Top) : (_thumbArea.Left - _trackArea.Left);

            var newPosition = thumbPos - offsetInPixels;

            ScrollToPhysical(newPosition);
        }

        public void UpdateScrollBar()
        {
            if (ViewSize >= Maximum)
                return;

            var area = ClientRectangle;

            // Cap to maximum value
            var maximumValue = Maximum - ViewSize;
            if (Value > maximumValue)
                Value = maximumValue;

            // Arrow buttons
            if (_scrollOrientation == DarkScrollOrientation.Vertical)
            {
                _upArrowArea = new Rectangle(area.Left, area.Top, Consts.ArrowButtonSize, Consts.ArrowButtonSize);
                _downArrowArea = new Rectangle(area.Left, area.Bottom - Consts.ArrowButtonSize, Consts.ArrowButtonSize, Consts.ArrowButtonSize);
            }
            else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
            {
                _upArrowArea = new Rectangle(area.Left, area.Top, Consts.ArrowButtonSize, Consts.ArrowButtonSize);
                _downArrowArea = new Rectangle(area.Right - Consts.ArrowButtonSize, area.Top, Consts.ArrowButtonSize, Consts.ArrowButtonSize);
            }

            // Track
            if (_scrollOrientation == DarkScrollOrientation.Vertical)
            {
                _trackArea = new Rectangle(area.Left, area.Top + Consts.ArrowButtonSize, area.Width, area.Height - (Consts.ArrowButtonSize * 2));
            }
            else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
            {
                _trackArea = new Rectangle(area.Left + Consts.ArrowButtonSize, area.Top, area.Width - (Consts.ArrowButtonSize * 2), area.Height);
            }

            // Thumb
            UpdateThumb();

            Invalidate();
        }

        private void UpdateThumb(bool forceRefresh = false)
        {
            // Calculate size ratio
            _viewContentRatio = (float)ViewSize / (float)Maximum;
            var viewAreaSize = Maximum - ViewSize;
            var positionRatio = (float)Value / (float)viewAreaSize;

            // Update area
            if (_scrollOrientation == DarkScrollOrientation.Vertical)
            {
                var thumbSize = (int)(_trackArea.Height * _viewContentRatio);

                if (thumbSize < Consts.MinimumThumbSize)
                    thumbSize = Consts.MinimumThumbSize;

                var trackAreaSize = _trackArea.Height - thumbSize;
                var thumbPosition = (int)(trackAreaSize * positionRatio);

                _thumbArea = new Rectangle(_trackArea.Left + 3, _trackArea.Top + thumbPosition, Consts.ScrollBarSize - 6, thumbSize);
            }
            else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
            {
                var thumbSize = (int)(_trackArea.Width * _viewContentRatio);

                if (thumbSize < Consts.MinimumThumbSize)
                    thumbSize = Consts.MinimumThumbSize;

                var trackAreaSize = _trackArea.Width - thumbSize;
                var thumbPosition = (int)(trackAreaSize * positionRatio);

                _thumbArea = new Rectangle(_trackArea.Left + thumbPosition, _trackArea.Top + 3, thumbSize, Consts.ScrollBarSize - 6);
            }

            if (forceRefresh)
            {
                Invalidate();
                Update();
            }
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            // DEBUG: Scrollbar bg
            /*using (var b = new SolidBrush(Colors.MediumBackground))
            {
                g.FillRectangle(b, ClientRectangle);
            }*/

            // DEBUG: Arrow backgrounds
            /*using (var b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, _upArrowArea);
                g.FillRectangle(b, _downArrowArea);
            }*/

            // Up arrow
            var upIcon = _upArrowHot ? ScrollIcons.scrollbar_arrow_hot : ScrollIcons.scrollbar_arrow_standard;

            if (_upArrowClicked)
                upIcon = ScrollIcons.scrollbar_arrow_clicked;

            if (_scrollOrientation == DarkScrollOrientation.Vertical)
                upIcon.RotateFlip(RotateFlipType.RotateNoneFlipY);
            else if (_scrollOrientation == DarkScrollOrientation.Horizontal)
                upIcon.RotateFlip(RotateFlipType.Rotate90FlipNone);

            g.DrawImageUnscaled(upIcon,
                                _upArrowArea.Left + (_upArrowArea.Width / 2) - (upIcon.Width / 2),
                                _upArrowArea.Top + (_upArrowArea.Height / 2) - (upIcon.Height / 2));

            // Down arrow
            var downIcon = _downArrowHot ? ScrollIcons.scrollbar_arrow_hot : ScrollIcons.scrollbar_arrow_standard;

            if (_downArrowClicked)
                downIcon = ScrollIcons.scrollbar_arrow_clicked;

            if (_scrollOrientation == DarkScrollOrientation.Horizontal)
                downIcon.RotateFlip(RotateFlipType.Rotate270FlipNone);

            g.DrawImageUnscaled(downIcon,
                                _downArrowArea.Left + (_downArrowArea.Width / 2) - (downIcon.Width / 2),
                                _downArrowArea.Top + (_downArrowArea.Height / 2) - (downIcon.Height / 2));

            // Draw thumb
            var scrollColor = _thumbHot ? Colors.GreyHighlight : Colors.GreySelection;

            if (_isScrolling)
                scrollColor = Colors.ActiveControl;

            using (var b = new SolidBrush(scrollColor))
            {
                g.FillRectangle(b, _thumbArea);
            }
        }

        #endregion
    }
}
