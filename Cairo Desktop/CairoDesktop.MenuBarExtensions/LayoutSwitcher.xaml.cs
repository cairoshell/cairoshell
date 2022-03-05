using ManagedShell.Common.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ManagedShell.Common.Structs;

namespace CairoDesktop.MenuBarExtensions
{

    public partial class LayoutSwitcher : UserControl
    {
        private KeyboardLayout LastZeroLayout;
        private KeyboardLayout LastForegroundLayout;
        public ObservableCollection<KeyboardLayout> CurrentLayouts { get; private set; }

        public LayoutSwitcher()
        {
            CurrentLayouts = new ObservableCollection<KeyboardLayout>();

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
            var newZeroLayout = KeyboardLayoutHelper.GetKeyboardLayout(true);
            var newForegroundLayout = KeyboardLayoutHelper.GetKeyboardLayout(false);

            if (newForegroundLayout.HKL != LastForegroundLayout.HKL)
            {
                currentLayout.Text = newForegroundLayout.ThreeLetterName;
            }
            else if (newZeroLayout.HKL != LastZeroLayout.HKL)
            {
                currentLayout.Text = newZeroLayout.ThreeLetterName;
            }

            LastForegroundLayout = newForegroundLayout;
            LastZeroLayout = newZeroLayout;
        }

        private void LayoutSwitcherItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            CurrentLayouts.Clear();
            var layouts = KeyboardLayoutHelper.GetKeyboardLayoutList();
            foreach (var layout in layouts) CurrentLayouts.Add(layout);
        }

        private void OpenLayoutSettings(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartProcess("control", "input.dll");
        }

        private void ChangeCurrentLayout(object sender, RoutedEventArgs e)
        {
            //TODO: PostMessage and LoadKeyboardLayout are inadequate with WPF-based programs. (and Cairo)
            KeyboardLayoutHelper.SetKeyboardLayout(((KeyboardLayout)((sender as MenuItem).DataContext)).HKL);
        }
    }
}
