using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.Localization;
using CairoDesktop.SupportingClasses;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Icon.xaml
    /// </summary>
    public partial class Icon : UserControl
    {
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

        private bool currentlyRenaming;
        private SystemFile file;

        public Icon()
        {
            InitializeComponent();
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

            // set SystemFile from binding
            file = DataContext as SystemFile;
            if (file != null)
            {
                // adjust appearance based on settings and usage
                if (Location == "Desktop")
                {
                    btnFile.Style = Application.Current.FindResource("CairoDesktopButtonStyle") as Style;
                    btnFile.ContextMenu = null;
                    btnFile.Click -= btnFile_Click;
                    btnFile.MouseDoubleClick += btnFile_MouseDoubleClick;
                    btnFile.MouseRightButtonUp += btnFile_MouseRightButtonUp;
                    txtFilename.Foreground = Application.Current.FindResource("DesktopIconText") as SolidColorBrush;

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
                    Binding iconBinding = new Binding("Icon");
                    iconBinding.Mode = BindingMode.OneWay;
                    iconBinding.FallbackValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                    iconBinding.TargetNullValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                    imgIcon.SetBinding(Image.SourceProperty, iconBinding);
                }

                file.InteractiveRenameEvent += InteractiveRenameEvent;

                if (file.InteractiveRenameRequested)
                {
                    enterRename();
                }
            }
        }

        private void InteractiveRenameEvent(object sender, EventArgs e)
        {
            enterRename();
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

            if (Settings.Instance.DesktopIconSize == (int)IconSize.ExtraLarge)
            {
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
                imgIcon.Width = 32;
                imgIcon.Height = 32;
                Binding iconBinding = new Binding("Icon");
                iconBinding.Mode = BindingMode.OneWay;
                iconBinding.FallbackValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                iconBinding.TargetNullValue = Application.Current.FindResource("NullIcon") as BitmapImage;
                imgIcon.SetBinding(Image.SourceProperty, iconBinding);
            }

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

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            execute();
        }

        private void btnFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // only execute on left button double-click
            if (e.LeftButton == MouseButtonState.Pressed) execute();
        }

        private void execute()
        {
            if (!currentlyRenaming)
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
                                && Settings.Instance.EnableDynamicDesktop
                                && DesktopManager.IsEnabled)
                            {
                                DesktopManager.Instance.NavigationManager.NavigateTo(file.FullName);
                                return;
                            }
                            else if (file.IsDirectory)
                            {
                                FolderHelper.OpenLocation(file.FullName);
                                return;
                            }
                        }

                        DesktopManager.Instance.IsOverlayOpen = false;

                        Shell.ExecuteProcess(file.FullName);
                        return;
                    }
                }

                CairoMessage.Show(DisplayString.sError_FileNotFoundInfo, DisplayString.sError_OhNo, MessageBoxButton.OK,
                    CairoMessageImage.Error);
            }
        }

        #region Context menu
        private void btnFile_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // desktop icon context menu
            // not used for stacks, which use our own menu due to the shell menu causing stacks to close
            if (!currentlyRenaming)
            {
                ShellContextMenu cm = new ShellContextMenu(new SystemFile[] { file }, executeFileAction);
                e.Handled = true;
            }
        }

        private void executeFileAction(string action, string path)
        {
            switch (action)
            {
                case CustomCommands.DirectoryActions.OpenFolder:
                    if (Settings.Instance.EnableDynamicDesktop)
                    {
                        DesktopManager.Instance.NavigationManager.NavigateTo(path);
                    }
                    else
                    {
                        FolderHelper.OpenLocation(path);
                    }
                    break;
                case CustomCommands.Actions.Rename:
                    file.RequestInteractiveRename(this);
                    break;
                case CustomCommands.DirectoryActions.AddStack:
                case CustomCommands.DirectoryActions.RemoveStack:
                case CustomCommands.Actions.OpenWithShell:
                    CustomCommands.PerformAction(action, path);
                    break;
                default:
                    if (action != CustomCommands.Actions.Cut && action != CustomCommands.Actions.Copy && action != CustomCommands.Actions.Link)
                    {
                        DesktopManager.Instance.IsOverlayOpen = false;
                    }

                    break;
            }
        }

        private void miVerb_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string verb = item.Tag as string;

            if (verb == CustomCommands.Actions.Rename)
            {
                enterRename();
            }
            else
            {
                CustomCommands.PerformAction(verb, file.FullName);
            }

            DesktopManager.Instance.IsOverlayOpen = false;
        }

        private void ctxFile_Loaded(object sender, RoutedEventArgs e)
        {
            ctxFile.Items.Clear();

            ctxFile.Items.Add(new MenuItem { Header = DisplayString.sInterface_Open, Tag = CustomCommands.Actions.Open });

            if (!file.IsDirectory)
            {
                ctxFile.Items.Add(new MenuItem { Header = DisplayString.sInterface_OpenWith, Tag = CustomCommands.Actions.OpenWith });
            }
            else
            {
                ctxFile.Items.Add(new MenuItem { Header = DisplayString.sInterface_AddToStacks, Tag = CustomCommands.DirectoryActions.AddStack });
            }

            foreach (string verb in file.Verbs)
            {
                if (verb.ToLower() != CustomCommands.Actions.Open)
                {
                    ctxFile.Items.Add(new MenuItem { Header = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(verb), Tag = verb });
                }
            }

            ctxFile.Items.Add(new Separator());

            ctxFile.Items.Add(new MenuItem { Header = DisplayString.sInterface_Copy, Tag = CustomCommands.Actions.Copy });

            ctxFile.Items.Add(new Separator());

            ctxFile.Items.Add(new MenuItem { Header = DisplayString.sInterface_Delete, Tag = CustomCommands.Actions.Delete });
            ctxFile.Items.Add(new MenuItem { Header = DisplayString.sInterface_Rename, Tag = CustomCommands.Actions.Rename });

            ctxFile.Items.Add(new Separator());

            ctxFile.Items.Add(new MenuItem { Header = DisplayString.sInterface_Properties, Tag = CustomCommands.Actions.Properties });
        }
        #endregion

        #region Rename
        private void enterRename()
        {
            txtRename.Visibility = Visibility.Visible;
            bdrFilename.Visibility = Visibility.Collapsed;

            int selectionLength = txtFilename.Text.Length - 1;
            if (txtFilename.Text.Contains("."))
            {
                selectionLength = txtFilename.Text.LastIndexOf('.');
            }

            txtRename.Focus();
            txtRename.Select(0, selectionLength);

            currentlyRenaming = true;
        }

        private void txtRename_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;

            if (!file.Rename(box.Text))
            {
                box.Text = Path.GetFileName(file.FullName);
            }

            foreach (UIElement peer in (box.Parent as DockPanel).Children)
            {
                if (peer is Border)
                {
                    peer.Visibility = Visibility.Visible;
                }
            }

            box.Visibility = Visibility.Collapsed;
            currentlyRenaming = false;
        }

        private void txtRename_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        #endregion

        #region Drop
        private bool isDropMove = false;
        private void btnFile_DragOver(object sender, DragEventArgs e)
        {
            if (!currentlyRenaming)
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
        }

        private void btnFile_Drop(object sender, DragEventArgs e)
        {
            if (!currentlyRenaming)
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
                        SystemDirectory destDirectory = new SystemDirectory(file.FullName, Dispatcher);
                        if (isDropMove)
                        {
                            destDirectory.MoveInto(fileNames);
                        }
                        else
                        {
                            destDirectory.CopyInto(fileNames);
                        }
                    }
                    else
                    {
                        if (isDropMove)
                        {
                            file.ParentDirectory?.MoveInto(fileNames);
                        }
                        else
                        {
                            file.ParentDirectory?.CopyInto(fileNames);
                        }
                    }

                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Drag
        private Point? startPoint = null;
        private bool inDrag = false;
        private void btnFile_PreviewMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            if (!currentlyRenaming)
            {
                startPoint = e.GetPosition(this);
            }
        }

        private void btnFile_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!currentlyRenaming)
            {
                if (!inDrag && startPoint != null)
                {
                    inDrag = true;

                    Point mousePos = e.GetPosition(this);
                    Vector diff = (Point)startPoint - mousePos;

                    if (mousePos.Y <= ActualHeight && ((Point)startPoint).Y <= ActualHeight &&
                        (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) &&
                        (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                    {
                        Button button = sender as Button;
                        DataObject dragObject = new DataObject();
                        dragObject.SetFileDropList(
                            new System.Collections.Specialized.StringCollection() { file.FullName });

                        try
                        {
                            DragDrop.DoDragDrop(button, dragObject,
                                (e.RightButton == MouseButtonState.Pressed
                                    ? DragDropEffects.All
                                    : DragDropEffects.Move));
                        }
                        catch
                        {
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
            file.InteractiveRenameEvent -= InteractiveRenameEvent;
        }

        private void UserControl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                // [Ctrl] + [C] => Copy
                case Key.C when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    CustomCommands.PerformAction(CustomCommands.Actions.Copy, file.FullName);
                    break;

                // [Ctrl] + [X] => Cut
                case Key.X when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    CustomCommands.PerformAction(CustomCommands.Actions.Cut, file.FullName);
                    break;

                // [Ctrl] + [V] => Paste
                case Key.V when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    if(file.IsDirectory)
                    {
                        // Paste item into folder?
                    }
                    break;

                // [Ctrl] + [Z] => Undo an action
                case Key.Z when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    break;

                // [Ctrl] + [Y] => Redo an action
                case Key.Y when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    break;

                // [Shift] + [Delete]  => Delete an item permanently without placing it into the Recycle Bin
                case Key.Delete when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift):
                    CustomCommands.PerformAction(CustomCommands.Actions.Delete, file.FullName);
                    break;

                // [Delete] => Delete an item and place it into the Recycle Bin
                case Key.Delete:
                    CustomCommands.PerformAction(CustomCommands.Actions.Delete, file.FullName);
                    break;

                // [Enter] => Open file properties
                case Key.Enter when Keyboard.Modifiers.HasFlag(ModifierKeys.Alt):
                    CustomCommands.PerformAction(CustomCommands.Actions.Properties, file.FullName);
                    break;

                // [Enter] => Open file
                case Key.Enter:
                    CustomCommands.PerformAction(CustomCommands.Actions.Open, file.FullName);
                    break;

                // [F2] => Rename Item. Select name excluding file extension
                case Key.F2:
                    CustomCommands.PerformAction(CustomCommands.Actions.Rename, file.FullName);
                    break;
            }
        }
    }
}