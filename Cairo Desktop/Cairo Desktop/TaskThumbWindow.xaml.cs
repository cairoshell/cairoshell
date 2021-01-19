using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for TaskThumbWindow.xaml
    /// </summary>
    public partial class TaskThumbWindow : Window
    {
        private TaskButton taskButton;
        private bool isClosing;
        private bool isAnimating;
        private bool isDwmEnabled;
        private IntPtr handle;

        public TaskThumbWindow(TaskButton parent)
        {
            InitializeComponent();

            taskButton = parent;
            DataContext = parent.Window;

            taskButton.SetParentAutoHide(false);

            // check if DWM is enabled, if not, hide the thumbnail placeholder
            isDwmEnabled = NativeMethods.DwmIsCompositionEnabled();
            if (!isDwmEnabled)
            {
                dwmThumbnail.Visibility = Visibility.Collapsed;
                pnlTitle.Margin = new Thickness(0);
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // hide from alt-tab
            WindowInteropHelper helper = new WindowInteropHelper(this);
            handle = helper.Handle;
            WindowHelper.HideWindowFromTasks(handle);

            // get anchor point
            Point taskButtonPoint = taskButton.GetThumbnailAnchor();

            if (Configuration.Settings.Instance.TaskbarPosition == 1)
            {
                // taskbar on top
                Top = taskButtonPoint.Y + taskButton.ActualHeight;

                bdrThumb.Style = FindResource("TaskThumbWindowBorderTopStyle") as Style;
                bdrThumbInner.Style = FindResource("TaskThumbWindowInnerBorderTopStyle") as Style;

                bdrTranslate.Y *= -1;

                ToolTipService.SetPlacement(this, System.Windows.Controls.Primitives.PlacementMode.Bottom);
            }
            else
            {
                Top = taskButtonPoint.Y - ActualHeight;
            }

            Left = taskButtonPoint.X - ((ActualWidth - taskButton.ActualWidth) / 2);

            if (isDwmEnabled)
            {
                // set up thumbnail
                dwmThumbnail.DpiScale = taskButton.ParentTaskbar.DpiScale;
                dwmThumbnail.ThumbnailOpacity = 0;
                dwmThumbnail.SourceWindowHandle = taskButton.Window.Handle;

                // set up animation
                isAnimating = true;
                System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // runs once per frame for the duration of the animation
            if (isAnimating)
            {
                dwmThumbnail.ThumbnailOpacity = Convert.ToByte(bdrThumb.Opacity * 255);
                dwmThumbnail.Refresh();
            }
            else
            {
                // refresh one last time to get the final frame's updates
                dwmThumbnail.ThumbnailOpacity = 255;
                dwmThumbnail.Refresh();
                System.Windows.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            if (isDwmEnabled) dwmThumbnail.SourceWindowHandle = IntPtr.Zero;
            taskButton.ThumbWindow = null;
            taskButton.SetParentAutoHide(true);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isClosing && !taskButton.btn.ContextMenu.IsOpen && !taskButton.IsMouseOver)
                Close();
        }

        private void bdrThumbInner_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                taskButton.SelectWindow();
                Close();
            }
        }

        private void bdrThumbInner_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            taskButton.ConfigureContextMenu();
            taskButton.btn.ContextMenu.IsOpen = true;
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            isAnimating = false;
        }

        private void bdrThumbInner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (isDwmEnabled)
            {
                WindowHelper.PeekWindow(true, taskButton.Window.Handle, taskButton.ParentTaskbar.Handle);
            }
        }

        private void bdrThumbInner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (isDwmEnabled)
            {
                WindowHelper.PeekWindow(false, taskButton.Window.Handle, taskButton.ParentTaskbar.Handle);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            taskButton.Window.Close();
            Close();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (closeButton.Visibility != Visibility.Visible)
                closeButton.Visibility = Visibility.Visible;
        }
    }
}
