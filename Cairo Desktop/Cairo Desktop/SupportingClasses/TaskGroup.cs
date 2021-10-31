using CairoDesktop.Configuration;
using ManagedShell.Common.Enums;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using ManagedShell.UWPInterop;
using ManagedShell.WindowsTasks;
using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CairoDesktop.SupportingClasses
{
    public class TaskGroup : INotifyPropertyChanged, IDisposable
    {
        private string _title;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private ImageSource _icon;

        public ImageSource Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        private ApplicationWindow.WindowState _state;

        public ApplicationWindow.WindowState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        private ImageSource _overlayIcon;

        public ImageSource OverlayIcon
        {
            get
            {
                return _overlayIcon;
            }
            private set
            {
                _overlayIcon = value;
                OnPropertyChanged();
            }
        }

        private string _overlayIconDescription;

        public string OverlayIconDescription
        {
            get
            {
                return _overlayIconDescription;
            }
            private set
            {
                _overlayIconDescription = value;
                OnPropertyChanged();
            }
        }

        private NativeMethods.TBPFLAG _progressState;

        public NativeMethods.TBPFLAG ProgressState
        {
            get
            {
                return _progressState;
            }

            set
            {
                _progressState = value;

                if (value == NativeMethods.TBPFLAG.TBPF_NOPROGRESS)
                {
                    ProgressValue = 0;
                }

                OnPropertyChanged();
            }
        }

        private int _progressValue;

        public int ProgressValue
        {
            get
            {
                return _progressValue;
            }

            set
            {
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        public ReadOnlyObservableCollection<object> Windows;

        private StoreApp _storeApp;

        public TaskGroup(ReadOnlyObservableCollection<object> windows)
        {
            State = ApplicationWindow.WindowState.Inactive;

            if (windows == null)
            {
                return;
            }

            Windows = windows;
            (Windows as INotifyCollectionChanged).CollectionChanged += TaskGroup_CollectionChanged;

            foreach(var aWindow in Windows)
            {
                if (aWindow is ApplicationWindow appWindow)
                {
                    appWindow.PropertyChanged += ApplicationWindow_PropertyChanged;
                }
            }

            setInitialValues();
            setState();
        }

        private void setInitialValues()
        {
            if (Windows[0] is ApplicationWindow window)
            {
                if (window.IsUWP)
                {
                    _storeApp = StoreAppHelper.AppList.GetAppByAumid(window.AppUserModelID);
                    Title = _storeApp.DisplayName;
                    Icon = _storeApp.GetIconImageSource(IconHelper.ParseSize(Settings.Instance.TaskbarIconSize) == IconSize.Small ? IconSize.Small : IconSize.Large);
                }
                else
                {
                    Title = FileVersionInfo.GetVersionInfo(window.WinFileName).FileDescription;

                    Task.Factory.StartNew(() =>
                    {
                        Icon = IconImageConverter.GetImageFromAssociatedIcon(window.WinFileName, IconHelper.ParseSize(Settings.Instance.TaskbarIconSize) == IconSize.Small ? IconSize.Small : IconSize.Large);
                    }, CancellationToken.None, TaskCreationOptions.None, IconHelper.IconScheduler);
                }
            }
        }

        private void setState()
        {
            bool active = Windows.Any(win =>
            {
                if (win is ApplicationWindow window)
                {
                    return window.State == ApplicationWindow.WindowState.Active;
                }

                return false;
            });

            bool flashing = Windows.Any(win =>
            {
                if (win is ApplicationWindow window)
                {
                    return window.State == ApplicationWindow.WindowState.Flashing;
                }

                return false;
            });

            if (flashing)
            {
                State = ApplicationWindow.WindowState.Flashing;
            }
            else if (active)
            {
                State = ApplicationWindow.WindowState.Active;
            }
            else
            {
                State = ApplicationWindow.WindowState.Inactive;
            }
        }

        private void ApplicationWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "State":
                    setState();
                    break;
                case "OverlayIcon":
                    // TODO
                    break;
                case "OverlayIconDescription":
                    // TODO
                    break;
                case "ProgressState":
                    // TODO
                    break;
                case "ProgressValue":
                    // TODO
                    break;
            }
        }

        private void TaskGroup_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem is ApplicationWindow appWindow)
                    {
                        appWindow.PropertyChanged += ApplicationWindow_PropertyChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem is ApplicationWindow appWindow)
                    {
                        appWindow.PropertyChanged -= ApplicationWindow_PropertyChanged;
                    }
                }
            }

            setState();
        }

        public void Dispose()
        {
            foreach (var aWindow in Windows)
            {
                if (aWindow is ApplicationWindow appWindow)
                {
                    appWindow.PropertyChanged -= ApplicationWindow_PropertyChanged;
                }
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
