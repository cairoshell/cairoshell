using CairoDesktop.Common;
using ManagedShell.Common.Enums;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using ManagedShell.UWPInterop;
using ManagedShell.WindowsTasks;
using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CairoDesktop.Taskbar.SupportingClasses
{
    public class TaskGroup : INotifyPropertyChanged, IDisposable
    {
        private bool _singleWindowMode;

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

        public ApplicationWindow.WindowState State
        {
            get
            {
                return getState();
            }
        }

        public ImageSource OverlayIcon
        {
            get
            {
                return getOverlayIcon();
            }
        }

        public string OverlayIconDescription
        {
            get
            {
                return getOverlayIconDescription();
            }
        }

        public NativeMethods.TBPFLAG ProgressState
        {
            get
            {
                return getProgressState();
            }
        }

        public int ProgressValue
        {
            get
            {
                return getProgressValue();
            }
        }

        public ReadOnlyObservableCollection<object> Windows;

        private StoreApp _storeApp;

        public TaskGroup(ReadOnlyObservableCollection<object> windows)
        {
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

            setDisplayValues();
        }

        private void setDisplayValues()
        {
            if (Windows.Count > 1)
            {
                _singleWindowMode = false;
                setDisplayValuesApp();
            }
            else
            {
                _singleWindowMode = true;
                setDisplayValuesWindow();
            }
        }

        private void setDisplayValuesApp()
        {
            if (Windows.Count > 0 && Windows[0] is ApplicationWindow window)
            {
                if (window.IsUWP)
                {
                    _storeApp = StoreAppHelper.AppList.GetAppByAumid(window.AppUserModelID);
                    Title = _storeApp.DisplayName;
                    Icon = window.Icon;
                }
                else
                {
                    Title = window.WinFileDescription;

                    Task.Factory.StartNew(() =>
                    {
                        Icon = IconImageConverter.GetImageFromAssociatedIcon(window.WinFileName, Settings.Instance.TaskbarIconSize == IconSize.Small ? IconSize.Small : IconSize.Large);
                    }, CancellationToken.None, TaskCreationOptions.None, IconHelper.IconScheduler);
                }
            }
        }

        private void setDisplayValuesWindow()
        {
            if (Windows.Count > 0 && Windows[0] is ApplicationWindow window)
            {
                Title = window.Title;
                Icon = window.Icon;
            }
        }

        private ApplicationWindow.WindowState getState()
        {
            if (Windows.Any(win =>
            {
                if (win is ApplicationWindow window)
                {
                    return window.State == ApplicationWindow.WindowState.Flashing;
                }

                return false;
            }))
            {
                return ApplicationWindow.WindowState.Flashing;
            }

            if (Windows.Any(win =>
            {
                if (win is ApplicationWindow window)
                {
                    return window.State == ApplicationWindow.WindowState.Active;
                }

                return false;
            }))
            {
                return ApplicationWindow.WindowState.Active;
            }
            
            return ApplicationWindow.WindowState.Inactive;
        }

        private ImageSource getOverlayIcon()
        {
            return getOverlayIconWindow()?.OverlayIcon;
        }

        private string getOverlayIconDescription()
        {
            return getOverlayIconWindow()?.OverlayIconDescription;
        }

        private ApplicationWindow getOverlayIconWindow()
        {
            ApplicationWindow windowWithOverlay = (ApplicationWindow)Windows.FirstOrDefault(win =>
            {
                if (win is ApplicationWindow window)
                {
                    return window.OverlayIcon != null;
                }

                return false;
            });

            if (windowWithOverlay != null && windowWithOverlay.OverlayIcon != null)
            {
                return windowWithOverlay;
            }

            return null;
        }

        private NativeMethods.TBPFLAG getProgressState()
        {
            if (Windows.Any(win =>
            {
                if (win is ApplicationWindow window)
                {
                    return window.ProgressState == NativeMethods.TBPFLAG.TBPF_INDETERMINATE;
                }

                return false;
            }))
            {
                return NativeMethods.TBPFLAG.TBPF_INDETERMINATE;
            }

            if (Windows.Any(win =>
            {
                if (win is ApplicationWindow window)
                {
                    return window.ProgressState == NativeMethods.TBPFLAG.TBPF_NORMAL;
                }

                return false;
            }))
            {
                return NativeMethods.TBPFLAG.TBPF_NORMAL;
            }

            return NativeMethods.TBPFLAG.TBPF_NOPROGRESS;
        }

        private int getProgressValue()
        {
            int count = Windows.Count(win =>
            {
                if (win is ApplicationWindow window)
                {
                    return window.ProgressValue > 0;
                }

                return false;
            });

            if (count < 1)
            {
                return 0;
            }

            int total = Windows.Sum(win => {
                if (win is ApplicationWindow window)
                {
                    return window.ProgressValue > 0 ? window.ProgressValue : 0;
                }
                return 0;
            });

            if (total < 1)
            {
                return 0;
            }

            return total / count;
        }

        private void ApplicationWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "State":
                    OnPropertyChanged("State");
                    break;
                case "OverlayIcon":
                    OnPropertyChanged("OverlayIcon");
                    break;
                case "OverlayIconDescription":
                    OnPropertyChanged("OverlayIconDescription");
                    break;
                case "ProgressState":
                    OnPropertyChanged("ProgressState");
                    break;
                case "ProgressValue":
                    OnPropertyChanged("ProgressValue");
                    break;
                case "Title" when _singleWindowMode:
                    if (Windows.Count > 0 && Windows[0] is ApplicationWindow titleWindow)
                    {
                        Title = titleWindow.Title;
                    }
                    break;
                case "Icon":
                    if (Windows.Count > 0 && Windows[0] is ApplicationWindow iconWindow && (_singleWindowMode || iconWindow.IsUWP))
                    {
                        Icon = iconWindow.Icon;
                    }
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

                if (e.OldItems.Count > 0)
                {
                    // If windows were removed, update progress values in case the window was destroyed before updating its progress value
                    OnPropertyChanged("ProgressState");
                    OnPropertyChanged("ProgressValue");
                }
            }

            // If the window count has changed such that single window mode should toggle
            if (_singleWindowMode == (Windows.Count > 1))
            {
                setDisplayValues();
            }

            OnPropertyChanged("State");
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
