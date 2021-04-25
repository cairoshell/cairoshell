using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ManagedShell.Common.Enums;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.AppGrabber
{
    [Serializable()]
    public class ApplicationInfo : IEquatable<ApplicationInfo>, IComparable<ApplicationInfo>, INotifyPropertyChanged
    {
        /// <summary>
        /// This object holds the basic information necessary for identifying an application.
        /// </summary>
		public ApplicationInfo()
        {
            Name = "";
            Path = "";
        }

        /// <summary>
        /// This object holds the basic information necessary for identifying an application.
        /// </summary>
        /// <param name="name">The friendly name of this application.</param>
        /// <param name="path">Path to the shortcut.</param>
        /// <param name="target">Path to the executable.</param>
        /// <param name="icon">ImageSource used to denote the application's icon in a graphical environment.</param>
        public ApplicationInfo(string name, string path, string target, ImageSource icon, string iconColor)
        {
            Name = name;
            Path = path;
            Target = target;
            Icon = icon;
            IconColor = iconColor;
        }

        private bool _iconLoading;

        private string name;
        /// <summary>
        /// The friendly name of this application.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string path;
        /// <summary>
        /// Path to the shortcut.
        /// </summary>
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                OnPropertyChanged();
            }
        }

        public string PathDirectory
        {
            get
            {
                if (IsStoreApp)
                    return Localization.DisplayString.sProgramsMenu;
                else
                {
                    try
                    {
                        return System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(Path));
                    }
                    catch
                    {
                        return "";
                    }
                }
            }
        }

        private string target;
        /// <summary>
        /// Path to the executable.
        /// </summary>
        public string Target
        {
            get { return target; }
            set
            {
                target = value;
                OnPropertyChanged();
            }
        }

        private string iconColor;
        /// <summary>
        /// Icon plate background color
        /// </summary>
        public string IconColor
        {
            get
            {
                if (!string.IsNullOrEmpty(iconColor))
                    return iconColor;
                else
                    return "Transparent";
            }
            set
            {
                iconColor = value;
                OnPropertyChanged();
            }
        }

        private bool alwaysAdmin;
        /// <summary>
        /// If the user has chosen to run the app as admin always.
        /// </summary>
        public bool AlwaysAdmin
        {
            get { return alwaysAdmin; }
            set
            {
                alwaysAdmin = value;
                OnPropertyChanged();
            }
        }

        private bool askAlwaysAdmin;
        /// <summary>
        /// Whether the UI should ask if the app should be always run as admin (set true after running as admin once)
        /// </summary>
        public bool AskAlwaysAdmin
        {
            get { return askAlwaysAdmin; }
            set
            {
                askAlwaysAdmin = value;
                OnPropertyChanged();
            }
        }

        private ImageSource icon;
        /// <summary>
        /// ImageSource used to denote the application's icon in a graphical environment.
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                if (icon == null && !_iconLoading)
                {
                    _iconLoading = true;

                    Task.Factory.StartNew(() =>
                    {
                        Icon = GetAssociatedIcon();
                        Icon.Freeze();
                        _iconLoading = false;
                    }, CancellationToken.None, TaskCreationOptions.None, IconHelper.IconScheduler);
                }

                return icon;
            }
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        public bool IsStoreApp
        {
            get
            {
                return path.StartsWith("appx:");
            }
        }

        private Category category;
        /// <summary>
        /// The Category object to which this ApplicationInfo object belongs.
        /// Note: DO NOT ASSIGN MANUALLY. This property should only be set by a Category object when adding/removing from its internal list.
        /// </summary>
        public Category Category
        {
            get { return category; }
            set
            {
                category = value;
                OnPropertyChanged();
            }
        }

        #region IEquatable
        /// <summary>
        /// Determines if this ApplicationInfo object refers to the same application as another ApplicationInfo object.
        /// </summary>
        /// <param name="other">ApplicationInfo object to compare to.</param>
        /// <returns>True if the Name and Path values are equal, False if not.</returns>
        public bool Equals(ApplicationInfo other)
        {
            //if (this.Name != other.Name) return false; -- because apps can be renamed, this is no longer valid
            if (Path == other.Path)
            {
                return true;
            }
            if (System.IO.Path.GetExtension(Path).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                if (Target == other.Target && Name == other.Name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if this ApplicationInfo object refers to the same application as another ApplicationInfo object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>True if the Name and Path values are equal, False if not.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ApplicationInfo))
                return false;
            return Equals((ApplicationInfo)obj);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            if (Name != null)
                hashCode ^= Name.GetHashCode();
            if (Path != null)
                hashCode ^= Path.GetHashCode();
            return hashCode;
        }
        #endregion

        #region IComparable
        /// <summary>
        /// Is this object greater than, less than, or equal to another ApplicationInfo? (For sorting purposes only)
        /// </summary>
        /// <param name="other">Object to compare to.</param>
        /// <returns>0 if same, negative if less, positive if more.</returns>
        public int CompareTo(ApplicationInfo other)
        {
            return Name.CompareTo(other.Name);
        }
        #endregion

        public override string ToString()
        {
            return $"Name={Name} Path={Path}";
        }

        public static bool operator ==(ApplicationInfo x, ApplicationInfo y)
        {
            // cast to object to prevent stack overflow
            if ((object)x == null && (object)y == null)
                return true;
            else if ((object)x == null || (object)y == null)
               return false;

            return x.Equals(y);
        }

        public static bool operator !=(ApplicationInfo x, ApplicationInfo y)
        {
            return !(x == y);
        }

        private ImageSource GetAssociatedIcon()
        {
            IconSize size = IconSize.Small;
            if (Category != null && Category.Type == AppCategoryType.QuickLaunch && (IconSize)Configuration.Settings.Instance.TaskbarIconSize != IconSize.Small)
                size = IconSize.Large;

            return GetIconImageSource(size);
        }

        /// <summary>
        /// Gets an ImageSource object representing the associated icon of a file.
        /// </summary>
        public ImageSource GetIconImageSource(IconSize size)
        {
            if (IsStoreApp)
            {
                var storeApp = ManagedShell.UWPInterop.StoreAppHelper.AppList.GetAppByAumid(Target);

                if (storeApp == null)
                {
                    return IconImageConverter.GetDefaultIcon();
                }

                return storeApp.GetIconImageSource(size);
            }
            
            return IconImageConverter.GetImageFromAssociatedIcon(Path, size);
        }

        public static ApplicationInfo FromStoreApp(ManagedShell.UWPInterop.StoreApp storeApp)
        {
            ApplicationInfo ai = new ApplicationInfo();
            ai.Name = storeApp.DisplayName;
            ai.Path = "appx:" + storeApp.AppUserModelId;
            ai.Target = storeApp.AppUserModelId;
            ai.IconColor = storeApp.IconColor;

            return ai;
        }

        /// <summary>
        /// Create a copy of this object.
        /// </summary>
        /// <returns>A new ApplicationInfo object with the same data as this object, not bound to a Category.</returns>
        internal ApplicationInfo Clone()
        {
            ApplicationInfo rval = new ApplicationInfo(Name, Path, Target, Icon, IconColor);
            return rval;
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
