using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using CairoDesktop.Common.ViewModels;
using CairoDesktop.Configuration;

namespace CairoDesktop.Common.Views
{
    public partial class MonitorPreferenceView : UserControl
    {
        public MonitorPreferenceView()
        {
            InitializeComponent();

            DataContext = new MonitorPreferenceViewModel();
            Preference = ViewModel.Preference;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        MonitorPreferenceViewModel ViewModel => (MonitorPreferenceViewModel)DataContext;

        #region Preference property
        public MonitorPreference Preference
        {
            get { return (MonitorPreference)GetValue(PreferenceProperty); }
            set { SetValue(PreferenceProperty, value); }
        }
        public static readonly DependencyProperty PreferenceProperty =
            DependencyProperty.Register(nameof(Preference), typeof(MonitorPreference),
                                        ownerType: typeof(MonitorPreferenceView),
                                        new PropertyMetadata(defaultValue: null, PreferenceChanged));

        static void PreferenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue) return;

            var view = (MonitorPreferenceView)d;
            var preference = (MonitorPreference)e.NewValue;
            view.ViewModel.Preference = preference;
        }
        #endregion

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MonitorPreferenceViewModel.Preference):
                    Preference = ViewModel.Preference;
                    break;
            }
        }

        #region UI
        public string UsePrimaryScreenOptionTitle
        {
            get { return (string)GetValue(UsePrimaryScreenOptionTitleProperty); }
            set { SetValue(UsePrimaryScreenOptionTitleProperty, value); }
        }
        public static readonly DependencyProperty UsePrimaryScreenOptionTitleProperty =
            DependencyProperty.Register(nameof(UsePrimaryScreenOptionTitle), typeof(string),
                typeof(MonitorPreferenceView), new PropertyMetadata(""));
        #endregion
    }
}
