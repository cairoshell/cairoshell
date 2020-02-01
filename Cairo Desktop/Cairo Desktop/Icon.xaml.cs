using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.Localization;
using CairoDesktop.SupportingClasses;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Icon.xaml
    /// </summary>
    public partial class Icon : UserControl
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location", typeof(string), typeof(Icon), new PropertyMetadata(null));
        public string Location
        {
            get { return (string)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        private SystemFile file;

        public Icon()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // set SystemFile from binding
            file = (SystemFile)DataContext;

            // adjust appearance based on settings and usage
            if (Location == "Desktop")
            {
                btnFile.Style = Application.Current.FindResource("CairoDesktopButtonStyle") as Style;
                btnFile.ContextMenu = null;
                btnFile.Click -= btnFile_Click;
                btnFile.MouseDoubleClick += btnFile_Click;
                txtFilename.Foreground = Application.Current.FindResource("DesktopIconText") as SolidColorBrush;

                if (Settings.DesktopLabelPosition == 0)
                {
                    // horizontal icons
                    btnFile.Margin = new Thickness(9, 0, 8, 5);
                    bdrFilename.Height = 36;
                    txtRename.Width = 130;
                    txtRename.TextAlignment = TextAlignment.Left;
                    txtFilename.Width = 130;
                    txtFilename.Height = double.NaN;
                    txtFilename.TextAlignment = TextAlignment.Left;
                    txtFilename.VerticalAlignment = VerticalAlignment.Center;
                    imgIcon.SetValue(DockPanel.DockProperty, Dock.Left);
                    txtRename.SetValue(DockPanel.DockProperty, Dock.Right);
                    bdrFilename.SetValue(DockPanel.DockProperty, Dock.Right);
                }

                if (Settings.DesktopIconSize == 2)
                {
                    // large icons
                    imgIcon.Width = 48;
                    imgIcon.Height = 48;
                    Binding iconBinding = new Binding("LargeIcon");
                    iconBinding.Mode = BindingMode.OneWay;
                    iconBinding.FallbackValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                    iconBinding.TargetNullValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                    imgIcon.SetBinding(Image.SourceProperty, iconBinding);
                }
                else
                {
                    // small icons
                    Binding iconBinding = new Binding("Icon");
                    iconBinding.Mode = BindingMode.OneWay;
                    iconBinding.FallbackValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                    iconBinding.TargetNullValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                    imgIcon.SetBinding(Image.SourceProperty, iconBinding);
                }

                if (Settings.DesktopLabelPosition == 0 && Settings.DesktopIconSize == 0)
                {
                    // horizontal small
                    btnFile.Height = 48;
                }

                if (Settings.DesktopLabelPosition == 0 && Settings.DesktopIconSize == 2)
                {
                    // horizontal large
                    btnFile.Height = 64;
                }

                // (vertical small is in xaml)

                if (Settings.DesktopLabelPosition == 1 && Settings.DesktopIconSize == 2)
                {
                    // vertical large
                    btnFile.Height = 97;
                }
            }
            else
            {
                // stacks view

                // remove desktop effects
                bdrFilename.Effect = null;
                txtFilename.Effect = null;

                // bind icon
                Binding iconBinding = new Binding("Icon");
                iconBinding.Mode = BindingMode.OneWay;
                iconBinding.FallbackValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                iconBinding.TargetNullValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                imgIcon.SetBinding(Image.SourceProperty, iconBinding);
            }
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            if (file != null)
            {
                if (!string.IsNullOrWhiteSpace(file.FullName))
                {
                    // Determine if [SHIFT] key is held. Bypass Directory Processing, which will use the Shell to open the item.
                    if (!KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                    {
                        // if directory, perform special handling
                        if (file.IsDirectory
                            && Location == "Desktop"
                            && Settings.EnableDynamicDesktop
                            && Startup.DesktopWindow != null)
                        {
                            Startup.DesktopWindow.Navigate(file.FullName);
                            return;
                        }
                        else if (file.IsDirectory)
                        {
                            FolderHelper.OpenLocation(file.FullName);
                            return;
                        }
                    }

                    if (Startup.DesktopWindow != null)
                        Startup.DesktopWindow.IsOverlayOpen = false;

                    Shell.ExecuteProcess(file.FullName);
                    return;
                }
            }

            CairoMessage.Show(DisplayString.sError_FileNotFoundInfo, DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region Context menu
        private void miVerb_Click(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_MenuItem_Click(sender, e);
        }

        private void ctxFile_Loaded(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_ContextMenu_Loaded(sender, e);
        }
        #endregion

        #region Rename
        private void txtRename_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            string orig = ((Button)((DockPanel)box.Parent).Parent).CommandParameter as string;

            if (!SystemFile.RenameFile(orig, box.Text))
                box.Text = Path.GetFileName(orig);

            foreach (UIElement peer in (box.Parent as DockPanel).Children)
                if (peer is Border)
                    peer.Visibility = Visibility.Visible;

            box.Visibility = Visibility.Collapsed;
        }

        private void txtRename_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                (sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
        #endregion

        #region Drop
        private bool isDropMove = false;
        private void btnFile_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(typeof(SystemFile)))
            {
                if ((e.KeyStates & DragDropKeyStates.RightMouseButton) != 0)
                {
                    e.Effects = DragDropEffects.Copy;
                    isDropMove = false;
                }
                else if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) != 0)
                {
                    e.Effects = DragDropEffects.Move;
                    isDropMove = true;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
                isDropMove = false;
            }

            e.Handled = true;
        }

        private void btnFile_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (e.Data.GetDataPresent(typeof(SystemFile)))
            {
                SystemFile dropData = e.Data.GetData(typeof(SystemFile)) as SystemFile;
                fileNames = new string[] { dropData.FullName };
            }

            if (fileNames != null)
            {
                if (file.IsDirectory)
                {
                    if (!isDropMove) SystemDirectory.CopyInto(fileNames, file.FullName);
                    else if (isDropMove) SystemDirectory.MoveInto(fileNames, file.FullName);
                }
                else
                {
                    if (!isDropMove) file.ParentDirectory.CopyInto(fileNames);
                    else if (isDropMove) file.ParentDirectory.MoveInto(fileNames);
                }

                e.Handled = true;
            }
        }
        #endregion

        #region Drag
        private Point? startPoint = null;
        private bool inDrag = false;
        private void btnFile_PreviewMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(this);
        }

        private void btnFile_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!inDrag && startPoint != null)
            {
                inDrag = true;

                Point mousePos = e.GetPosition(this);
                Vector diff = (Point)startPoint - mousePos;

                if (mousePos.Y <= this.ActualHeight && ((Point)startPoint).Y <= this.ActualHeight && (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    Button button = sender as Button;
                    DataObject dragObject = new DataObject();
                    dragObject.SetFileDropList(new System.Collections.Specialized.StringCollection() { file.FullName });

                    try
                    {
                        DragDrop.DoDragDrop(button, dragObject, (e.RightButton == MouseButtonState.Pressed ? DragDropEffects.All : DragDropEffects.Move));
                    }
                    catch { }

                    // reset the stored mouse position
                    startPoint = null;
                }
                else if (e.LeftButton != MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed)
                {
                    // reset the stored mouse position
                    startPoint = null;
                }

                inDrag = false;
            }

            e.Handled = true;
        }
        #endregion
    }
}
