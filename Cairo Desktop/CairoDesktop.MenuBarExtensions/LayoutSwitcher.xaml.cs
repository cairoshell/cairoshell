﻿using ManagedShell.Common.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ManagedShell.Common.Structs;
using CairoDesktop.Configuration;

namespace CairoDesktop.MenuBarExtensions
{

    public partial class LayoutSwitcher : UserControl
    {
        private KeyboardLayout LastZeroLayout;
        private KeyboardLayout LastForegroundLayout;
        public ObservableCollection<KeyboardLayout> AvaliableLayouts { get; private set; }
        public KeyboardLayout CurrentLayout { get; private set; }

        public LayoutSwitcher()
        {
            AvaliableLayouts = new ObservableCollection<KeyboardLayout>();

            InitializeComponent();

            InitializeLayoutIndicator();
        }

        private void InitializeLayoutIndicator()
        {
            LayoutIndicator_Tick();

            DispatcherTimer layoutIconTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 200), DispatcherPriority.Background, delegate
            {
                LayoutIndicator_Tick();
            }, Dispatcher);
        }

        private void LayoutIndicator_Tick()
        {
            var newForegroundLayout = KeyboardLayoutHelper.GetKeyboardLayout();
            if (newForegroundLayout.HKL != LastForegroundLayout.HKL)
            {
                CurrentLayout = newForegroundLayout;
            }
            LastForegroundLayout = newForegroundLayout;

            if (Settings.Instance.EnableDesktop)
            {
                var newZeroLayout = KeyboardLayoutHelper.GetKeyboardLayout(true);
                if (newZeroLayout.HKL != LastZeroLayout.HKL)
                {
                    CurrentLayout = newZeroLayout;
                }
                LastZeroLayout = newZeroLayout;
            }

            CurrentLayoutBlock.Text = CurrentLayout.ThreeLetterName;
        }

        private void LayoutSwitcherItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            AvaliableLayouts.Clear();
            KeyboardLayoutHelper.GetKeyboardLayoutList().ForEach(AvaliableLayouts.Add);
        }

        private void OpenLayoutSettings(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartProcess("control", "input.dll");
        }

        private void ChangeCurrentLayout(object sender, RoutedEventArgs e)
        {
            KeyboardLayoutHelper.SetKeyboardLayout(((KeyboardLayout)(sender as MenuItem).DataContext).HKL);
        }
    }
}
