using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Threading.Tasks;

namespace CairoDesktop.Common
{
    public class SystemFile : INotifyPropertyChanged
    {
        private const int MAX_IMAGE_RELOAD_ATTEMPTS = 2;

        private static readonly string[] ImageFileTypes = { ".jpg", ".jpeg", ".jfif", ".gif", ".bmp", ".png" };
        private ImageSource _icon;
        private ImageSource _largeIcon;
        private string _friendlyName;
        private string _fullName;
        private string _name;
        private List<string> _verbs;
        private bool _iconLoading;
        private bool _iconLargeLoading;
        private int _imageReloadAttempts;

        public bool InteractiveRenameRequested { get; private set; }
        public EventHandler InteractiveRenameEvent;

        /// <summary>
        /// Gets whether or not the file is a directory.
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                try
                {
                    return Interop.Shell.Exists(FullName) && (File.GetAttributes(FullName) & FileAttributes.Directory) == FileAttributes.Directory;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the parent SystemDirectory for the file.
        /// </summary>
        public SystemDirectory ParentDirectory { get; private set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = Path.GetFileName(this.FullName);
                }

                return _name;
            }

            private set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets the name of the file, without extension depending on user preference.
        /// </summary>
        public string FriendlyName
        {
            get
            {
                if (_friendlyName == null)
                {
                    this.FriendlyName = Interop.Shell.GetDisplayName(this.FullName);
                }

                return _friendlyName;
            }

            private set
            {
                _friendlyName = value;
                OnPropertyChanged("FriendlyName");
            }
        }

        /// <summary>
        /// Gets or sets the FullName of the System File.
        /// </summary>
        public string FullName
        {
            get
            {
                return _fullName;
            }

            set
            {
                _fullName = value;
                OnPropertyChanged("FullName");


                // reset affected properties
                _name = null;
                OnPropertyChanged("Name");

                _friendlyName = null;
                OnPropertyChanged("FriendlyName");

                _icon = null;
                OnPropertyChanged("Icon");

                _largeIcon = null;
                OnPropertyChanged("LargeIcon");
            }
        }

        /// <summary>
        /// Gets or sets the Icon associated with this file.
        /// This is a Dependency Property.
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                if (_icon == null && !_iconLoading)
                {
                    _iconLoading = true;

                    Task.Factory.StartNew(() =>
                    {
                        Icon = GetDisplayIcon(IconSize.Large);
                        Icon.Freeze();
                        _iconLoading = false;
                    }, CancellationToken.None, TaskCreationOptions.None, Interop.Shell.IconScheduler);
                }

                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        /// <summary>
        /// Gets or sets the Icon associated with this file.
        /// This is a Dependency Property.
        /// </summary>
        public ImageSource LargeIcon
        {
            get
            {
                if (_largeIcon == null && !_iconLargeLoading)
                {
                    _iconLargeLoading = true;

                    Task.Factory.StartNew(() =>
                    {
                        LargeIcon = GetDisplayIcon(IconSize.ExtraLarge);
                        LargeIcon.Freeze();
                        _iconLargeLoading = false;
                    }, CancellationToken.None, TaskCreationOptions.None, Interop.Shell.IconScheduler);
                }

                return _largeIcon;
            }
            set
            {
                _largeIcon = value;
                OnPropertyChanged("LargeIcon");
            }
        }

        /// <summary>
        /// Gets the verbs for the StartInfo
        /// </summary>
        public List<string> Verbs
        {
            get
            {
                if (_verbs == null)
                    getVerbs();

                return _verbs;
            }

            private set
            {

                _verbs = value;
                OnPropertyChanged("Verbs");
            }
        }

        /// <summary>
        /// Initializes a new instance of the SystemFile class.
        /// </summary>
        /// <param name="filePath">The file path of the file in question.</param>
        /// <param name="parentDirectory">The SystemDirectory that contains this SystemFile.</param>
        public SystemFile(string filePath, SystemDirectory parentDirectory)
        {
            SetFilePath(filePath, parentDirectory);
        }

        public bool SetFilePath(string filePath, SystemDirectory parentDirectory)
        {
            if (Interop.Shell.Exists(filePath))
            {
                FullName = filePath;
                InteractiveRenameRequested = false;

                ParentDirectory = parentDirectory;
                OnPropertyChanged("ParentDirectory");

                return true;
            }
            return false;
        }

        /// <summary>
        /// Initializes the details of the file.
        /// </summary>
        private void getVerbs()
        {
            _verbs = new List<string>();

            if (!IsDirectory && !string.IsNullOrEmpty(FullName))
            {
                Process refProc = new Process();
                refProc.StartInfo.FileName = this.FullName;

                try
                {
                    this.Verbs.AddRange(refProc.StartInfo.Verbs);
                }
                catch { }

                refProc.Dispose();
            }
        }

        public static long GetFileSize(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return file.Length;
            }
        }

        /// <summary>
        /// Checks the path's file extension against the list of known types to determine if the file is an image.
        /// </summary>
        /// <returns>Indication of if the file is an image or not.</returns>
        public bool IsImage
        {
            get
            {
                try
                {
                    if (!File.Exists(FullName))
                    {
                        return false;
                    }

                    string ext = Path.GetExtension(FullName);
                    foreach (string fileType in ImageFileTypes)
                    {
                        if (ext.Equals(fileType, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                catch { }

                return false;
            }
        }

        public bool Rename(string newFilename)
        {
            try
            {
                InteractiveRenameRequested = false;
                // get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(FullName);
                string newFilePathName = Path.GetDirectoryName(FullName) + "\\" + newFilename;

                if (newFilePathName != FullName)
                {
                    //detect whether its a directory or file
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        Directory.Move(FullName, newFilePathName);
                    else
                        File.Move(FullName, newFilePathName);
                }
                return true;
            }
            catch (Exception ex)
            {
                CairoMessage.Show("The file was unable to be renamed because: " + ex.Message, "Unable to rename", MessageBoxButton.OK, CairoMessageImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Retrieves the display icon of the file.
        /// If the file is an image then it will return the image its self (e.g. preview).
        /// </summary>
        private ImageSource GetDisplayIcon(IconSize size)
        {
            if (Interop.Shell.Exists(FullName))
            {
                if (IsImage)
                {
                    try
                    {
                        if (GetFileSize(FullName) > 0)
                        {
                            BitmapImage img = new BitmapImage();
                            img.BeginInit();
                            img.CacheOption = BitmapCacheOption.OnLoad;
                            img.UriSource = new Uri(FullName);
                            int dSize = 32;

                            if (size == IconSize.ExtraLarge)
                                dSize = 48;

                            Interop.Shell.TransformToPixels(dSize, dSize, out dSize, out dSize);
                            img.DecodePixelWidth = dSize;
                            img.EndInit();
                            img.Freeze();

                            return img;
                        }
                        else
                        {
                            return handleThumbnailError(size);
                        }
                    }
                    catch
                    {
                        return handleThumbnailError(size);
                    }
                }
                else
                {
                    // This will attempt to get the icon - if it fails the default icon will be returned.
                    return IconImageConverter.GetImageFromAssociatedIcon(FullName, size);
                }
            }
            else
            {
                return IconImageConverter.GetDefaultIcon();
            }
        }

        private ImageSource handleThumbnailError(IconSize size)
        {
            if (_imageReloadAttempts < MAX_IMAGE_RELOAD_ATTEMPTS)
            {
                // retry soon because this file might still be writing to disk
                DispatcherTimer iconcheck = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
                iconcheck.Interval = new TimeSpan(0, 0, 1);
                iconcheck.Tick += iconcheck_Tick;
                iconcheck.Start();

                // show the placeholder icon so we don't show multiple icon changes. we'll show the default file icon if we run out of attempts.
                return IconImageConverter.GetDefaultIcon();
            }
            else
            {
                // get the default icon for the file
                return IconImageConverter.GetImageFromAssociatedIcon(FullName, size);
            }
        }

        private void iconcheck_Tick(object sender, EventArgs e)
        {
            // set icons to null to trigger re-fetch of icon
            _imageReloadAttempts++;
            Icon = null;
            LargeIcon = null;
            (sender as DispatcherTimer).Stop();
        }

        public void RequestInteractiveRename(object sender)
        {
            InteractiveRenameRequested = true;
            InteractiveRenameEvent?.Invoke(sender, null);
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// This Event is raised whenever a property of this object has changed. Necesary to sync state when binding.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        [DebuggerNonUserCode]
        private void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion
    }

}
