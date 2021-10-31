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
        internal TaskButton TaskButton;
        internal bool IsAnimating = true;
        internal bool IsDwmEnabled;
        internal bool IsContextMenuOpen;

        private bool isClosing;
        private IntPtr handle;

        public TaskThumbWindow(TaskButton parent)
        {
            InitializeComponent();

            TaskButton = parent;
            DataContext = TaskButton;

            TaskButton.SetParentAutoHide(false);

            IsDwmEnabled = NativeMethods.DwmIsCompositionEnabled();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // hide from alt-tab
            WindowInteropHelper helper = new WindowInteropHelper(this);
            handle = helper.Handle;
            WindowHelper.HideWindowFromTasks(handle);

            // get anchor point
            Point taskButtonPoint = TaskButton.GetThumbnailAnchor();

            if (Configuration.Settings.Instance.TaskbarPosition == 1)
            {
                // taskbar on top
                Top = taskButtonPoint.Y + TaskButton.ActualHeight;

                bdrThumb.Style = FindResource("TaskThumbWindowBorderTopStyle") as Style;
                bdrThumbInner.Style = FindResource("TaskThumbWindowInnerBorderTopStyle") as Style;

                bdrTranslate.Y *= -1;

                ToolTipService.SetPlacement(this, System.Windows.Controls.Primitives.PlacementMode.Bottom);
            }
            else
            {
                Top = taskButtonPoint.Y - ActualHeight;
            }

            Left = taskButtonPoint.X - ((ActualWidth - TaskButton.ActualWidth) / 2);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            TaskButton.ThumbWindow = null;
            TaskButton.SetParentAutoHide(true);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isClosing && !IsContextMenuOpen && !TaskButton.IsMouseOver)
                Close();
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
        }
    }
}
