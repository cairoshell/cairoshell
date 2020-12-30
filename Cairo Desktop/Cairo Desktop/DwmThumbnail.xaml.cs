using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using ManagedShell.Interop;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DwmThumbnail.xaml
    /// </summary>
    public partial class DwmThumbnail : UserControl
    {
        public byte ThumbnailOpacity = 255;
        public double DpiScale = 1.0;

        public DwmThumbnail()
        {
            InitializeComponent();

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        public IntPtr Handle
        {
            get
            {
                HwndSource source = (HwndSource)HwndSource.FromVisual(this);
                IntPtr handle = source.Handle;
                return handle;
            }
        }

        private IntPtr _sourceWindowHandle = IntPtr.Zero;
        private IntPtr _thumbHandle;

        public IntPtr SourceWindowHandle
        {
            get
            {
                return _sourceWindowHandle;
            }
            set
            {
                if (_sourceWindowHandle != IntPtr.Zero)
                {
                    NativeMethods.DwmUnregisterThumbnail(_thumbHandle);
                    _thumbHandle = IntPtr.Zero;
                }

                _sourceWindowHandle = value;
                if (_sourceWindowHandle != IntPtr.Zero && NativeMethods.DwmRegisterThumbnail(Handle, _sourceWindowHandle, out _thumbHandle) == 0)
                    Refresh();
            }
        }

        public NativeMethods.Rect Rect
        {
            get
            {
                try
                {
                    if (this == null)
                        return new NativeMethods.Rect(0, 0, 0, 0);

                    Window ancestor = Window.GetWindow(this);
                    if (ancestor != null)
                    {
                        var generalTransform = TransformToAncestor(ancestor);
                        var leftTopPoint = generalTransform.Transform(new Point(0, 0));
                        return new NativeMethods.Rect(
                              (int)(leftTopPoint.X * DpiScale),
                              (int)(leftTopPoint.Y * DpiScale),
                              (int)(leftTopPoint.X * DpiScale) + (int)(ActualWidth * DpiScale),
                              (int)(leftTopPoint.Y * DpiScale) + (int)(ActualHeight * DpiScale)
                             );
                    }
                    else
                    {
                        return new NativeMethods.Rect(0, 0, 0, 0);
                    }
                }
                catch
                {
                    return new NativeMethods.Rect(0, 0, 0, 0);
                }
            }
        }

        public void Refresh()
        {
            if (this == null)
                return;

            if (_thumbHandle == IntPtr.Zero)
                return;

            if (this != null)
            {
                NativeMethods.DwmQueryThumbnailSourceSize(_thumbHandle, out NativeMethods.PSIZE size);
                double aspectRatio = (double)size.x / size.y;

                var props = new NativeMethods.DWM_THUMBNAIL_PROPERTIES
                {
                    fVisible = true,
                    dwFlags = NativeMethods.DWM_TNP_VISIBLE | NativeMethods.DWM_TNP_RECTDESTINATION | NativeMethods.DWM_TNP_OPACITY,
                    opacity = ThumbnailOpacity,
                    rcDestination = Rect
                };

                if (this != null)
                {
                    if (size.x <= Rect.Width && size.y <= Rect.Height)
                    {
                        // do not scale
                        props.rcDestination.Top += (Rect.Height - size.y) / 2;
                        props.rcDestination.Left += (Rect.Width - size.x) / 2;
                        props.rcDestination.Right = props.rcDestination.Left + size.x;
                        props.rcDestination.Bottom = props.rcDestination.Top + size.y;
                    }
                    else
                    {
                        // scale, preserving aspect ratio and center
                        double controlAspectRatio = (double) Rect.Width / Rect.Height;

                        if (aspectRatio > controlAspectRatio)
                        {
                            // wide
                            int height = (int) (Rect.Width / aspectRatio);

                            props.rcDestination.Top += (Rect.Height - height) / 2;
                            props.rcDestination.Bottom = props.rcDestination.Top + height;
                        }
                        else if (aspectRatio < controlAspectRatio)
                        {
                            // tall
                            int width = (int) (Rect.Height * aspectRatio);

                            props.rcDestination.Left += (Rect.Width - width) / 2;
                            props.rcDestination.Right = props.rcDestination.Left + width;
                        }
                    }
                }

                if (this != null)
                    NativeMethods.DwmUpdateThumbnailProperties(_thumbHandle, ref props);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_thumbHandle != IntPtr.Zero)
            {
                NativeMethods.DwmUnregisterThumbnail(_thumbHandle);
                _thumbHandle = IntPtr.Zero;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Refresh();
        }

        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}