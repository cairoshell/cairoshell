// This file is part of CoderLine NeoGeniX.Skinning.
//
// CoderLine NeoGeniX.Skinning is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// CoderLine NeoGeniX.Skinning is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with CoderLine NeoGeniX.Skinning.  If not, see <http://www.gnu.org/licenses/>.
//
// (C) 2010 Daniel Kuschny, (http://www.coderline.net)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using NeoGeniX.Skinning.Painting;
using NeoGeniX.Skinning.Win32;

namespace NeoGeniX.Skinning
{
    /// <summary>
    /// This NativeWindow implementation processes the window messages
    /// of a given Form to enable the skinning.
    /// </summary>
    public class SkinningForm : NativeWindow
    {
        #region Fields

        // form data
        private Form _parentForm;
        private readonly List<CaptionButton> _captionButtons;
        private bool _formIsActive;

        // graphics data
        private readonly BufferedGraphicsContext _bufferContext;
        private BufferedGraphics _bufferGraphics;
        private Size _currentCacheSize;

        // used for state resetting 
        private CaptionButton _pressedButton;
        private CaptionButton _hoveredButton;

        private SkinningManager _manager;


        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether whe should process the nc area.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if we should process the nc area; otherwise, <c>false</c>.
        /// </value>
        private bool IsProcessNcArea
        {
            get
            {
                // check if we should process the nc area
                return
                    !(_parentForm == null ||
                      _parentForm.MdiParent != null && _parentForm.WindowState == FormWindowState.Maximized);
            }
        }

        #endregion

        #region Constructor and Destructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SkinningForm"/> class.
        /// </summary>
        /// <param name="parentForm">The parent form.</param>
        /// <param name="manager">The manager.</param>
        public SkinningForm(Form parentForm, SkinningManager manager)
        {
            _manager = manager;
            _parentForm = parentForm;
            _captionButtons = new List<CaptionButton>();

            _bufferContext = BufferedGraphicsManager.Current;
            _bufferGraphics = null;

            if (parentForm.Handle != IntPtr.Zero)
                OnHandleCreated(parentForm, EventArgs.Empty);

            RegisterEventHandlers();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SkinningForm"/> is reclaimed by garbage collection.
        /// </summary>
        ~SkinningForm()
        {
            UnregisterEventHandlers();
        }
        #endregion

        #region Parent Form Handlers

        /// <summary>
        /// Registers all important eventhandlers.
        /// </summary>
        private void RegisterEventHandlers()
        {
            _parentForm.HandleCreated += OnHandleCreated;
            _parentForm.HandleDestroyed += OnHandleDestroyed;
            _parentForm.TextChanged += OnTextChanged;
            _parentForm.Disposed += OnParentDisposed;
        }

        /// <summary>
        /// Unregisters all important eventhandlers.
        /// </summary>
        private void UnregisterEventHandlers()
        {
            _parentForm.HandleCreated -= OnHandleCreated;
            _parentForm.HandleDestroyed -= OnHandleDestroyed;
            _parentForm.TextChanged -= OnTextChanged;
            _parentForm.Disposed -= OnParentDisposed;
        }

        /// <summary>
        /// Called when the handle of the parent form is created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnHandleCreated(object sender, EventArgs e)
        {
            // this little line allows us to handle the windowMessages of the parent form in this class
            AssignHandle(((Form)sender).Handle);
            if (IsProcessNcArea)
            {
                UpdateStyle();
                UpdateCaption();
            }
        }

        /// <summary>
        /// Called when the handle of the parent form is destroyed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnHandleDestroyed(object sender, EventArgs e)
        {
            // release handle as it is destroyed
            ReleaseHandle();
        }

        /// <summary>
        /// Called when the parent of the parent form is disposed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnParentDisposed(object sender, EventArgs e)
        {
            // unregister events as the parent of the form is disposed
            if (_parentForm != null)
                UnregisterEventHandlers();
            _parentForm = null;
        }

        /// <summary>
        /// Called when the text on the parent form has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnTextChanged(object sender, EventArgs e)
        {
            // Redraw on title change
            if (IsProcessNcArea)
                NcPaint(true);
        }

        #endregion

        #region Skinning Executors

        /// <summary>
        /// Invokes the default window procedure associated with this window.
        /// </summary>
        /// <param name="m">A <see cref="T:System.Windows.Forms.Message"/> that is associated with the current Windows message.</param>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            bool supressOriginalMessage = false;
            if (IsProcessNcArea)
                switch ((Win32Messages)m.Msg)
                {
                    // update form data on style change
                    case Win32Messages.STYLECHANGED:
                        UpdateStyle();
                        _manager.CurrentSkin.OnSetRegion(_parentForm, _parentForm.Size);
                        break;

                    #region Handle Form Activation
                    case Win32Messages.ACTIVATEAPP:
                        // redraw
                        _formIsActive = (int) m.WParam != 0;
                        NcPaint(true);
                        break;

                    case Win32Messages.ACTIVATE:
                        // Set active state and redraw
                        _formIsActive = ((int)WAFlags.WA_ACTIVE == (int)m.WParam || (int)WAFlags.WA_CLICKACTIVE == (int)m.WParam);
                        NcPaint(true);
                        break;
                    case Win32Messages.MDIACTIVATE:
                        // set active and redraw on activation 
                        if (m.WParam == _parentForm.Handle)
                            _formIsActive = false;
                        else if (m.LParam == _parentForm.Handle)
                            _formIsActive = true;
                        NcPaint(true);
                        break;
                    #endregion

                    #region Handle Mouse Processing
                    // Set Pressed button on mousedown
                    case Win32Messages.NCLBUTTONDOWN:
                        supressOriginalMessage = OnNcLButtonDown(ref m);
                        break;
                    // Set hovered button on mousemove
                    case Win32Messages.NCMOUSEMOVE:
                        OnNcMouseMove(m);
                        break;
                    // perform button actions if a button was clicked
                    case Win32Messages.NCLBUTTONUP:
                        // Handle button up
                        if (OnNcLButtonUp(m))
                            supressOriginalMessage = true;
                        break;
                    // restore button states on mouseleave
                    case Win32Messages.NCMOUSELEAVE:
                    case Win32Messages.MOUSELEAVE:
                    case Win32Messages.MOUSEHOVER:
                        if (_pressedButton != null)
                            _pressedButton.Pressed = false;
                        if (_hoveredButton != null)
                        {
                            _hoveredButton.Hovered = false;
                            _hoveredButton = null;
                        }
                        NcPaint(true);
                        break;
                    #endregion

                    #region Size Processing

                    // Set region as window is shown                    
                    case Win32Messages.SHOWWINDOW:
                        _manager.CurrentSkin.OnSetRegion(_parentForm, _parentForm.Size);
                        break;
                    // adjust region on resize
                    case Win32Messages.SIZE:
                        OnSize(m);
                        break;
                    // ensure that the window doesn't overlap docked toolbars on desktop (like taskbar)
                    case Win32Messages.GETMINMAXINFO:
                        supressOriginalMessage = CalculateMaxSize(ref m);
                        break;
                    // update region on resize
                    case Win32Messages.WINDOWPOSCHANGING:
                        WINDOWPOS wndPos = (WINDOWPOS)m.GetLParam(typeof(WINDOWPOS));
                        if ((wndPos.flags & (int)SWPFlags.SWP_NOSIZE) == 0)
                        {
                            _manager.CurrentSkin.OnSetRegion(_parentForm, new Size(wndPos.cx, wndPos.cy));
                        }
                        break;
                    // remove region on maximize or repaint on resize
                    case Win32Messages.WINDOWPOSCHANGED:
                        if (_parentForm.WindowState == FormWindowState.Maximized)
                            _parentForm.Region = null;

                        WINDOWPOS wndPos2 = (WINDOWPOS)m.GetLParam(typeof(WINDOWPOS));
                        if ((wndPos2.flags & (int)SWPFlags.SWP_NOSIZE) == 0)
                        {
                            UpdateCaption();
                            NcPaint(true);
                        }
                        break;
                    #endregion

                    #region Non Client Area Handling
                    // paint the non client area
                    case Win32Messages.NCPAINT:
                        if (NcPaint(true))
                        {
                            m.Result = (IntPtr)1;
                            supressOriginalMessage = true;
                        }
                        break;
                    // calculate the non client area size
                    case Win32Messages.NCCALCSIZE:
                        if (m.WParam == (IntPtr)1)
                        {
                            if (_parentForm.MdiParent != null)
                                break;
                            // add caption height to non client area
                            NCCALCSIZE_PARAMS p = (NCCALCSIZE_PARAMS)m.GetLParam(typeof(NCCALCSIZE_PARAMS));
                            p.rect0.Top += FormExtenders.GetCaptionHeight(_parentForm);
                            Marshal.StructureToPtr(p, m.LParam, true);
                        }
                        break;
                    // non client hit test
                    case Win32Messages.NCHITTEST:
                        if (NcHitTest(ref m))
                            supressOriginalMessage = true;
                        break;
                    #endregion
                }

            if (!supressOriginalMessage)
                base.WndProc(ref m);
        }

        /// <summary>
        /// Handles the window sizing
        /// </summary>
        /// <param name="m">The m.</param>
        private void OnSize(Message m)
        {
            UpdateCaption();
            // update form styles on maximize/restore
            if (_parentForm.MdiParent != null)
            {
                if ((int)m.WParam == 0)
                    UpdateStyle();
                if ((int)m.WParam == 2)
                    _parentForm.Refresh();
            }

            // update region if needed
            bool wasMaxMin = (_parentForm.WindowState == FormWindowState.Maximized ||
                _parentForm.WindowState == FormWindowState.Minimized);

            RECT rect1 = new RECT();
            Win32Api.GetWindowRect(_parentForm.Handle, ref rect1);

            Rectangle rc = new Rectangle(rect1.Left, rect1.Top, rect1.Right - rect1.Left, rect1.Bottom - rect1.Top - 1);


            if (wasMaxMin && _parentForm.WindowState == FormWindowState.Normal &&
                rc.Size == _parentForm.RestoreBounds.Size)
            {
                _manager.CurrentSkin.OnSetRegion(_parentForm, new Size(rect1.Right - rect1.Left, rect1.Bottom - rect1.Top));
                NcPaint(true);
            }
        }

        /// <summary>
        /// Handles the mouse move event on the non client area.
        /// </summary>
        /// <param name="m">The m.</param>
        private void OnNcMouseMove(Message m)
        {
            // Check for hovered and pressed buttons
            if (Control.MouseButtons != MouseButtons.Left)
            {
                if (_pressedButton != null)
                {
                    _pressedButton.Pressed = false;
                    _pressedButton = null;
                }
            }
            CaptionButton button = CommandButtonByHitTest((HitTest)m.WParam);

            if (_hoveredButton != button && _hoveredButton != null)
                _hoveredButton.Hovered = false;
            if (_pressedButton == null)
            {
                if (button != null)
                    button.Hovered = true;
                _hoveredButton = button;
            }
            else
                _pressedButton.Pressed = (button == _pressedButton);
        }

        /// <summary>
        /// Handle a left mouse button down event.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        private bool OnNcLButtonDown(ref Message m)
        {
            CaptionButton button = CommandButtonByHitTest((HitTest)m.WParam);
            if (_pressedButton != button && _pressedButton != null)
                _pressedButton.Pressed = false;
            if (button != null)
                button.Pressed = true;
            _pressedButton = button;
            if (_pressedButton != null)
            {
                m.Result = (IntPtr)1;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ensure that the window doesn't overlap docked toolbars on desktop (like taskbar)
        /// </summary>
        /// <param name="m">The message.</param>
        /// <returns></returns>
        private bool CalculateMaxSize(ref Message m)
        {
            if (_parentForm.Parent == null)
            {
                // create minMax info for maximize data
                MINMAXINFO info = (MINMAXINFO)m.GetLParam(typeof(MINMAXINFO));
                Rectangle rect = SystemInformation.WorkingArea;
                Size fullBorderSize = new Size(SystemInformation.Border3DSize.Width + SystemInformation.BorderSize.Width,
                    SystemInformation.Border3DSize.Height + SystemInformation.BorderSize.Height);

                info.ptMaxPosition.x = rect.Left - fullBorderSize.Width;
                info.ptMaxPosition.y = rect.Top - fullBorderSize.Height;
                info.ptMaxSize.x = rect.Width + fullBorderSize.Width * 2;
                info.ptMaxSize.y = rect.Height + fullBorderSize.Height * 2;

                info.ptMinTrackSize.y += FormExtenders.GetCaptionHeight(_parentForm);


                if (!_parentForm.MaximumSize.IsEmpty)
                {
                    info.ptMaxSize.x = Math.Min(info.ptMaxSize.x, _parentForm.MaximumSize.Width);
                    info.ptMaxSize.y = Math.Min(info.ptMaxSize.y, _parentForm.MaximumSize.Height);
                    info.ptMaxTrackSize.x = Math.Min(info.ptMaxTrackSize.x, _parentForm.MaximumSize.Width);
                    info.ptMaxTrackSize.y = Math.Min(info.ptMaxTrackSize.y, _parentForm.MaximumSize.Height);
                }

                if (!_parentForm.MinimumSize.IsEmpty)
                {
                    info.ptMinTrackSize.x = Math.Max(info.ptMinTrackSize.x, _parentForm.MinimumSize.Width);
                    info.ptMinTrackSize.y = Math.Max(info.ptMinTrackSize.y, _parentForm.MinimumSize.Height);
                }

                // set wished maximize size
                Marshal.StructureToPtr(info, m.LParam, true);

                m.Result = (IntPtr)0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the window style for the parent form.
        /// </summary>
        private void UpdateStyle()
        {
            // remove the border style
            Int32 currentStyle = Win32Api.GetWindowLong(Handle, GWLIndex.GWL_STYLE);
            if ((currentStyle & (int)(WindowStyles.WS_BORDER)) != 0)
            {
                currentStyle &= ~(int) (WindowStyles.WS_BORDER);
                Win32Api.SetWindowLong(_parentForm.Handle, GWLIndex.GWL_STYLE, currentStyle);
                Win32Api.SetWindowPos(_parentForm.Handle, (IntPtr) 0, -1, -1, -1, -1,
                                      (int) (SWPFlags.SWP_NOZORDER | SWPFlags.SWP_NOSIZE | SWPFlags.SWP_NOMOVE |
                                             SWPFlags.SWP_FRAMECHANGED | SWPFlags.SWP_NOREDRAW | SWPFlags.SWP_NOACTIVATE));
            }
        }


        /// <summary>
        /// Updates the caption.
        /// </summary>
        private void UpdateCaption()
        {
            // create buttons
            if (_captionButtons.Count == 0)
            {
                _captionButtons.Add(new CaptionButton(HitTest.HTCLOSE));
                if (FormExtenders.IsDrawMaximizeBox(_parentForm))
                {
                    CaptionButton button = new CaptionButton(HitTest.HTMAXBUTTON);
                    _captionButtons.Add(button);
                }
                if (FormExtenders.IsDrawMinimizeBox(_parentForm))
                {
                    CaptionButton button = new CaptionButton(HitTest.HTMINBUTTON);
                    _captionButtons.Add(button);
                }

                // add command handlers
                foreach (CaptionButton button in _captionButtons)
                    button.PropertyChanged += OnCommandButtonPropertyChanged;
            }

            // Calculate Caption Button Bounds
            RECT rectScreen = new RECT();
            Win32Api.GetWindowRect(_parentForm.Handle, ref rectScreen);
            Rectangle rect = rectScreen.ToRectangle();

            Size borderSize = FormExtenders.GetBorderSize(_parentForm);
            rect.Offset(-rect.Left, -rect.Top);

            Size captionButtonSize = FormExtenders.GetCaptionButtonSize(_parentForm);
            Rectangle buttonRect = new Rectangle(rect.Right - borderSize.Width - captionButtonSize.Width, rect.Top + borderSize.Height,
                                    captionButtonSize.Width, captionButtonSize.Height);

            foreach (CaptionButton button in _captionButtons)
            {
                button.Bounds = buttonRect;
                buttonRect.X -= captionButtonSize.Width;
            }
        }

        /// <summary>
        /// Called when a property of a CommandButton has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnCommandButtonPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // if a button is hovered or pressed invalidate
            if (e.PropertyName == "Pressed" || e.PropertyName == "Hovered")
                NcPaint(true);
        }

        /// <summary>
        /// Gets the command button at the specified position.
        /// </summary>
        /// <param name="point">The position.</param>
        /// <returns>the CaptionButton instance or null if no button was found.</returns>
        private CaptionButton CommandButtonFromPoint(Point point)
        {
            foreach (CaptionButton button in _captionButtons)
                if (button.Bounds.Contains(point)) return button;
            return null;
        }

        /// <summary>
        /// Gets the command button with the specified HitTest.
        /// </summary>
        /// <param name="hitTest">The hitTest.</param>
        /// <returns>the CaptionButton instance or null if no button was found.</returns>
        private CaptionButton CommandButtonByHitTest(HitTest hitTest)
        {
            foreach (CaptionButton button in _captionButtons)
                if (button.HitTest == hitTest)
                    return button;
            return null;
        }


        /// <summary>
        /// Redraws the non client area..
        /// </summary>
        public void Invalidate()
        {
            if (IsProcessNcArea)
                NcPaint(true);
        }

        /// <summary>
        /// Redraws the non client area.
        /// </summary>
        /// <param name="invalidateBuffer">if set to <c>true</c> the buffer is invalidated.</param>
        /// <returns>true if the original painting should be suppressed otherwise false.</returns>
        private bool NcPaint(bool invalidateBuffer)
        {
            if (!IsProcessNcArea)
                return false;
            bool result = false;

            IntPtr hdc = (IntPtr)0;
            Graphics g = null;
            Region region = null;
            IntPtr hrgn = (IntPtr)0;

            try
            {
                // no drawing needed
                if (_parentForm.MdiParent != null && _parentForm.WindowState == FormWindowState.Maximized)
                {
                    _currentCacheSize = Size.Empty;
                    return false;
                }

                // prepare image bounds
                Size borderSize = FormExtenders.GetBorderSize(_parentForm);
                int captionHeight = FormExtenders.GetCaptionHeight(_parentForm);

                RECT rectScreen = new RECT();
                Win32Api.GetWindowRect(_parentForm.Handle, ref rectScreen);

                Rectangle rectBounds = rectScreen.ToRectangle();
                rectBounds.Offset(-rectBounds.X, -rectBounds.Y);

                // prepare clipping
                Rectangle rectClip = rectBounds;
                region = new Region(rectClip);
                rectClip.Inflate(-borderSize.Width, -borderSize.Height);
                rectClip.Y += captionHeight;
                rectClip.Height -= captionHeight;

                // create graphics handle
                hdc = Win32Api.GetDCEx(_parentForm.Handle, (IntPtr)0,
                    (DCXFlags.DCX_CACHE | DCXFlags.DCX_CLIPSIBLINGS | DCXFlags.DCX_WINDOW));
                g = Graphics.FromHdc(hdc);

                // Apply clipping
                region.Exclude(rectClip);
                hrgn = region.GetHrgn(g);
                Win32Api.SelectClipRgn(hdc, hrgn);

                // create new buffered graphics if needed
                if (_bufferGraphics == null || _currentCacheSize != rectBounds.Size)
                {
                    if (_bufferGraphics != null)
                        _bufferGraphics.Dispose();

                    _bufferGraphics = _bufferContext.Allocate(g, new Rectangle(0, 0,
                                rectBounds.Width, rectBounds.Height));
                    _currentCacheSize = rectBounds.Size;
                    invalidateBuffer = true;
                }

                if (invalidateBuffer)
                {
                    // Create painting meta data for form
                    SkinningFormPaintData paintData = new SkinningFormPaintData(_bufferGraphics.Graphics, rectBounds)
                    {
                        Borders = borderSize,
                        CaptionHeight = captionHeight,
                        Active = _formIsActive,
                        HasMenu = FormExtenders.HasMenu(_parentForm),
                        IconSize = SystemInformation.SmallIconSize,
                        IsSmallCaption =
                            captionHeight ==
                            SystemInformation.ToolWindowCaptionHeight,
                        Text = _parentForm.Text
                    };

                    // create painting meta data for caption buttons
                    if (_captionButtons.Count > 0)
                    {
                        paintData.CaptionButtons = new CaptionButtonPaintData[_captionButtons.Count];
                        for (int i = 0; i < _captionButtons.Count; i++)
                        {
                            CaptionButton button = _captionButtons[i];
                            CaptionButtonPaintData buttonData = new CaptionButtonPaintData(_bufferGraphics.Graphics, button.Bounds)
                            {
                                Pressed = button.Pressed,
                                Hovered = button.Hovered,
                                Enabled = button.Enabled,
                                HitTest = button.HitTest
                            };
                            paintData.CaptionButtons[i] = buttonData;
                        }
                    }

                    // paint
                    result = _manager.CurrentSkin.OnNcPaint(_parentForm, paintData);
                }

                // render buffered graphics 
                if (_bufferGraphics != null)
                    _bufferGraphics.Render(g);
            }
            catch (Exception)
            {// error drawing
                result = false;
            }

            // cleanup data
            if (hdc != (IntPtr)0)
            {
                Win32Api.SelectClipRgn(hdc, (IntPtr)0);
                Win32Api.ReleaseDC(_parentForm.Handle, hdc);
            }
            if (region != null && hrgn != (IntPtr)0)
                region.ReleaseHrgn(hrgn);

            if (region != null)
                region.Dispose();

            if (g != null)
                g.Dispose();

            return result;
        }

        /// <summary>
        /// Performs the non client HitTest
        /// </summary>
        /// <param name="m">The Message</param>
        /// <returns>true if the orginal handler should be suppressed otherwise false.</returns>
        private bool NcHitTest(ref Message m)
        {
            if (!IsProcessNcArea)
                return false;

            Point point = new Point(m.LParam.ToInt32());
            Rectangle rectScreen = FormExtenders.GetScreenRect(_parentForm);
            Rectangle rect = rectScreen;

            // custom processing
            if (rect.Contains(point))
            {
                Size borderSize = FormExtenders.GetBorderSize(_parentForm);
                rect.Inflate(-borderSize.Width, -borderSize.Height);

                // let form handle hittest itself if we are on borders
                if (!rect.Contains(point))
                    return false;

                Rectangle rectCaption = rect;
                rectCaption.Height = FormExtenders.GetCaptionHeight(_parentForm);

                // not in caption -> client
                if (!rectCaption.Contains(point))
                {
                    m.Result = (IntPtr)(int)HitTest.HTCLIENT;
                    return true;
                }

                // on icon?
                if (FormExtenders.HasMenu(_parentForm))
                {
                    Rectangle rectSysMenu = rectCaption;
                    rectSysMenu.Size = SystemInformation.SmallIconSize;
                    if (rectSysMenu.Contains(point))
                    {
                        m.Result = (IntPtr)(int)HitTest.HTSYSMENU;
                        return true;
                    }
                }

                // on Button?
                Point pt = new Point(point.X - rectScreen.X, point.Y - rectScreen.Y);
                CaptionButton sysButton = CommandButtonFromPoint(pt);
                if (sysButton != null)
                {
                    m.Result = (IntPtr)sysButton.HitTest;
                    return true;
                }

                // on Caption?
                m.Result = (IntPtr)(int)HitTest.HTCAPTION;
                return true;
            }
            m.Result = (IntPtr)(int)HitTest.HTNOWHERE;
            return true;
        }

        /// <summary>
        /// Handles the left button up message
        /// </summary>
        /// <param name="m">The message</param>
        /// <returns></returns>
        private bool OnNcLButtonUp(Message m)
        {
            if (!IsProcessNcArea)
                return false;

            // do we have a pressed button?
            if (_pressedButton != null)
            {
                // get button at wparam
                CaptionButton button = CommandButtonByHitTest((HitTest)m.WParam);
                if (button == null)
                    return false;

                if (button.Pressed)
                {
                    switch (button.HitTest)
                    {
                        case HitTest.HTCLOSE:
                            _parentForm.Close();
                            return true;
                        case HitTest.HTMAXBUTTON:
                            if (_parentForm.WindowState == FormWindowState.Maximized)
                            {
                                _parentForm.WindowState = FormWindowState.Normal;
                                _parentForm.Refresh();
                            }
                            else if (_parentForm.WindowState == FormWindowState.Normal ||
                                     _parentForm.WindowState == FormWindowState.Minimized)
                            {
                                _parentForm.WindowState = FormWindowState.Maximized;
                            }
                            break;
                        case HitTest.HTMINBUTTON:
                            _parentForm.WindowState = _parentForm.WindowState == FormWindowState.Minimized
                                                          ? FormWindowState.Normal
                                                          : FormWindowState.Minimized;
                            break;

                    }
                }

                _pressedButton.Pressed = false;
                _pressedButton.Hovered = false;
                _pressedButton = null;
            }
            return false;
        }
        #endregion
    }
}

