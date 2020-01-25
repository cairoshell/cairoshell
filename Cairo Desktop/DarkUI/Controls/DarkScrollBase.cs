using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public abstract class DarkScrollBase : Control
    {
        #region Event Region

        public event EventHandler ViewportChanged;
        public event EventHandler ContentSizeChanged;

        #endregion

        #region Field Region

        protected readonly DarkScrollBar _vScrollBar;
        protected readonly DarkScrollBar _hScrollBar;

        private Size _visibleSize;
        private Size _contentSize;

        private Rectangle _viewport;

        private Point _offsetMousePosition;

        private int _maxDragChange = 0;
        private Timer _dragTimer;

        #endregion

        #region Property Region


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle Viewport
        {
            get { return _viewport; }
            private set
            {
                _viewport = value;

                if (ViewportChanged != null)
                    ViewportChanged(this, null);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size ContentSize
        {
            get { return _contentSize; }
            set
            {
                _contentSize = value;
                UpdateScrollBars();

                if (ContentSizeChanged != null)
                    ContentSizeChanged(this, null);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point OffsetMousePosition
        {
            get { return _offsetMousePosition; }
        }

        [Category("Behavior")]
        [Description("Determines the maximum scroll change when dragging.")]
        [DefaultValue(0)]
        public int MaxDragChange
        {
            get { return _maxDragChange; }
            set
            {
                _maxDragChange = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDragging { get; private set; }

        #endregion

        #region Constructor Region

        protected DarkScrollBase()
        {
            SetStyle(ControlStyles.Selectable |
                     ControlStyles.UserMouse, true);

            _vScrollBar = new DarkScrollBar { ScrollOrientation = DarkScrollOrientation.Vertical };
            _hScrollBar = new DarkScrollBar { ScrollOrientation = DarkScrollOrientation.Horizontal };

            Controls.Add(_vScrollBar);
            Controls.Add(_hScrollBar);

            _vScrollBar.ValueChanged += delegate { UpdateViewport(); };
            _hScrollBar.ValueChanged += delegate { UpdateViewport(); };

            _vScrollBar.MouseDown += delegate { Select(); };
            _hScrollBar.MouseDown += delegate { Select(); };

            _dragTimer = new Timer();
            _dragTimer.Interval = 1;
            _dragTimer.Tick += DragTimer_Tick;
        }

        #endregion

        #region Method Region

        private void UpdateScrollBars()
        {
            if (_vScrollBar.Maximum != ContentSize.Height)
                _vScrollBar.Maximum = ContentSize.Height;

            if (_hScrollBar.Maximum != ContentSize.Width)
                _hScrollBar.Maximum = ContentSize.Width;

            var scrollSize = Consts.ScrollBarSize;

            _vScrollBar.Location = new Point(ClientSize.Width - scrollSize, 0);
            _vScrollBar.Size = new Size(scrollSize, ClientSize.Height);

            _hScrollBar.Location = new Point(0, ClientSize.Height - scrollSize);
            _hScrollBar.Size = new Size(ClientSize.Width, scrollSize);

            if (DesignMode)
                return;

            // Do this twice in case changing the visibility of the scrollbars
            // causes the VisibleSize to change in such a way as to require a second scrollbar.
            // Probably a better way to detect that scenario...
            SetVisibleSize();
            SetScrollBarVisibility();
            SetVisibleSize();
            SetScrollBarVisibility();

            if (_vScrollBar.Visible)
                _hScrollBar.Width -= scrollSize;

            if (_hScrollBar.Visible)
                _vScrollBar.Height -= scrollSize;

            _vScrollBar.ViewSize = _visibleSize.Height;
            _hScrollBar.ViewSize = _visibleSize.Width;

            UpdateViewport();
        }

        private void SetScrollBarVisibility()
        {
            _vScrollBar.Visible = _visibleSize.Height < ContentSize.Height;
            _hScrollBar.Visible = _visibleSize.Width < ContentSize.Width;
        }

        private void SetVisibleSize()
        {
            var scrollSize = Consts.ScrollBarSize;

            _visibleSize = new Size(ClientSize.Width, ClientSize.Height);

            if (_vScrollBar.Visible)
                _visibleSize.Width -= scrollSize;

            if (_hScrollBar.Visible)
                _visibleSize.Height -= scrollSize;
        }

        private void UpdateViewport()
        {
            var left = 0;
            var top = 0;
            var width = ClientSize.Width;
            var height = ClientSize.Height;

            if (_hScrollBar.Visible)
            {
                left = _hScrollBar.Value;
                height -= _hScrollBar.Height;
            }

            if (_vScrollBar.Visible)
            {
                top = _vScrollBar.Value;
                width -= _vScrollBar.Width;
            }

            Viewport = new Rectangle(left, top, width, height);

            var pos = PointToClient(MousePosition);
            _offsetMousePosition = new Point(pos.X + Viewport.Left, pos.Y + Viewport.Top);

            Invalidate();
        }

        public void ScrollTo(Point point)
        {
            HScrollTo(point.X);
            VScrollTo(point.Y);
        }

        public void VScrollTo(int value)
        {
            if (_vScrollBar.Visible)
                _vScrollBar.Value = value;
        }

        public void HScrollTo(int value)
        {
            if (_hScrollBar.Visible)
                _hScrollBar.Value = value;
        }

        protected virtual void StartDrag()
        {
            IsDragging = true;
            _dragTimer.Start();
        }

        protected virtual void StopDrag()
        {
            IsDragging = false;
            _dragTimer.Stop();
        }

        public Point PointToView(Point point)
        {
            return new Point(point.X - Viewport.Left, point.Y - Viewport.Top);
        }

        public Rectangle RectangleToView(Rectangle rect)
        {
            return new Rectangle(new Point(rect.Left - Viewport.Left, rect.Top - Viewport.Top), rect.Size);
        }

        #endregion

        #region Event Handler Region

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            UpdateScrollBars();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateScrollBars();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            _offsetMousePosition = new Point(e.X + Viewport.Left, e.Y + Viewport.Top);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
                Select();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            var horizontal = false;

            if (_hScrollBar.Visible && ModifierKeys == Keys.Control)
                horizontal = true;

            if (_hScrollBar.Visible && !_vScrollBar.Visible)
                horizontal = true;

            if (!horizontal)
            {
                if (e.Delta > 0)
                    _vScrollBar.ScrollByPhysical(3);
                else if (e.Delta < 0)
                    _vScrollBar.ScrollByPhysical(-3);
            }
            else
            {
                if (e.Delta > 0)
                    _hScrollBar.ScrollByPhysical(3);
                else if (e.Delta < 0)
                    _hScrollBar.ScrollByPhysical(-3);
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            // Allows arrow keys to trigger OnKeyPress
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    e.IsInputKey = true;
                    break;
            }
        }

        private void DragTimer_Tick(object sender, EventArgs e)
        {
            var pos = PointToClient(MousePosition);

            var right = ClientRectangle.Right;
            var bottom = ClientRectangle.Bottom;

            if (_vScrollBar.Visible)
                right = _vScrollBar.Left;

            if (_hScrollBar.Visible)
                bottom = _hScrollBar.Top;

            if (_vScrollBar.Visible)
            {
                // Scroll up
                if (pos.Y < ClientRectangle.Top)
                {
                    var difference = (pos.Y - ClientRectangle.Top) * -1;

                    if (MaxDragChange > 0 && difference > MaxDragChange)
                        difference = MaxDragChange;

                    _vScrollBar.Value = _vScrollBar.Value - difference;
                }

                // Scroll down
                if (pos.Y > bottom)
                {
                    var difference = pos.Y - bottom;

                    if (MaxDragChange > 0 && difference > MaxDragChange)
                        difference = MaxDragChange;

                    _vScrollBar.Value = _vScrollBar.Value + difference;
                }
            }

            if (_hScrollBar.Visible)
            {
                // Scroll left
                if (pos.X < ClientRectangle.Left)
                {
                    var difference = (pos.X - ClientRectangle.Left) * -1;

                    if (MaxDragChange > 0 && difference > MaxDragChange)
                        difference = MaxDragChange;

                    _hScrollBar.Value = _hScrollBar.Value - difference;
                }

                // Scroll right
                if (pos.X > right)
                {
                    var difference = pos.X - right;

                    if (MaxDragChange > 0 && difference > MaxDragChange)
                        difference = MaxDragChange;

                    _hScrollBar.Value = _hScrollBar.Value + difference;
                }
            }
        }

        #endregion
    }
}
