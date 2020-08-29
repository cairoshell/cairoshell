using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CairoDesktop.SupportingClasses
{
    /// <summary>
    /// https://picsum.photos/
    /// </summary>
    public class PicSumWallpaperClient
    {
        private UriBuilder _uriBuilder;
        private BitmapImage _bitmapImage;

        public PicSumWallpaperClient(int width, int height, bool grayscale = false, int? blur = null)
        {
            _uriBuilder = new UriBuilder($"https://picsum.photos/{width}/{height}");
            List<string> queryParts = new List<string>();
            if (grayscale)
            {
                queryParts.Add("grayscale");
            }

            if (blur.HasValue)
            {
                string blurPart = "blur";
                if (blur.Value > 0 && blur.Value <= 10)
                {
                    blurPart += $"={blur.Value}";
                }

                queryParts.Add(blurPart);
            }

            if (queryParts.Count > 0)
            {
                _uriBuilder.Query = string.Join("&", queryParts.ToArray());
            }

            var url = _uriBuilder.Uri.AbsoluteUri;
            var buffer = new WebClient().DownloadData(url);
            _bitmapImage = new BitmapImage();

            using (var stream = new MemoryStream(buffer))
            {
                _bitmapImage.BeginInit();
                _bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                _bitmapImage.StreamSource = stream;
                _bitmapImage.EndInit();
            }
        }

        /// <summary>
        /// Gets the image as an ImageSource
        /// </summary>
        public ImageSource Wallpaper
        {
            get
            {
                return _bitmapImage;
            }
        }
    }
}