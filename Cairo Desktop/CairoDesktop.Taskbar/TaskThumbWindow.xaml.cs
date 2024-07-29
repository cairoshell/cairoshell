using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using CairoDesktop.Common;
using ManagedShell.AppBar;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;

namespace CairoDesktop.Taskbar
{
    /// <summary>
    /// Interaction logic for TaskThumbWindow.xaml
    /// </summary>
    public partial class TaskThumbWindow : Window, INotifyPropertyChanged
    {
        const int MAX_THUMBS = 5;

        public bool ShowThumbnails
        {
            get
            {
                if (!IsDwmEnabled || !Settings.Instance.EnableTaskbarThumbnails)
                {
                    return false;
                }

                if (TaskButton != null && TaskButton.WindowGroup != null && TaskButton.WindowGroup.Count > MAX_THUMBS)
                {
                    return false;
                }

                return true;
            }
        }

        public AppBarEdge AppBarEdge
        {
            get => Settings.Instance.TaskbarEdge;
        }

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

            TaskButton.SetParentPopupOpen(true);

            IsDwmEnabled = NativeMethods.DwmIsCompositionEnabled();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // hide from alt-tab
            WindowInteropHelper helper = new WindowInteropHelper(this);
            handle = helper.Handle;
            WindowHelper.HideWindowFromTasks(handle);

            if (Settings.Instance.TaskbarEdge == AppBarEdge.Top)
            {
                // taskbar on top
                bdrThumb.Style = FindResource("TaskThumbWindowBorderTopStyle") as Style;
                bdrThumbInner.Style = FindResource("TaskThumbWindowInnerBorderTopStyle") as Style;

                bdrTranslate.Y *= -1;

                ToolTipService.SetPlacement(this, System.Windows.Controls.Primitives.PlacementMode.Bottom);
            }

            SetPosition();

            if (TaskButton != null && TaskButton.WindowGroup != null)
            {
                ((INotifyCollectionChanged)TaskButton.WindowGroup).CollectionChanged += TaskThumbWindow_CollectionChanged;
            }
        }

        private void TaskThumbWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("ShowThumbnails");
        }

        private void SetPosition()
        {
            if (TaskButton == null || TaskButton.ParentTaskbar == null)
            {
                return;
            }

            Point taskButtonPoint = TaskButton.GetThumbnailAnchor();

            if (Settings.Instance.TaskbarEdge == AppBarEdge.Top)
            {
                // taskbar on top
                Top = taskButtonPoint.Y + TaskButton.ActualHeight;
            }
            else
            {
                Top = taskButtonPoint.Y - ActualHeight;
            }

            double desiredLeft = taskButtonPoint.X - ((ActualWidth - TaskButton.ActualWidth) / 2);

            if (desiredLeft < TaskButton.ParentTaskbar.Left)
            {
                Left = TaskButton.ParentTaskbar.Left;
            }
            else if (desiredLeft + ActualWidth > TaskButton.ParentTaskbar.Left + TaskButton.ParentTaskbar.ActualWidth)
            {
                double bump = desiredLeft + ActualWidth - (TaskButton.ParentTaskbar.Left + TaskButton.ParentTaskbar.ActualWidth);
                Left = desiredLeft - bump;
            }
            else
            {
                Left = desiredLeft;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isClosing = true;
            TaskButton.ThumbWindow = null;
            TaskButton.SetParentPopupOpen(false);

            if (TaskButton != null && TaskButton.WindowGroup != null)
            {
                ((INotifyCollectionChanged)TaskButton.WindowGroup).CollectionChanged -= TaskThumbWindow_CollectionChanged;
            }
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void ThumbWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPosition();
        }
    }
}
