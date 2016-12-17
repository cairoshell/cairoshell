using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;

namespace CairoDesktop
{
    public class SystemFile : INotifyPropertyChanged
    {
        private static readonly string[] ImageFileTypes = new string[] { ".jpg", ".jpeg", ".gif", ".bmp", ".png" };
        private Dispatcher _dispatcher;
        private ImageSource _icon;
        private List<string> _verbs = new List<string>();

        /// <summary>
        /// Initializes a new instance of the SystemFile class using the CurrentDispatcher.
        /// </summary>
        /// <param name="filePath">The file path of the file in question.</param>
        public SystemFile(string filePath)
            : this(filePath, Dispatcher.CurrentDispatcher)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SystemFile class.
        /// </summary>
        /// <param name="filePath">The file path of the file in question.</param>
        /// <param name="dispatcher">The current UI dispatcher.</param>
        public SystemFile(string filePath, Dispatcher dispatcher)
        {
            this.FullName = filePath;
            this.Name = Path.GetFileName(filePath);
            this._dispatcher = dispatcher;
            Initialize();
        }

        /// <summary>
        /// Initializes the details of the file.
        /// </summary>
        private void Initialize()
        {
            Process refProc = new Process();
            refProc.StartInfo.FileName = this.FullName;

            this.Verbs.AddRange(refProc.StartInfo.Verbs);

            refProc.Dispose();

            _dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => GetDisplayIcon()));
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the FullName of the System File.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the Icon associated with this file.
        /// This is a Dependency Property.
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        /// <summary>
        /// Gets the verbs for the StartInfo
        /// </summary>
        public List<string> Verbs
        {
            get
            {
                return _verbs;
            }

            private set
            {

                _verbs = value;
                OnPropertyChanged("Verbs");
            }
        }

        /// <summary>
        /// Checks the path's file extension against the list of known types to determine if the file is an image.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>Indication of if the file is an image or not.</returns>
        public static bool GetFileIsImage(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            string ext = Path.GetExtension(path);
            foreach (string fileType in ImageFileTypes)
            {
                if (ext.Equals(fileType, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves the display icon of the file.
        /// If the file is an image then it will return the image its self (e.g. preview).
        /// </summary>
        private void GetDisplayIcon()
        {
            if (GetFileIsImage(this.FullName))
            {
                var img = new BitmapImage();
                Stream imgStream = null;
                try
                {
                    imgStream = File.Open(this.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = imgStream;
                    img.EndInit();
                    img.Freeze();

                    this.Icon = img;
                }
                catch
                {
                    this.Icon = AppGrabber.WpfWin32ImageConverter.GetImageFromAssociatedIcon(this.FullName);
                }
                finally
                {
                    if (imgStream != null)
                    {
                        imgStream.Close();
                    }
                }
            } 
            else 
            {
                // This will attempts to get the icon via AppGrabber - if it fails the default icon will be returned.
                this.Icon = AppGrabber.WpfWin32ImageConverter.GetImageFromAssociatedIcon(this.FullName);
            }
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
