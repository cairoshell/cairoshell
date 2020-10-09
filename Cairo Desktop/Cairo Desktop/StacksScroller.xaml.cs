using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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

        private bool isLoaded;

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
    }
}
