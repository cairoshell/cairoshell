using ManagedShell.Common.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class LayoutEntry
    {
        public int Layout { get; set; }
        public string Name
        {
            get
            {
                return CultureInfo.GetCultureInfo((short)Layout).NativeName;
            }
        }
    }

    public partial class LayoutSwitcher : UserControl
    {
        private int LastZeroLayout;
        private int LastForegroundLayout;
        public ObservableCollection<LayoutEntry> CurrentLayouts { get; private set; }

        public LayoutSwitcher()
        {
            CurrentLayouts = new ObservableCollection<LayoutEntry>();

            InitializeComponent();

            InitializeLayoutIndicator();
        }

        private void InitializeLayoutIndicator()
        {
            LayoutIndicator_Tick();

            DispatcherTimer layoutIconTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, delegate
            {
                LayoutIndicator_Tick();
            }, Dispatcher);
        }

        private void LayoutIndicator_Tick()
        {
            //HACK: I don't why, but GetKeyboardLayout with foreground window PID doesn't update when explorer.exe is focused.
            //(ex. try to click desktop background and try to change layout)
            //but: with zero PID it updates when explorer.exe or Cairo is focused only.
            var newZeroLayout = LayoutHelper.GetKeyboardLayout(true);
            var newForegroundLayout = LayoutHelper.GetKeyboardLayout(false);

            if (newForegroundLayout != LastForegroundLayout)
            {
                currentLayout.Text = CultureInfo.GetCultureInfo((short) newForegroundLayout).ThreeLetterISOLanguageName.ToUpper();
            } else if (newZeroLayout != LastZeroLayout)
            {
                currentLayout.Text = CultureInfo.GetCultureInfo((short) newZeroLayout).ThreeLetterISOLanguageName.ToUpper();
            }

            LastForegroundLayout = newForegroundLayout;
            LastZeroLayout = newZeroLayout;
        }

        private void LayoutSwitcherItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            CurrentLayouts.Clear();
            var layouts = LayoutHelper.GetKeyboardLayoutList();

            foreach (var layout in layouts)
            {
                CurrentLayouts.Add(new LayoutEntry()
                {
                    Layout = (int) layout
                });
            }
        }

        private void OpenLayoutSettings(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartProcess("control", "input.dll");
        }

        private void ChangeCurrentLayout(object sender, RoutedEventArgs e)
        {
            //TODO: PostMessage and LoadKeyboardLayout are inadequate with WPF-based programs. (and Cairo)
            LayoutHelper.SetKeyboardLayout(((sender as MenuItem).DataContext as LayoutEntry).Layout);
        }
    }
}
