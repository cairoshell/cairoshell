using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.SupportingClasses;
using ManagedShell.Common.Enums;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.ShellFolders;
using ManagedShell.ShellFolders.Enums;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Icon.xaml
    /// </summary>
    public partial class Icon : UserControl
    {
        private static readonly string[] ImageFileTypes = { ".jpg", ".jpeg", ".jfif", ".gif", ".bmp", ".png" };
        
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location",
                typeof(string),
                typeof(Icon),
                new PropertyMetadata(null));

        public string Location
        {
            get { return (string)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public event EventHandler IconLoaded;
        public event EventHandler IconUnloaded;

        internal bool IsRenaming;
        private ShellFile file;
        
        private readonly FileOperationWorker _fileWorker;

        public Icon()
        {
            InitializeComponent();
            
            _fileWorker = new FileOperationWorker();
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "DesktopLabelPosition":
                    case "DesktopIconSize":
                        if (Location == "Desktop")
                        {
                            setDesktopIconAppearance();
                        }
                        break;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // register for settings changes
            Settings.Instance.PropertyChanged += Instance_PropertyChanged;

            // set ShellFile from binding
            file = DataContext as ShellFile;
            if (file != null)
            {
                // adjust appearance based on settings and usage
                if (Location == "Desktop")
                {
                    btnFile.Style = FindResource("CairoDesktopButtonStyle") as Style;
                    btnFile.ContextMenu = null;
                    txtFilename.Foreground = FindResource("DesktopIconText") as SolidColorBrush;

                    setDesktopIconAppearance();
                }
                else
                {
                    // stacks view

                    // adjust text sizing
                    txtFilename.Height = 35;
                    txtRename.Height = 34;
                    txtRename.Margin = new Thickness(5, 0, 5, 2);

                    // remove desktop effects
                    bdrFilename.Effect = null;
                    txtFilename.Effect = null;

                    // bind icon
                    Binding iconBinding = new Binding("LargeIcon");
                    iconBinding.Mode = BindingMode.OneWay;
                    iconBinding.FallbackValue = FindResource("NullIcon") as BitmapImage;
                    iconBinding.TargetNullValue = FindResource("NullIcon") as BitmapImage;
                    imgIcon.SetBinding(Image.SourceProperty, iconBinding);
                }
                
                if (!file.IsFolder && ImageFileTypes.Contains(Path.GetExtension(file.Path)))
                {
                    // set icon thumbnail shadow
                    imgIcon.Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(0, 0, 0),
                        Direction = 270,
                        ShadowDepth = 1,
                        BlurRadius = 4,
                        Opacity = 0.5
                    };
                }
                else
                {
                    imgIcon.Effect = null;
                }

                IconLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void setDesktopIconAppearance()
        {
            if (Settings.Instance.DesktopLabelPosition == 0)
            {
                // horizontal icons
                btnFile.Margin = new Thickness(9, 0, 8, 5);
                txtRename.Width = 130;
                txtFilename.Width = 130;
                txtFilename.TextAlignment = TextAlignment.Left;
                txtFilename.VerticalAlignment = VerticalAlignment.Center;
                imgIcon.SetValue(DockPanel.DockProperty, Dock.Left);
                txtRename.SetValue(DockPanel.DockProperty, Dock.Right);
                bdrFilename.SetValue(DockPanel.DockProperty, Dock.Right);
            }
            else
            {
                // vertical icons
                btnFile.Margin = new Thickness(0, 0, 0, 0);
                txtRename.Width = 80;
                txtFilename.Width = 80;
                txtFilename.TextAlignment = TextAlignment.Center;
                txtFilename.VerticalAlignment = VerticalAlignment.Top;
                imgIcon.SetValue(DockPanel.DockProperty, Dock.Top);
                txtRename.SetValue(DockPanel.DockProperty, Dock.Bottom);
                bdrFilename.SetValue(DockPanel.DockProperty, Dock.Bottom);
            }

            string bindingPath = "LargeIcon";
            if (Settings.Instance.DesktopIconSize == (int)IconSize.ExtraLarge)
            {
                imgIcon.Width = IconHelper.GetSize(IconSize.ExtraLarge);
                imgIcon.Height = IconHelper.GetSize(IconSize.ExtraLarge);
                bindingPath = "ExtraLargeIcon";
            }
            else
            {
                imgIcon.Width = IconHelper.GetSize(IconSize.Large);
                imgIcon.Height = IconHelper.GetSize(IconSize.Large);
            }
            
            Binding iconBinding = new Binding(bindingPath);
            iconBinding.Mode = BindingMode.OneWay;
            iconBinding.FallbackValue = FindResource("NullIcon") as BitmapImage;
            iconBinding.TargetNullValue = FindResource("NullIcon") as BitmapImage;
            imgIcon.SetBinding(Image.SourceProperty, iconBinding);

            switch ($"{Settings.Instance.DesktopLabelPosition}{Settings.Instance.DesktopIconSize}")
            {
                case "00": // horizontal small
                    btnFile.Height = 48;
                    break;
                case "02": // horizontal large
                    btnFile.Height = 64;
                    break;
                case "12": // vertical large
                    btnFile.Height = 97;
                    break;
                default: // vertical small
                    btnFile.Height = 85;
                    break;
            }
        }

        #region Rename
        internal void BeginRename()
        {
            txtRename.Visibility = Visibility.Visible;
            bdrFilename.Visibility = Visibility.Collapsed;

            int selectionLength = txtFilename.Text.Length;
            if (txtFilename.Text.Contains("."))
            {
                selectionLength = txtFilename.Text.LastIndexOf('.');
            }
            
            txtRename.Focus();
            txtRename.Select(0, selectionLength);

            IsRenaming = true;
        }

        private void txtRename_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (box == null || file == null)
            {
                return;
            }
            
            try
            {
                file.Rename(box.Text);
            }
            catch (Exception exception)
            {
                box.Text = file.FileName;
                CairoMessage.Show("The file was unable to be renamed because: " + exception.Message, "Unable to rename", MessageBoxButton.OK, CairoMessageImage.Error);
            }

            foreach (UIElement peer in (box.Parent as DockPanel).Children)
            {
                if (peer is Border)
                {
                    peer.Visibility = Visibility.Visible;
                }
            }

            box.Visibility = Visibility.Collapsed;
            IsRenaming = false;
        }

        private void txtRename_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            e.Handled = true;
        }
        #endregion

        #region Drop
        private bool isDropMove;
        private void btnFile_DragOver(object sender, DragEventArgs e)
        {
            if (!IsRenaming)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(typeof(ShellItem)))
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
        }

        private void btnFile_Drop(object sender, DragEventArgs e)
        {
            if (!IsRenaming)
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (e.Data.GetDataPresent(typeof(ShellItem)) && e.Data.GetData(typeof(ShellItem)) is ShellItem dropData)
                {
                    fileNames = new[] { dropData.Path };
                }

                if (fileNames != null && _fileWorker != null)
                {
                    _fileWorker.PerformOperation(isDropMove ? FileOperation.Move : FileOperation.Copy, 
                        fileNames, 
                        file.IsFolder ? file.Path : Path.GetDirectoryName(file.Path));

                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Drag
        private Point? startPoint;
        private bool inDrag;

        private void btnFile_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            if (!IsRenaming)
            {
                startPoint = e.GetPosition(this);
            }
        }

        private void btnFile_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Reset the mouse position
            if (!IsRenaming)
            {
                startPoint = null;
            }
        }

        private void btnFile_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (file == null)
            {
                return;
            }

            if (!file.IsFileSystem)
            {
                // we can only perform file operations on filesystem objects
                return;
            }
            
            if (!IsRenaming)
            {
                if (!inDrag && startPoint != null)
                {
                    inDrag = true;

                    Point mousePos = e.GetPosition(this);

                    if (mousePos.X == 0 && mousePos.Y == 0)
                    {
                        // sometimes we get an invalid position, ignore it so we don't start a drag operation
                        startPoint = null;
                        return;
                    }
                    
                    Vector diff = (Point)startPoint - mousePos;

                    if (mousePos.Y <= ActualHeight && ((Point)startPoint).Y <= ActualHeight &&
                        (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) &&
                        (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                    {
                        Button button = sender as Button;
                        DataObject dragObject = new DataObject();

                        try
                        {
                            dragObject.SetFileDropList(
                                new System.Collections.Specialized.StringCollection { file.Path });
                            
                            DragDrop.DoDragDrop(button, dragObject,
                                (e.RightButton == MouseButtonState.Pressed
                                    ? DragDropEffects.All
                                    : DragDropEffects.Move));
                        }
                        catch (Exception exception)
                        {
                            ShellLogger.Warning($"Icon: Unable to perform drag-drop operation: {exception.Message}");
                        }

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
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // unregister settings changes
            Settings.Instance.PropertyChanged -= Instance_PropertyChanged;

            IconUnloaded?.Invoke(this, EventArgs.Empty);
        }

        private void UserControl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsRenaming)
            {
                return;
            }
            
            switch (e.Key)
            {
                // [Ctrl] + [C] => Copy
                case Key.C when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    CustomCommands.PerformAction(CustomCommands.Actions.Copy, file.Path);
                    break;

                // [Ctrl] + [X] => Cut
                case Key.X when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    // TODO: Not yet implemented
                    //CustomCommands.PerformAction(CustomCommands.Actions.Cut, file.Path);
                    break;

                // [Ctrl] + [V] => Paste
                case Key.V when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    if(file.IsFolder)
                    {
                        // TODO: Paste item into folder?
                    }
                    break;

                // TODO: [Shift] + [Delete]  => Delete an item permanently without placing it into the Recycle Bin
                case Key.Delete when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift):
                    CustomCommands.PerformAction(CustomCommands.Actions.Delete, file.Path);
                    break;

                // [Delete] => Delete an item and place it into the Recycle Bin
                case Key.Delete:
                    CustomCommands.PerformAction(CustomCommands.Actions.Delete, file.Path);
                    break;

                // TODO: [Alt] + [Enter] => Open file properties
                case Key.Enter when Keyboard.Modifiers.HasFlag(ModifierKeys.Alt):
                    CustomCommands.PerformAction(CustomCommands.Actions.Properties, file.Path);
                    break;

                // [Enter] => Open file
                case Key.Enter:
                    CustomCommands.PerformAction(CustomCommands.Actions.Open, file.Path);
                    break;

                // TODO: [F2] => Rename Item. Select name excluding file extension
                case Key.F2:
                    CustomCommands.PerformAction(CustomCommands.Actions.Rename, file.Path);
                    break;
            }
        }
    }
}