using CairoDesktop.Common;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            this.Name = "";
            this.Path = "";
        }

        /// <summary>
        /// This object holds the basic information necessary for identifying an application.
        /// </summary>
        /// <param name="name">The friendly name of this application.</param>
        /// <param name="path">Path to the shortcut.</param>
        /// <param name="target">Path to the executable.</param>
        /// <param name="icon">ImageSource used to denote the application's icon in a graphical environment.</param>
        public ApplicationInfo(string name, string path, string target, ImageSource icon, string iconColor, string iconPath)
        {
            this.Name = name;
            this.Path = path;
            this.Target = target;
            this.Icon = icon;
            this.IconColor = iconColor;
            this.IconPath = iconPath;
        }

        private bool _iconLoading = false;

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
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                }
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
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Path"));
                }
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
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Target"));
                }
            }
        }

        private string iconColor;
        /// <summary>
        /// Path to the executable.
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
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IconColor"));
                }
            }
        }

        private string iconPath;
        /// <summary>
        /// Path to the executable.
        /// </summary>
        public string IconPath
        {
            get { return iconPath; }
            set
            {
                iconPath = value;
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IconPath"));
                }
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
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AlwaysAdmin"));
                }
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
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AskAlwaysAdmin"));
                }
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

                    var thread = new Thread(() =>
                    {
                        Icon = GetAssociatedIcon();
                        Icon.Freeze();
                        _iconLoading = false;
                    });
                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }

                return icon;
            }
            set
            {
                icon = value;
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Icon"));
                }
            }
        }

        public bool IsStoreApp
        {
            get
            {
                return this.path.StartsWith("appx:");
            }
        }

        private Category category;
        /// <summary>
        /// The Category object to which this ApplicationInfo object belongs.
        /// Note: DO NOT ASSIGN MANUALLY. This property should only be set by a Category oject when adding/removing from its internal list.
        /// </summary>
        public Category Category
        {
            get { return category; }
            set
            {
                category = value;
                // Notify Databindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Category"));
                }
            }
        }

        /// <summary>
        /// Determines if this ApplicationInfo object refers to the same application as another ApplicationInfo object.
        /// </summary>
        /// <param name="other">ApplicationInfo object to compare to.</param>
        /// <returns>True if the Name and Path values are equal, False if not.</returns>
        public bool Equals(ApplicationInfo other)
        {
            //if (this.Name != other.Name) return false; -- because apps can be renamed, this is no longer valid
            if (this.Path == other.Path)
            {
                return true;
            }
            if (System.IO.Path.GetExtension(this.Path).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                if ((this.Target == other.Target) && (this.Name == other.Name))
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
            return this.Equals((ApplicationInfo)obj);
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

        /// <summary>
        /// Is this object greater than, less than, or equal to another ApplicationInfo? (For sorting purposes only)
        /// </summary>
        /// <param name="other">Object to compare to.</param>
        /// <returns>0 if same, negative if less, positive if more.</returns>
        public int CompareTo(ApplicationInfo other)
        {
            return this.Name.CompareTo(other.Name);
        }

        public override string ToString()
        {
            return string.Format("Name={0} Path={1}", this.Name, this.Path);
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

        /// <summary>
        /// This Event is raised whenever a property of this object has changed. Necesary to sync state when binding.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets an ImageSource object representing the associated icon of a file.
        /// </summary>
        public ImageSource GetAssociatedIcon()
        {
            int size = 1;
            if (this.Category != null && this.Category.Type == AppCategoryType.QuickLaunch && Configuration.Settings.TaskbarIconSize != 1)
                size = 0;

            if (this.IsStoreApp)
            {
                if (string.IsNullOrEmpty(this.IconPath) || !Interop.Shell.Exists(this.IconPath))
                {
                    try
                    {
                        string[] icon = UWPInterop.StoreAppHelper.GetAppIcon(this.Target, size);
                        this.IconPath = icon[0];
                        this.IconColor = icon[1];
                    }
                    catch
                    {
                        return IconImageConverter.GetDefaultIcon();
                    }
                }

                try
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(this.IconPath, UriKind.Absolute);
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
                catch
                {
                    return IconImageConverter.GetDefaultIcon();
                }
            }
            else
                return IconImageConverter.GetImageFromAssociatedIcon(this.Path, size);
        }

        /// <summary>
        /// Create a copy of this object.
        /// </summary>
        /// <returns>A new ApplicationInfo object with the same data as this object, not bound to a Category.</returns>
        internal ApplicationInfo Clone()
        {
            ApplicationInfo rval = new ApplicationInfo(this.Name, this.Path, this.Target, this.Icon, this.IconColor, this.IconPath);
            return rval;
        }
    }

}
