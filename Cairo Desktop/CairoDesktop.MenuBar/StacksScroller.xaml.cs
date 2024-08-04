using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.ShellFolders;
using ManagedShell.ShellFolders.Enums;

namespace CairoDesktop.MenuBar
{
    /// <summary>
    /// Interaction logic for StacksScroller.xaml
    /// </summary>
    public partial class StacksScroller : UserControl
    {
        public static DependencyProperty ParentContainerProperty = DependencyProperty.Register("ParentContainer", typeof(StacksContainer), typeof(StacksScroller), new PropertyMetadata(null));
        public StacksContainer ParentContainer
        {
            get { return (StacksContainer)GetValue(ParentContainerProperty); }
            set { SetValue(ParentContainerProperty, value); }
        }

        public MenuItem ParentMenuItem;
        internal ICommandService _commandService;
        
        private bool isLoaded;
        private Icon LastIconSelected;
        private Dictionary<uint, string> ContextMenuCommandUIDs = new Dictionary<uint, string>();

        public StacksScroller()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            Close();

            ParentContainer?.OpenDir((sender as ICommandSource).CommandParameter.ToString(), !Settings.Instance.FoldersOpenDesktopOverlay || KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey));
        }

        private void Scroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender != null)
            {
                var scrollViewer = VisualTreeHelper.GetChild(sender as ListView, 0) as ScrollViewer;

                if (scrollViewer == null)
                    return;

                if (e.Delta < 0)
                    scrollViewer.LineRight();
                else
                    scrollViewer.LineLeft();

                e.Handled = true;
            }
        }

        private void StacksScroller_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                Binding widthBinding = new Binding("PopupWidth");
                widthBinding.Source = ParentContainer;
                Scroller.SetBinding(WidthProperty, widthBinding);

                if (DataContext is ShellFolder folder)
                {
                    Scroller.ItemsSource = new FolderView(folder).DisplayItems;
                }

                if (Settings.Instance.EnableDynamicDesktop && Settings.Instance.FoldersOpenDesktopOverlay &&
                    Settings.Instance.EnableDesktop && !GroupPolicyHelper.NoDesktop)
                {
                    OpenButton.ToolTip = new ToolTip { Content = DisplayString.sStacks_OpenOnDesktop };
                }

                isLoaded = true;
            }
        }

        private void Close()
        {
            try
            {
                // For some reason the framework throws an NRE here if you attempt to drag a non-filesystem file, so catch it
                if (ParentMenuItem != null)
                {
                    ParentMenuItem.IsSubmenuOpen = false;

                    // Stacks capture the mouse; need to release so that mouse events go to the intended recipient after closing
                    Mouse.Capture(null);
                }
            }
            catch (Exception e)
            {
                ShellLogger.Warning($"StacksScroller: Unable to close stack: {e.Message}");
            }
            
        }
        
        #region Icons
        private void Icon_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Icon icon = sender as Icon;
            if (icon == null || icon.IsRenaming)
            {
                return;
            }

            e.Handled = true;

            ShellFile file = icon.DataContext as ShellFile;
            
            Close();

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

            icon.CaptureMouse();
            ShellFile file = icon.DataContext as ShellFile;
            LastIconSelected = icon;

            if (InvokeContextMenu(file, true))
            {
                e.Handled = true;

                if (LastIconSelected != null && LastIconSelected.IsRenaming)
                {
                    LastIconSelected = null;
                    
                    // user selected rename, so keep things open
                    return;
                }

                LastIconSelected = null;

                Close();
            }
        }
        #endregion

        #region Context menu
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
                if (Settings.Instance.FoldersOpenDesktopOverlay && ContextMenuCommandUIDs.ContainsValue("OpenLocation") && !KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                {
                    builder.DefaultItemUID = ContextMenuCommandUIDs.FirstOrDefault(item => item.Value == "OpenLocation").Key;
                }

                builder.AddSeparator();
            }

            return builder;
        }

        private bool InvokeContextMenu(ShellFile file, bool isInteractive)
        {
            if (file == null)
            {
                return false;
            }
            
            var _ = new ShellItemContextMenu(new ShellItem[] { file }, DataContext as ShellFolder, IntPtr.Zero, HandleFileAction, isInteractive, GetFileCommandBuilder(file), new ShellMenuCommandBuilder());
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
                    return true; // don't reset LastIconSelected yet so that we can tell that it entered rename mode
                default:
                    // handle Cairo actions
                    if (uint.TryParse(action, out uint cairoAction) && ContextMenuCommandUIDs.TryGetValue(cairoAction, out string identifier))
                    {
                        _commandService.InvokeCommand(identifier, ("Path", items[0].Path));
                        handled = true;
                    }
                    break;
            }

            LastIconSelected = null;
            return handled;
        }
        #endregion
    }
}
