using DarkUI.Docking;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Win32
{
    public class DockResizeFilter : IMessageFilter
    {
        #region Field Region

        private DarkDockPanel _dockPanel;

        private Timer _dragTimer;
        private bool _isDragging;
        private Point _initialContact;
        private DarkDockSplitter _activeSplitter;

        #endregion

        #region Constructor Region

        public DockResizeFilter(DarkDockPanel dockPanel)
        {
            _dockPanel = dockPanel;

            _dragTimer = new Timer();
            _dragTimer.Interval = 1;
            _dragTimer.Tick += DragTimer_Tick;
        }

        #endregion

        #region IMessageFilter Region

        public bool PreFilterMessage(ref Message m)
        {
            // We only care about mouse events
            if (!(m.Msg == (int)WM.MOUSEMOVE ||
                  m.Msg == (int)WM.LBUTTONDOWN || m.Msg == (int)WM.LBUTTONUP || m.Msg == (int)WM.LBUTTONDBLCLK ||
                  m.Msg == (int)WM.RBUTTONDOWN || m.Msg == (int)WM.RBUTTONUP || m.Msg == (int)WM.RBUTTONDBLCLK))
                return false;

            // Stop drag.
            if (m.Msg == (int)WM.LBUTTONUP)
            {
                if (_isDragging)
                {
                    StopDrag();
                    return true;
                }
            }

            // Exit out early if we're simply releasing a non-splitter drag over the area
            if (m.Msg == (int)WM.LBUTTONUP && !_isDragging)
                return false;

            // Force cursor if already dragging.
            if (_isDragging)
                Cursor.Current = _activeSplitter.ResizeCursor;

            // Return out early if we're dragging something that's not a splitter.
            if (m.Msg == (int)WM.MOUSEMOVE && !_isDragging && _dockPanel.MouseButtonState != MouseButtons.None)
                return false;

            // Try and create a control from the message handle.
            var control = Control.FromHandle(m.HWnd);

            // Exit out if we didn't manage to create a control.
            if (control == null)
                return false;

            // Exit out if the control is not the dock panel or a child control.
            if (!(control == _dockPanel || _dockPanel.Contains(control)))
                return false;

            // Update the mouse cursor
            CheckCursor();

            // Start drag.
            if (m.Msg == (int)WM.LBUTTONDOWN)
            {
                var hotSplitter = HotSplitter();
                if (hotSplitter != null)
                {
                    StartDrag(hotSplitter);
                    return true;
                }
            }

            // Stop events passing through if we're hovering over a splitter
            if (HotSplitter() != null)
                return true;

            // Stop all events from going through if we're dragging a splitter.
            if (_isDragging)
                return true;

            return false;
        }

        #endregion

        #region Event Handler Region

        private void DragTimer_Tick(object sender, EventArgs e)
        {
            if (_dockPanel.MouseButtonState != MouseButtons.Left)
            {
                StopDrag();
                return;
            }

            var difference = new Point(_initialContact.X - Cursor.Position.X, _initialContact.Y - Cursor.Position.Y);
            _activeSplitter.UpdateOverlay(difference);
        }

        #endregion

        #region Method Region

        private void StartDrag(DarkDockSplitter splitter)
        {
            _activeSplitter = splitter;
            Cursor.Current = _activeSplitter.ResizeCursor;

            _initialContact = Cursor.Position;
            _isDragging = true;

            _activeSplitter.ShowOverlay();
            _dragTimer.Start();
        }

        private void StopDrag()
        {
            _dragTimer.Stop();
            _activeSplitter.HideOverlay();

            var difference = new Point(_initialContact.X - Cursor.Position.X, _initialContact.Y - Cursor.Position.Y);
            _activeSplitter.Move(difference);

            _isDragging = false;
        }

        private DarkDockSplitter HotSplitter()
        {
            foreach (var splitter in _dockPanel.Splitters)
            {
                if (splitter.Bounds.Contains(Cursor.Position))
                    return splitter;
            }

            return null;
        }

        private void CheckCursor()
        {
            if (_isDragging)
                return;

            var hotSplitter = HotSplitter();
            if (hotSplitter != null)
                Cursor.Current = hotSplitter.ResizeCursor;
        }

        private void ResetCursor()
        {
            Cursor.Current = Cursors.Default;
            CheckCursor();
        }

        #endregion
    }
}