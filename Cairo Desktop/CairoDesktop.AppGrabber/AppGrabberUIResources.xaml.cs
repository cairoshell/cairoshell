using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop.AppGrabber
{
    partial class AppGrabberUIResources : ResourceDictionary
    {
        bool isDragging = false;

        private void DelCatButtonShown(object sender, EventArgs e)
        {
            Button button = sender as Button;
            Category actionableCategory = button.CommandParameter as Category;
            if (actionableCategory.Type > 0)
            {
                button.Visibility = Visibility.Collapsed;
            }
        }

        private void Category_Block_DoubleClick(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                TextBlock block = sender as TextBlock;
                // Do not allow special category to be renamed.
                if ((block.DataContext as Category).Type > 0) return;

                foreach (UIElement peer in (block.Parent as DockPanel).Children)
                {
                    if (peer is TextBox)
                    {
                        peer.Visibility = Visibility.Visible;
                        peer.Focus();
                        (peer as TextBox).ContextMenu = null;
                        (peer as TextBox).SelectAll();
                    }
                }
                block.Visibility = Visibility.Collapsed;
            }
        }

        private void CategoryButtonClicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Category actionableCategory = button.CommandParameter as Category;
            CategoryList catList = actionableCategory.ParentCategoryList;
            switch (button.Content as String)
            {
                case "5":
                    catList.MoveCategory(actionableCategory, -1);
                    break;
                case "6":
                    catList.MoveCategory(actionableCategory, 1);
                    break;
                case "r":
                    //Don't allow removal of special category
                    if (actionableCategory.Type > 0) return;

                    Category uncategorized = catList.GetSpecialCategory(AppCategoryType.Uncategorized);
                    for (int i = actionableCategory.Count - 1; i >= 0; i--)
                    {
                        ApplicationInfo app = actionableCategory[i];
                        actionableCategory.RemoveAt(i);
                        uncategorized.Add(app);
                    }
                    catList.Remove(actionableCategory);
                    break;
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void Category_Label_Edit_End(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            foreach (UIElement peer in (box.Parent as DockPanel).Children)
            {
                if (peer is TextBlock)
                {
                    peer.Visibility = Visibility.Visible;
                }
            }
            box.Visibility = Visibility.Collapsed;
        }

        private void ListView_Focus_Lost(object sender, RoutedEventArgs e)
        {
            ListView view = sender as ListView;
            view.SelectedIndex = -1;
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            if (isDragging && e.Data.GetData(typeof(ApplicationInfo)) is ApplicationInfo)
            {
                e.Effects = DragDropEffects.Move | DragDropEffects.Copy;
            }
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                CairoLogger.Instance.Debug(e.Data.GetData(typeof(ApplicationInfo)).ToString());
                ApplicationInfo dropData = e.Data.GetData(typeof(ApplicationInfo)) as ApplicationInfo;
                ListView dropTarget = sender as ListView;

                if (dropTarget.ItemsSource is Category)
                {
                    Category target = dropTarget.ItemsSource as Category;

                    if (target.Type == AppCategoryType.QuickLaunch)
                    {
                        e.Effects = DragDropEffects.Copy;

                        // Do not duplicate entries
                        if (!target.Contains(dropData))
                        {
                            ApplicationInfo dropClone = dropData.Clone();

                            if (e.OriginalSource != null && e.OriginalSource is FrameworkElement && (e.OriginalSource as FrameworkElement).DataContext != null && (e.OriginalSource as FrameworkElement).DataContext is ApplicationInfo)
                                target.Insert(target.IndexOf((e.OriginalSource as FrameworkElement).DataContext as ApplicationInfo), dropClone);
                            else
                                target.Add(dropClone);

                            dropClone.Icon = null; // icon may differ depending on category
                            dropClone.IconPath = null;
                        }
                        else
                        {
                            // reorder existing
                            if (e.OriginalSource != null && e.OriginalSource is FrameworkElement && (e.OriginalSource as FrameworkElement).DataContext != null && (e.OriginalSource as FrameworkElement).DataContext is ApplicationInfo)
                                target.Move(target.IndexOf(dropData), target.IndexOf((e.OriginalSource as FrameworkElement).DataContext as ApplicationInfo));
                        }
                    }
                    else if (sourceView != null && sourceView != sender)
                    {
                        e.Effects = DragDropEffects.Move;

                        Category source = sourceView.ItemsSource as Category;

                        source.Remove(dropData);

                        if (source.Type != AppCategoryType.QuickLaunch)
                        {
                            target.Add(dropData); // if coming from quick launch, simply remove from quick launch

                            if (dropTarget.Items.Contains(dropData))
                            {
                                dropTarget.ScrollIntoView(dropTarget.Items[dropTarget.Items.IndexOf(dropData)]);
                            }
                        }
                    }
                }
                else
                {
                    e.Effects = DragDropEffects.Move;

                    (sourceView.ItemsSource as IList<ApplicationInfo>).Remove(dropData);
                    (dropTarget.ItemsSource as IList<ApplicationInfo>).Add(dropData);

                    if (dropTarget.Items.Contains(dropData))
                    {
                        dropTarget.ScrollIntoView(dropTarget.Items[dropTarget.Items.IndexOf(dropData)]);
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (fileNames != null)
                {
                    ListView dropTarget = sender as ListView;

                    if (!(dropTarget.ItemsSource is Category))
                    {
                        foreach (String fileName in fileNames)
                        {
                            CairoLogger.Instance.Debug(fileName);

                            if (Shell.Exists(fileName))
                            {
                                ApplicationInfo customApp = AppGrabber.PathToApp(fileName, false, false);
                                if (!object.ReferenceEquals(customApp, null))
                                {
                                    (dropTarget.ItemsSource as IList<ApplicationInfo>).Add(customApp);

                                    if (dropTarget.Items.Contains(customApp))
                                    {
                                        dropTarget.ScrollIntoView(dropTarget.Items[dropTarget.Items.IndexOf(customApp)]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            sourceView = null;
            isDragging = false;
        }

        private void ListView_DragLeave(object sender, DragEventArgs e)
        {
            (sender as ListView).AllowDrop = true;
            e.Effects = DragDropEffects.Move | DragDropEffects.Copy;
        }

        ListView sourceView;
        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.OriginalSource.GetType() != typeof(System.Windows.Controls.Primitives.Thumb) && e.OriginalSource.GetType() != typeof(System.Windows.Controls.Primitives.RepeatButton) && e.OriginalSource.GetType() != typeof(System.Windows.Controls.Primitives.Track))
            {
                sourceView = sender as ListView;

                if (sourceView.SelectedItem != null)
                {
                    isDragging = true;
                    try
                    {
                        DragDrop.DoDragDrop(sourceView, sourceView.SelectedItem, DragDropEffects.Move | DragDropEffects.Copy);
                    }
                    catch (Exception ex)
                    {
                        //Output the reason to the debugger
                        CairoLogger.Instance.Error("Error doing Drag-Drop from appgrabber:" + ex.Message, ex);
                    }
                }
            }
        }

        // Moving Categories Around
        private void TextBlock_PreviewDragEnter(object sender, DragEventArgs e)
        {
            string[] formats = e.Data.GetFormats();
            bool b = e.Data.GetDataPresent(typeof(Category));
            if (isDragging && e.Data.GetDataPresent(typeof(Category)) && (e.AllowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
            {
                if (e.Data.GetData(typeof(Category)) as Category != (sender as TextBlock).DataContext as Category)
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
            if (isDragging && e.Data.GetDataPresent(typeof(ApplicationInfo)) && (e.AllowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void TextBlock_Drop(object sender, DragEventArgs e)
        {
            TextBlock dropBlock = sender as TextBlock;
            Category dropCategory = dropBlock.DataContext as Category;

            if (e.Data.GetDataPresent(typeof(Category)))
            {
                CairoLogger.Instance.Debug(e.Data.GetData(typeof(Category)).ToString());
                Category dropData = e.Data.GetData(typeof(Category)) as Category;

                CategoryList parent = dropCategory.ParentCategoryList;
                int initialIndex = parent.IndexOf(dropData);
                int dropIndex = parent.IndexOf(dropCategory);
                parent.Move(initialIndex, dropIndex);
            }
            else if (e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                ApplicationInfo dropData = e.Data.GetData(typeof(ApplicationInfo)) as ApplicationInfo;
                if (dropCategory.Type == AppCategoryType.QuickLaunch)
                {
                    e.Effects = DragDropEffects.Copy;

                    // Do not duplicate entries
                    if (!dropCategory.Contains(dropData))
                    {
                        ApplicationInfo dropClone = dropData.Clone();
                        dropCategory.Add(dropClone);
                        dropClone.Icon = null; // icon may differ depending on category
                        dropClone.IconPath = null;
                    }
                }
                else if (sourceView != null)
                {
                    e.Effects = DragDropEffects.Move;

                    (sourceView.ItemsSource as Category).Remove(dropData);
                    if ((sourceView.ItemsSource as Category).Type != AppCategoryType.QuickLaunch)
                    {
                        dropCategory.Add(dropData);
                    }
                }

                sourceView = null;
            }

            isDragging = false;
        }

        private void TextBlock_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !isDragging)
            {
                isDragging = true;
                TextBlock sourceBlock = sender as TextBlock;
                Category selectedCategory = sourceBlock.DataContext as Category;
                try
                {
                    DragDrop.DoDragDrop(sourceBlock, selectedCategory, DragDropEffects.Move);
                }
                catch (Exception ex)
                {
                    //Output the reason to the debugger
                    CairoLogger.Instance.Error("Error doing Drag-Drop from AppGrabber TextBlock. Details: " + ex.Message , ex);
                }
            }
        }
    }
}
