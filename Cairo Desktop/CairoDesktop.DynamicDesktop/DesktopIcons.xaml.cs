using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using CairoDesktop.DynamicDesktop.Services;
using ManagedShell.Common.Helpers;
using ManagedShell.ShellFolders;
using ManagedShell.ShellFolders.Enums;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;
using System.Collections.Generic;

namespace CairoDesktop.DynamicDesktop
{
    /// <summary>
    /// Interaction logic for DesktopIcons.xaml
    /// </summary>
    public partial class DesktopIcons : UserControl
    {
        private readonly ICommandService _commandService;
        private readonly DesktopManager _desktopManager;
        private readonly Settings _settings;

        internal bool RenameNewIcon;
        private Icon LastIconSelected;
        private Dictionary<uint, string> ContextMenuCommandUIDs = new Dictionary<uint, string>();

        public static DependencyProperty locationProperty =
            DependencyProperty.Register("Location",
                typeof(ShellFolder),
                typeof(DesktopIcons),
                new PropertyMetadata(null));

        public ShellFolder Location
        {
            get
            {
                return GetValue(locationProperty) as ShellFolder;
            }
            set
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() =>
                    {
                        Location = value;
                        IconsControl.ItemsSource = new FolderView(value).DisplayItems;
                    });
                    return;
                }

                SetValue(locationProperty, value);
                IconsControl.ItemsSource = new FolderView(value).DisplayItems;
            }
        }

        public DesktopIcons(DesktopManager manager, ICommandService commandService, Settings settings)
        {
            InitializeComponent();

            _commandService = commandService;
            _desktopManager = manager;
            _settings = settings;

            _settings.PropertyChanged += Settings_PropertyChanged;

            setPosition();
            SetIconLocation();
        }

        public RenderTargetBitmap GenerateBitmap(Grid sourceGrid)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(sourceGrid.ActualWidth * DpiHelper.DpiScale), (int)(sourceGrid.ActualHeight * DpiHelper.DpiScale), DpiHelper.DpiScale * 96, DpiHelper.DpiScale * 96, PixelFormats.Default);
            bmp.Render(this);
            bmp.Freeze();

            return bmp;
        }

        public void SetIconLocation()
        {
            IntPtr desktopPtr = IntPtr.Zero;
            if (_desktopManager.IsOverlayOpen && _desktopManager.DesktopOverlayWindow != null)
            {
                desktopPtr = _desktopManager.DesktopOverlayWindow.Handle;
            }
            else if (_desktopManager.DesktopWindow != null)
            {
                desktopPtr = _desktopManager.DesktopWindow.Handle;
            }
            
            Location = new ShellFolder(_desktopManager.NavigationManager.CurrentItem.Path, desktopPtr, true);
            
            // set the display name for navigation history
            _desktopManager.NavigationManager.CurrentItem.DisplayName = Location.DisplayName;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "DesktopLabelPosition":
                        setPosition();
                        break;
                }
            }
        }

        private void setPosition()
        {
            int xOffset = 7;
            int yOffset = 13;

            if (_settings.DesktopLabelPosition == 1)
                xOffset = 0;

            panel.Margin = new Thickness(xOffset, yOffset, 0, 0);
        }

        #region Events
        
        private void IconsControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = VisualTreeHelper.GetChild(sender as ItemsControl, 0) as ScrollViewer;

            if (scrollViewer == null)
                return;

            if (e.Delta < 0)
                scrollViewer.LineRight();
            else
                scrollViewer.LineLeft();

            e.Handled = true;
        }

        private void Icon_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Icon icon = sender as Icon;
            if (icon == null || icon.IsRenaming)
            {
                return;
            }

            e.Handled = true;
            
            ShellFile file = icon.DataContext as ShellFile;

            if (file == null || string.IsNullOrWhiteSpace(file.Path))
            {
                CairoMessage.Show(DisplayString.sError_FileNotFoundInfo, DisplayString.sError_OhNo, MessageBoxButton.OK,
                    CairoMessageImage.Error);
                return;
            }

            if (!InvokeContextMenu(file, false))
            {
                CairoMessage.Show(DisplayString.sError_FileNotFoundInfo, DisplayString.sError_OhNo, MessageBoxButton.OK,
                    CairoMessageImage.Error);
            }
        }

        private void Icon_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Icon icon = sender as Icon;
            if (icon == null || icon.IsRenaming)
            {
                return;
            }
            
            ShellFile file = icon.DataContext as ShellFile;
            LastIconSelected = icon;
            
            if (InvokeContextMenu(file, true))
            {
                e.Handled = true;
            }
        }

        #endregion
        private ShellMenuCommandBuilder GetFileCommandBuilder(ShellFile file)
        {
            if (file == null)
            {
                return new ShellMenuCommandBuilder();
            }

            ShellMenuCommandBuilder builder = new ShellMenuCommandBuilder();

            var commands = _commandService.Commands.Where(command => command is ICairoShellItemCommandInfo);
            uint uid = (uint)Enum.GetValues(typeof(CommonContextMenuItem)).Cast<CommonContextMenuItem>().Max();

            ContextMenuCommandUIDs.Clear();

            foreach (var command in commands)
            {
                if (command is ICairoShellItemCommandInfo fileCommand && fileCommand.IsAvailableForShellItem(file))
                {
                    uid++;
                    ContextMenuCommandUIDs.Add(uid, command.Identifier);
                    builder.AddCommand(new ShellMenuCommand
                    {
                        Flags = MFT.BYCOMMAND,
                        Label = fileCommand.LabelForShellItem(file),
                        UID = uid
                    });
                }
            }

            if (builder.Commands.Count > 0)
            {
                // If the [SHIFT] key is held, don't change the default action to ours
                // Only set as the default action for filesystem items because we don't support all shell views
                if (ContextMenuCommandUIDs.ContainsValue("OpenLocation") && !KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                {
                    builder.DefaultItemUID = ContextMenuCommandUIDs.FirstOrDefault(item => item.Value == "OpenLocation").Key;
                }

                builder.AddSeparator();
            }

            return builder;
        }

        private bool InvokeContextMenu(ShellFile file, bool isInteractive)
        {
            if (_desktopManager == null || file == null)
            {
                return false;
            }

            IntPtr desktopPtr = IntPtr.Zero;
            if (_desktopManager.IsOverlayOpen && _desktopManager.DesktopOverlayWindow != null)
            {
                desktopPtr = _desktopManager.DesktopOverlayWindow.Handle;
            }
            else if (_desktopManager.DesktopWindow != null)
            {
                desktopPtr = _desktopManager.DesktopWindow.Handle;
            }

            var _ = new ShellItemContextMenu(new ShellItem[] { file }, _desktopManager.DesktopLocation, desktopPtr, HandleFileAction, isInteractive, GetFileCommandBuilder(file), new ShellMenuCommandBuilder());
            return true;
        }

        private bool HandleFileAction(string action, ShellItem[] items, bool allFolders)
        {
            if (items.Length < 1)
            {
                return false;
            }

            bool handled = false;
            switch (action)
            {
                case CommonContextMenuVerb.Rename:
                    LastIconSelected?.BeginRename();
                    handled = true;
                    break;
                default:
                    // handle Cairo actions
                    if (uint.TryParse(action, out uint cairoAction) && ContextMenuCommandUIDs.TryGetValue(cairoAction, out string identifier))
                    {
                        _commandService.InvokeCommand(identifier, ("Path", items[0].Path), ("NoOverlay", true));
                        handled = true;
                    }
                    else if (action != CommonContextMenuVerb.Cut && action != CommonContextMenuVerb.Copy && action != CommonContextMenuVerb.Delete)
                    {
                        // these actions are handled for us, but we should hide the desktop overlay
                        _desktopManager.IsOverlayOpen = false;
                    }
                    break;
            }

            LastIconSelected = null;
            return handled;
        }

        private void Icon_OnIconLoaded(object sender, EventArgs e)
        {
            // Whenever a new icon loads, check if the RenameNewIcon flag was set.
            // The "New" menu sets this flag.
            // If set, enter rename mode, because this file was created from that menu.
            
            if (!RenameNewIcon)
            {
                return;
            }

            Icon icon = sender as Icon;

            if (icon == null)
            {
                return;
            }

            RenameNewIcon = false;
            icon.BeginRename();
        }

        private void Icon_OnKeyUp(object sender, KeyEventArgs e)
        {
            // Close desktop overlay if certain key commands invoked.
            
            Icon icon = sender as Icon;

            if (icon == null)
            {
                return;
            }
            
            if (icon.IsRenaming)
            {
                return;
            }

            switch (e.Key)
            {
                // [Ctrl] + [C] => Copy
                case Key.C when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    _commandService.InvokeCommand("CopyFile", ("Path", icon.File.Path));
                    break;

                // [Ctrl] + [X] => Cut
                case Key.X when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    // TODO: Not yet implemented
                    break;

                // [Ctrl] + [V] => Paste
                case Key.V when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    if (icon.File.IsFolder)
                    {
                        // TODO: Paste item into folder?
                    }
                    break;

                // [Delete] => Delete an item and place it into the Recycle Bin
                case Key.Delete:
                    _commandService.InvokeCommand("DeleteFile", ("Path", icon.File.Path));
                    break;

                // TODO: [Alt] + [Enter] => Open file properties
                case Key.Enter when Keyboard.Modifiers.HasFlag(ModifierKeys.Alt):
                    _desktopManager.IsOverlayOpen = false;
                    _commandService.InvokeCommand("ShowFileProperties", ("Path", icon.File.Path));
                    break;

                // [Enter] => Open file
                case Key.Enter:
                    if (!icon.File.IsFolder) _desktopManager.IsOverlayOpen = false;
                    _commandService.InvokeCommand(icon.File.IsFolder ? "OpenLocation" : "OpenFile", ("Path", icon.File.Path), ("NoOverlay", true));
                    break;

                // TODO: [F2] => Rename Item. Select name excluding file extension
                case Key.F2:
                    // TODO: Not yet implemented
                    break;
            }
        }
    }
}
