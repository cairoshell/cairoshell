namespace CairoDesktop.SupportingClasses
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml.Linq;

    namespace BingPhotoOfDayClient
    {
        /// <summary>
        /// A class designed to download the bing photo of the day
        /// </summary>
        /// <remarks>
        /// https://www.codeproject.com/Tips/1044421/Use-Bing-Photo-of-the-Day-in-Your-Application
        /// </remarks>
        public class BingWallPaperClient
        {
            // TODO: This should be moved to a plugin at some point

            private readonly string _feed;
            private readonly string _tempfilename;
            private readonly string _tempcoppyright;
            private bool _loadcalled;

            public BingWallPaperClient()
            {
                var tempdir = Environment.ExpandEnvironmentVariables("%temp%");
                _tempfilename = Path.Combine(tempdir, "bingphotooftheday.jpg");
                _tempcoppyright = Path.Combine(tempdir, "bingphotooftheday.txt");
                _loadcalled = false;

                //photo of the day data in XML format
                _feed = "http://www.bing.com/HPImageArchive.aspx?format=xml&idx=0&n=1&mkt=en-US";
            }

            /// <summary>
            /// Downloads the photo of the day synchronously
            /// </summary>
            public void DownLoad()
            {
                bool downloadneeded = true;
                if (File.Exists(_tempfilename))
                {
                    FileInfo fi = new FileInfo(_tempfilename);
                    if ((DateTime.UtcNow - fi.LastWriteTimeUtc).TotalHours < 24)
                    {
                        downloadneeded = false;
                    }
                }

                if (File.Exists(_tempcoppyright))
                {
                    CopyRightData = File.ReadAllText(_tempcoppyright);
                    downloadneeded = false;
                }
                else
                {
                    downloadneeded = true;
                }

                _loadcalled = true;

                if (!downloadneeded)
                {
                    return;
                }

                var document = XDocument.Load(_feed).Elements().Elements().FirstOrDefault();

                var url = (from i in document.Elements()
                           where i.Name == "url"
                           select i.Value.ToString()).FirstOrDefault();

                var imgurl = "http://www.bing.com" + url;

                var copyright = (from i in document.Elements()
                                 where i.Name == "copyright"
                                 select i.Value.ToString()).FirstOrDefault();

                var cplink = (from i in document.Elements()
                              where i.Name == "copyrightlink"
                              select i.Value.ToString()).FirstOrDefault();

                CopyRightData = copyright + Environment.NewLine + cplink;
                File.WriteAllText(_tempcoppyright, CopyRightData);

                using (var client = new WebClient())
                {
                    client.DownloadFile(imgurl, _tempfilename);
                }
            }

            /// <summary>
            /// Asynchronous & await-able version of the download routine
            /// </summary>
            /// <returns>An await-able task</returns>
            public Task DownloadAsync()
            {
                return Task.Run(() =>
                {
                    DownLoad();
                });
            }

            /// <summary>
            /// Gets the Photo of the day in a WPF compliant ImageSource
            /// </summary>
            public ImageSource WPFPhotoOfTheDay
            {
                get
                {
                    if (!_loadcalled)
                    {
                        throw new InvalidOperationException("Call the DownLoad() method first");
                    }

                    return new BitmapImage(new Uri(_tempfilename));
                }
            }
            /// <summary>
            /// Gets the Photo of the day in a Windows Forms compliant ImageSource
            /// </summary>
            public Bitmap WFPhotoOfTheDay
            {
                get
                {
                    if (!_loadcalled)
                    {
                        throw new InvalidOperationException("Call the DownLoad() method first");
                    }

                    return new Bitmap(_tempfilename);
                }
            }

            /// <summary>
            /// CopyRight data information
            /// </summary>
            public string CopyRightData
            {
                get;
                private set;
            }
        }
    }
}
