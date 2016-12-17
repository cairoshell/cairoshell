using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

namespace CairoDesktop.AppGrabber
{
    /// <summary>
    /// Provides static conversion methods to change Win32 Icons into ImageSources.
    /// </summary>
    public class WpfWin32ImageConverter
    {
        /// <summary>
        /// Retrieves the Icon for the file name as an ImageSource
        /// </summary>
        /// <param name="filename">The filename of the file to query the Icon for.</param>
        /// <returns>The icon as an ImageSource, otherwise a default image.</returns>
        public static ImageSource GetImageFromAssociatedIcon(string filename, bool isSmall = false) 
        {
            BitmapSource bs = null;
            
            IntPtr hIcon = Interop.Shell.GetIconByFilename(filename, isSmall);
                
            if (hIcon == IntPtr.Zero || hIcon == null)
            {
                return GetDefaultIcon();
            }

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Interop.Shell.Win32.DestroyIcon(hIcon);

            return bs;
        }

        /// <summary>
        /// Retrieves the Icon for the Handle provided as an ImageSource.
        /// </summary>
        /// <param name="hIcon">The icon's handle (HICON).</param>
        /// <returns>The Icon, or a default icon if not found.</returns>
        public static ImageSource GetImageFromHIcon(IntPtr hIcon) 
        {
            BitmapSource bs = null;
            if (hIcon != IntPtr.Zero)
            {
                try
                {
                    bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Interop.Shell.Win32.DestroyIcon(hIcon);
                }
                catch
                {
                    bs = GetDefaultIcon();
                }
            }
            else
            {
                bs = GetDefaultIcon();
            }
            bs.Freeze();

            return bs;
        }

        /// <summary>
        /// Creates an empty bitmap source in the size of an Icon.
        /// </summary>
        /// <returns>Empty icon bitmap.</returns>
        private static BitmapSource GenerateEmptyBitmapSource() {
            int width = 16;
            int height = width;
            int stride = width / 4;
            byte[] pixels = new byte[height * stride];

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Indexed1,
                BitmapPalettes.WebPalette, pixels, stride);
        }

        /// <summary>
        /// Gets the default icon from the resources.
        /// If this fails (e.g. the resource is missing or corrupt) the empty icon is returned.
        /// </summary>
        /// <returns>The default icon as a BitmapSource.</returns>
        public static BitmapSource GetDefaultIcon()
        {
            BitmapImage img = new BitmapImage();
            try
            {
                img.BeginInit();
                img.UriSource = new Uri("pack://application:,,/Resources/nullIcon.png", UriKind.RelativeOrAbsolute);
                img.EndInit();
            }
            catch
            {
                return GenerateEmptyBitmapSource();
            }

            return img;
        }
    }
}
