using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for TaskThumbWindow.xaml
    /// </summary>
    public partial class TaskThumbWindow : Window
    {
        private TaskButton taskButton;
        private bool isClosing;

        public TaskThumbWindow(TaskButton parent)
        {
            InitializeComponent();

            taskButton = parent;
            DataContext = parent.Window;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Point taskButtonPoint = taskButton.GetThumbnailAnchor();

            if (Configuration.Settings.Instance.TaskbarPosition == 1)
            {
                // taskbar on top
                Top = taskButtonPoint.Y + taskButton.ActualHeight;

                bdrThumb.Style = Application.Current.FindResource("TaskThumbWindowBorderTopStyle") as Style;
                bdrThumbInner.Style = Application.Current.FindResource("TaskThumbWindowInnerBorderTopStyle") as Style;

                ToolTipService.SetPlacement(this, System.Windows.Controls.Primitives.PlacementMode.Bottom);
            }
            else
            {
                Top = taskButtonPoint.Y - ActualHeight;
            }

            Left = taskButtonPoint.X - ((ActualWidth - taskButton.ActualWidth) / 2);

            dwmThumbnail.SourceWindowHandle = taskButton.Window.Handle;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            dwmThumbnail.SourceWindowHandle = IntPtr.Zero;
            taskButton.ThumbWindow = null;
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
            taskButton.btn.ContextMenu.IsOpen = true;
        }
    }
}
