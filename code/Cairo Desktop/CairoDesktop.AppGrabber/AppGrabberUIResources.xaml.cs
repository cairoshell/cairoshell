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
            if (actionableCategory.Name == "Uncategorized")
            {
                button.Visibility = Visibility.Collapsed;
            }
            if (actionableCategory.Name == "Quick Launch")
            {
                button.Visibility = Visibility.Collapsed;
            }
        }

        private void Category_Block_DoubleClick(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                TextBlock block = sender as TextBlock;
                // Do not allow the Uncategorized category to be renamed.
                if (block.Text == "Uncategorized") return;
                if (block.Text == "Quick Launch") return;
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
                case "<":
                    catList.MoveCategory(actionableCategory, -1);
                    break;
                case ">":
                    catList.MoveCategory(actionableCategory, 1);
                    break;
                case "X":
                    //Don't allow removal of uncategorized or quick launch
                    if (actionableCategory.Name == "Quick Launch") return;
                    if (actionableCategory.Name == "Uncategorized") return;
                    Category uncategorized = catList.GetCategory("Uncategorized");
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
            if (e.Data.GetDataPresent(typeof(ApplicationInfo)) && sourceView != sender)
            {
                System.Diagnostics.Debug.WriteLine(e.Data.GetData(typeof(ApplicationInfo)).ToString());
                ListView dropTarget = sender as ListView;
                if (dropTarget.ItemsSource is Category)
                {
                    if ((dropTarget.ItemsSource as Category).Name == "Quick Launch")
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                }
                ApplicationInfo dropData = e.Data.GetData(typeof(ApplicationInfo)) as ApplicationInfo;
                if (e.Effects == DragDropEffects.Move)
                {
                    (sourceView.ItemsSource as IList<ApplicationInfo>).Remove(dropData);
                    if (dropTarget.ItemsSource is Category)
                    {
                        if ((sourceView.ItemsSource as Category).Name != "Quick Launch")
                        {
                            ((dropTarget.ItemsSource) as IList<ApplicationInfo>).Add(dropData);
                        }
                    }
                    else
                    {
                        ((dropTarget.ItemsSource) as IList<ApplicationInfo>).Add(dropData);
                    }
                }
                else if (e.Effects == DragDropEffects.Copy)
                {
                    ApplicationInfo dropClone = dropData.Clone();
                    // Do not duplicate entries
                    if (!((dropTarget.ItemsSource) as Category).Contains(dropClone))
                    {
                        ((dropTarget.ItemsSource) as Category).Add(dropClone);
                    }
                }
                if (dropTarget.Items.Count > 0)
                {
                    dropTarget.ScrollIntoView(dropTarget.Items[dropTarget.Items.Count - 1]);
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
            if (e.LeftButton == MouseButtonState.Pressed)
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
                        System.Diagnostics.Debug.WriteLine("Error doing Drag-Drop from appgrabber. Details: " + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
        }

        // Moving Categories Arround
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
            if (e.Data.GetDataPresent(typeof(Category)))
            {
                TextBlock dropBlock = sender as TextBlock;
                Category replacedCategory = dropBlock.DataContext as Category;

                System.Diagnostics.Debug.WriteLine(e.Data.GetData(typeof(Category)).ToString());
                Category dropData = e.Data.GetData(typeof(Category)) as Category;

                CategoryList parent = replacedCategory.ParentCategoryList;
                int initialIndex = parent.IndexOf(dropData);
                int dropIndex = parent.IndexOf(replacedCategory);
                parent.Move(initialIndex, dropIndex);
            }
            else if (e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                if (((sender as TextBlock).DataContext as Category).Name == "Quick Launch")
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                }
                ApplicationInfo dropData = e.Data.GetData(typeof(ApplicationInfo)) as ApplicationInfo;
                Category dropCollection = (sender as TextBlock).DataContext as Category;
                if (e.Effects == DragDropEffects.Move)
                {
                    (sourceView.ItemsSource as IList<ApplicationInfo>).Remove(dropData);
                    if ((sourceView.ItemsSource as Category).Name != "Quick Launch")
                    {
                        dropCollection.Add(dropData);
                    }
                }
                else if (e.Effects == DragDropEffects.Copy)
                {
                    ApplicationInfo dropClone = dropData.Clone();
                    // Do not duplicate entries
                    if (!dropCollection.Contains(dropClone))
                    {
                        dropCollection.Add(dropClone);
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
                    System.Diagnostics.Debug.WriteLine("Error doing Drag-Drop from AppGrabber TextBlock. Details: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
    }
}
