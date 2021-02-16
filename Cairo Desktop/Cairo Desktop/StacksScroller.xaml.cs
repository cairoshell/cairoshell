using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CairoDesktop.Common;
using CairoDesktop.Localization;
using CairoDesktop.SupportingClasses;
using ManagedShell.ShellFolders;
using ManagedShell.ShellFolders.Enums;

namespace CairoDesktop
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

        private bool isLoaded;
        private Icon LastIconSelected;

        public StacksScroller()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                ParentContainer?.OpenDir((sender as ICommandSource).CommandParameter.ToString(), true);
            }
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

                isLoaded = true;
            }
        }

        private void Icon_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Icon icon = sender as Icon;
            if (icon == null || icon.IsRenaming)
            {
                return;
            }

            e.Handled = true;

            ShellFile file = icon.DataContext as ShellFile;

            if (ParentMenuItem != null)
            {
                ParentMenuItem.IsSubmenuOpen = false;
            }

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

                LastIconSelected = null;
                
                if (ParentMenuItem != null)
                {
                    ParentMenuItem.IsSubmenuOpen = false;
                }
            }
        }

        private ShellMenuCommandBuilder GetFileCommandBuilder(ShellFile file)
        {
            if (file == null)
            {
                return new ShellMenuCommandBuilder();
            }

            ShellMenuCommandBuilder builder = new ShellMenuCommandBuilder();

            if (file.IsFolder)
            {
                builder.AddCommand(new ShellMenuCommand
                {
                    Flags = MFT.BYCOMMAND, // enable this entry always
                    Label = DisplayString.sStacks_OpenInNewWindow,
                    UID = (uint)CairoContextMenuItem.OpenInNewWindow
                });

                if (StacksManager.Instance.StackLocations.All(i => i.Path != file.Path))
                {
                    builder.AddCommand(new ShellMenuCommand
                    {
                        Flags = MFT.BYCOMMAND, // enable this entry always
                        Label = DisplayString.sInterface_AddToStacks,
                        UID = (uint)CairoContextMenuItem.AddToStacks
                    });
                }
                else
                {
                    builder.AddCommand(new ShellMenuCommand
                    {
                        Flags = MFT.BYCOMMAND, // enable this entry always
                        Label = DisplayString.sInterface_RemoveFromStacks,
                        UID = (uint)CairoContextMenuItem.RemoveFromStacks
                    });
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

        private void HandleFileAction(string action, ShellItem[] items)
        {
            // TODO: Use command system
            if (items.Length < 1)
            {
                return;
            }

            switch (action)
            {
                case CustomCommands.Actions.OpenFolder:
                    // Determine if [SHIFT] key is held. Bypass directory processing, which will use the Shell to open the item.
                    if (KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                    {
                        FolderHelper.OpenWithShell(items[0].Path);
                    }
                    else
                    {
                        FolderHelper.OpenLocation(items[0].Path);
                    }
                    break;
                case CustomCommands.Actions.Rename:
                    LastIconSelected?.BeginRename();
                    break;
                default:
                    // handle Cairo actions
                    if (uint.TryParse(action, out uint cairoAction))
                    {
                        switch ((CairoContextMenuItem)cairoAction)
                        {
                            case CairoContextMenuItem.AddToStacks:
                                CustomCommands.PerformAction(CustomCommands.Actions.AddStack, items[0].Path);
                                break;
                            case CairoContextMenuItem.RemoveFromStacks:
                                CustomCommands.PerformAction(CustomCommands.Actions.RemoveStack, items[0].Path);
                                break;
                            case CairoContextMenuItem.OpenInNewWindow:
                                CustomCommands.PerformAction(CustomCommands.Actions.OpenWithShell, items[0].Path);
                                break;
                        }
                    }
                    break;
            }

            LastIconSelected = null;
        }
    }
}
