using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Desktop.xaml
    /// </summary>
    public partial class Desktop : Window, INotifyPropertyChanged
    {
        #region Properties
        private WindowInteropHelper helper;
        private bool altF4Pressed;

        public bool IsFbdOpen = false;
        public bool IsLowering;

        private Brush BackgroundBrush { get; set; }

        public DesktopIcons Icons;
        public DependencyProperty IsOverlayOpenProperty = DependencyProperty.Register("IsOverlayOpen", typeof(bool), typeof(Desktop), new PropertyMetadata(new bool()));
        private DynamicDesktopNavigationManager navigationManager;

        public IReadOnlyList<string> PathHistory
        {
            get
            {
                return navigationManager.ReadOnlyHistory;
            }
        }

        public string CurrentLocation
        {
            get
            {
                return Icons.Location.FullName;
            }
            set
            {
                if (Icons != null)
                {
                    if (Icons.Location != null)
                    {
                        Icons.Location.Dispose();
                    }

                    Icons.Location = new SystemDirectory(value, Dispatcher.CurrentDispatcher);
                    OnPropertyChanged("CurrentDirectoryFriendly");
                }
            }
        }

        public string CurrentDirectoryFriendly
        {
            get
            {
                return Localization.DisplayString.sDesktop_CurrentFolder + " " + Icons.Location.FullName;
            }
        }

        public bool IsOverlayOpen
        {
            get
            {
                return (bool)GetValue(IsOverlayOpenProperty);
            }
            set
            {
                SetValue(IsOverlayOpenProperty, value);

                if (value)
                {
                    ShowOverlay();
                }
                else
                {
                    CloseOverlay();
                }
            }
        }
        #endregion

        public Desktop()
        {
            InitializeComponent();

            setSize();
            setGridPosition();
            setBackground();

            navigationManager = new DynamicDesktopNavigationManager();
            navigationManager.Navigating += NavigationManager_Navigating;

            Settings.Instance.PropertyChanged += Instance_PropertyChanged;

            FullScreenHelper.Instance.FullScreenApps.CollectionChanged += FullScreenApps_CollectionChanged;
        }

        private void SetupPostInit()
        {
            Shell.HideWindowFromTasks(helper.Handle);

            int result = NativeMethods.SetShellWindow(helper.Handle);
            SendToBottom();

            if (Settings.Instance.EnableDesktopOverlayHotKey)
            {
                HotKeyManager.RegisterHotKey(Settings.Instance.DesktopOverlayHotKey, OnShowDesktop);
            }
        }

        private void OnShowDesktop(HotKey hotKey)
        {
            ToggleOverlay();
        }

        private void TryAndEat(Action action)
        {
            try
            { action.Invoke(); }
            catch { }
        }

        #region Window events
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_WINDOWPOSCHANGING)
            {
                if (!IsOverlayOpen)
                {
                    // if the overlay isn't open, we always want to be on the bottom. modify the WINDOWPOS structure so that nothing can change our z-order.

                    // Extract the WINDOWPOS structure corresponding to this message
                    NativeMethods.WINDOWPOS wndPos = NativeMethods.WINDOWPOS.FromMessage(lParam);

                    // Determine if the z-order is changing (absence of SWP_NOZORDER flag)
                    // If we are intentionally setting our z-order, allow it
                    if (!IsLowering && (wndPos.flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER) == 0)
                    {
                        // add the SWP_NOZORDER flag
                        wndPos.flags = wndPos.flags | NativeMethods.SetWindowPosFlags.SWP_NOZORDER;
                        wndPos.UpdateMessage(lParam);
                    }
                }
            }
            else if (msg == NativeMethods.WM_DISPLAYCHANGE && (Startup.IsCairoRunningAsShell))
            {
                SetPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                ReloadBackground();
                handled = true;
            }
            else if (msg == (int)NativeMethods.WM.SETTINGCHANGE &&
                    wParam.ToInt32() == (int)NativeMethods.SPI.SETDESKWALLPAPER)
            {
                ReloadBackground();

                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }

            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Startup.IsShuttingDown) // show the windows desktop
            {
                Shell.ToggleDesktopIcons(true);
            }
            else if (altF4Pressed) // Show the Shutdown Confirmation Window
            {
                SystemPower.ShowShutdownConfirmation();
                e.Cancel = true;
            }
            else // Eat it !!!
            {
                e.Cancel = true;
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4)
            {
                altF4Pressed = true;
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Top = 0;
            helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(new HwndSourceHook(WndProc));

            if (Settings.Instance.EnableDesktop && Icons == null)
            {
                grid.Children.Add(Icons = new DesktopIcons());

                string defaultDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string userDesktopPath = Settings.Instance.DesktopDirectory;

                // first run won't have desktop directory set
                if (string.IsNullOrWhiteSpace(userDesktopPath))
                {
                    Settings.Instance.DesktopDirectory = defaultDesktopPath;
                    userDesktopPath = defaultDesktopPath;
                }

                if (Directory.Exists(userDesktopPath))
                    Navigate(userDesktopPath);
                else if (Directory.Exists(defaultDesktopPath))
                    Navigate(defaultDesktopPath);

                if (Settings.Instance.EnableDynamicDesktop)
                {
                    TryAndEat(() =>
                    {
                        DesktopNavigationToolbar nav = new DesktopNavigationToolbar() { Owner = this };
                        nav.Show();
                    });
                }
            }

            SetupPostInit();
        }

        private void grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Topmost)
            {
                NativeMethods.SetForegroundWindow(helper.Handle);
            }
        }

        private void grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(ScrollViewer))
            {
                IsOverlayOpen = false;
            }
        }

        private void CairoDesktopWindow_LocationChanged(object sender, EventArgs e)
        {
            ResetPosition();
        }

        private void CairoDesktopWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // handle icon and desktop context menus
            if (e.OriginalSource.GetType() == typeof(System.Windows.Controls.ScrollViewer))
            {
                ShellContextMenu cm = new ShellContextMenu(Icons.Location, executeFolderAction);

                e.Handled = true;
            }
            else
            {
                ShellContextMenu.OpenContextMenuFromIcon(e, executeFileAction);
            }
        }

        private void CairoDesktopWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    break;
                case MouseButton.Right:
                    // Handled by CairoDesktopWindow_MouseRightButtonUp
                    break;
                case MouseButton.Middle:
                    break;
                case MouseButton.XButton1:
                    NavigateBackward();
                    break;
                case MouseButton.XButton2:
                    NavigateForward();
                    break;
            }
        }
        #endregion

        #region Change notifications
        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "DesktopBackgroundType":
                    case "BingWallpaperStyle":
                    case "CairoBackgroundImagePath":
                    case "CairoBackgroundImageStyle":
                    case "CairoBackgroundVideoPath":
                        ReloadBackground();
                        break;
                }
            }
        }

        private void FullScreenApps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Settings.Instance.DesktopBackgroundType == "cairoVideoWallpaper")
            {
                // pause video if we see a full screen app to preserve system performance.

                if (BackgroundBrush is VisualBrush brush)
                {
                    if (brush.Visual is MediaElement videoElement)
                    {
                        if (videoElement.LoadedBehavior == MediaState.Manual)
                        {
                            if (FullScreenHelper.Instance.FullScreenApps.Count > 0)
                            {
                                if (videoElement.CanPause)
                                {
                                    videoElement.Pause();
                                }
                            }
                            else
                            {
                                videoElement.Play();
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Size and positioning
        private void SendToBottom()
        {
            IsLowering = true;
            Shell.ShowWindowBottomMost(helper.Handle);
            IsLowering = false;
        }

        private void SetPosition(uint x, uint y)
        {
            Top = 0;
            Left = 0;

            Width = x;
            if (Startup.IsCairoRunningAsShell) Height = y;
            else Height = y - 1;
            setGridPosition();
        }

        public void ResetPosition()
        {
            Top = 0;
            Left = 0;

            setSize();
            setGridPosition();
        }

        private void setSize()
        {
            Width = AppBarHelper.PrimaryMonitorSize.Width;
            if (Startup.IsCairoRunningAsShell) Height = AppBarHelper.PrimaryMonitorSize.Height;
            else Height = AppBarHelper.PrimaryMonitorSize.Height - 1; // TODO making size of screen causes explorer to send ABN_FULLSCREENAPP (but is that a bad thing?)
        }

        private void setGridPosition()
        {
            grid.Width = AppBarHelper.PrimaryMonitorWorkArea.Width / Shell.DpiScale;
            grid.Height = AppBarHelper.PrimaryMonitorWorkArea.Height / Shell.DpiScale;
            grid.Margin = new Thickness(System.Windows.Forms.SystemInformation.WorkingArea.Left / Shell.DpiScale, System.Windows.Forms.SystemInformation.WorkingArea.Top / Shell.DpiScale, 0, 0);
        }
        #endregion

        #region Background
        private void setBackground()
        {
            if (Startup.IsCairoRunningAsShell)
            {
                try
                {
                    if (BackgroundBrush == null)
                        BackgroundBrush = GetCairoBackgroundBrush();

                    Background = BackgroundBrush;
                }
                catch
                {
                    CairoLogger.Instance.Error("Failed setting desktop background.");
                }
            }
            else
            {
                Background = new SolidColorBrush(Color.FromArgb(0x01, 0, 0, 0));
            }
        }

        private Brush GetCairoBackgroundBrush()
        {
            switch (Settings.Instance.DesktopBackgroundType)
            {
                case "cairoImageWallpaper":
                    return GetCairoBackgroundBrush_Image();
                case "cairoVideoWallpaper":
                    return GetCairoBackgroundBrush_Video();
                case "bingWallpaper":
                    return GetCairoBackgroundBrush_BingImageOfTheDay();
                case "colorWallpaper":
                    return GetCairoBackgroundBrush_Color();
                case "windowsDefaultBackground":
                default:
                    return GetCairoBackgroundBrush_Windows();
            }
        }

        private Brush GetCairoBackgroundBrush_Windows()
        {
            string wallpaper = string.Empty;
            CairoWallpaperStyle style = CairoWallpaperStyle.Stretch;

            try
            {
                wallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Wallpaper", "") as string;
                string regWallpaperStyle = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallpaperStyle", "") as string;
                string regTileWallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "TileWallpaper", "") as string;

                // https://docs.microsoft.com/en-us/windows/desktop/Controls/themesfileformat-overview
                switch ($"{regWallpaperStyle}{regTileWallpaper}")
                {
                    case "01": // Tiled { WallpaperStyle = 0; TileWallpaper = 1 }
                        style = CairoWallpaperStyle.Tile;
                        break;
                    case "00": // Centered { WallpaperStyle = 0; TileWallpaper = 0 }
                        style = CairoWallpaperStyle.Center;
                        break;
                    case "60": // Fit { WallpaperStyle = 6; TileWallpaper = 0 }
                        style = CairoWallpaperStyle.Fit;
                        break;
                    case "100": // Fill { WallpaperStyle = 10; TileWallpaper = 0 }
                        style = CairoWallpaperStyle.Fill;
                        break;
                    case "220": // Span { WallpaperStyle = 22; TileWallpaper = 0 }
                        style = CairoWallpaperStyle.Span;
                        break;
                    case "20": // Stretched { WallpaperStyle = 2; TileWallpaper = 0 }
                    default:
                        style = CairoWallpaperStyle.Stretch;
                        break;
                }
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Debug("Problem loading Windows background", ex);
            }

            return GetCairoBackgroundBrush_Image(wallpaper, style) ?? GetCairoBackgroundBrush_Color();
        }

        private Brush GetCairoBackgroundBrush_Image()
        {
            string wallpaper = Settings.Instance.CairoBackgroundImagePath;

            CairoWallpaperStyle wallpaperStyle = CairoWallpaperStyle.Stretch;
            if (Enum.IsDefined(typeof(CairoWallpaperStyle), Settings.Instance.CairoBackgroundImageStyle))
                wallpaperStyle = (CairoWallpaperStyle)Settings.Instance.CairoBackgroundImageStyle;

            return GetCairoBackgroundBrush_Image(wallpaper, wallpaperStyle) ?? GetCairoBackgroundBrush_Windows();
        }

        private Brush GetCairoBackgroundBrush_Color()
        {
            return new SolidColorBrush(Colors.CornflowerBlue);
        }

        private Brush GetCairoBackgroundBrush_Video()
        {
            string wallpaper = Settings.Instance.CairoBackgroundVideoPath;
            if (File.Exists(wallpaper))
            {
                // https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/how-to-paint-an-area-with-a-video
                MediaElement videoElement = new MediaElement();
                videoElement.Source = new Uri(wallpaper, UriKind.Relative);
                videoElement.LoadedBehavior = MediaState.Manual;
                videoElement.IsMuted = true;
                videoElement.MediaEnded += (o, a) => videoElement.Position = new TimeSpan(0, 0, 1);

                VisualBrush videoBrush = new VisualBrush();
                videoBrush.Visual = videoElement;
                videoBrush.AlignmentX = AlignmentX.Center;
                videoBrush.AlignmentY = AlignmentY.Center;
                videoBrush.TileMode = TileMode.None;
                videoBrush.Stretch = Stretch.UniformToFill;

                videoElement.Play();

                return videoBrush;
            }
            else
            {
                return GetCairoBackgroundBrush_Windows();
            }
        }

        private Brush GetCairoBackgroundBrush_Image(string wallpaper, CairoWallpaperStyle wallpaperStyle)
        {
            ImageBrush backgroundImageBrush = null;
            if (!string.IsNullOrWhiteSpace(wallpaper) && Shell.Exists(wallpaper))
            {
                TryAndEat(() =>
                {
                    Uri backgroundImageUri = new Uri(wallpaper, UriKind.Absolute);
                    BitmapImage backgroundBitmapImage = new BitmapImage(backgroundImageUri);
                    backgroundBitmapImage.Freeze();
                    backgroundImageBrush = new ImageBrush(backgroundBitmapImage);

                    switch (wallpaperStyle)
                    {
                        case CairoWallpaperStyle.Tile:
                            backgroundImageBrush.AlignmentX = AlignmentX.Left;
                            backgroundImageBrush.AlignmentY = AlignmentY.Top;
                            backgroundImageBrush.TileMode = TileMode.Tile;
                            backgroundImageBrush.Stretch = Stretch.Fill; // stretch to fill viewport, which is pixel size of image, as WPF is DPI-aware
                            backgroundImageBrush.Viewport = new Rect(0, 0, (backgroundImageBrush.ImageSource as BitmapSource).PixelWidth, (backgroundImageBrush.ImageSource as BitmapSource).PixelHeight);
                            backgroundImageBrush.ViewportUnits = BrushMappingMode.Absolute;
                            break;
                        case CairoWallpaperStyle.Center:
                            // need to find a way to ignore image DPI for this case
                            backgroundImageBrush.AlignmentX = AlignmentX.Center;
                            backgroundImageBrush.AlignmentY = AlignmentY.Center;
                            backgroundImageBrush.TileMode = TileMode.None;
                            backgroundImageBrush.Stretch = Stretch.None;
                            break;
                        case CairoWallpaperStyle.Fit:
                            backgroundImageBrush.AlignmentX = AlignmentX.Center;
                            backgroundImageBrush.AlignmentY = AlignmentY.Center;
                            backgroundImageBrush.TileMode = TileMode.None;
                            backgroundImageBrush.Stretch = Stretch.Uniform;
                            break;
                        case CairoWallpaperStyle.Fill:
                        case CairoWallpaperStyle.Span: // TODO: Impliment multiple monitor backgrounds
                            backgroundImageBrush.AlignmentX = AlignmentX.Center;
                            backgroundImageBrush.AlignmentY = AlignmentY.Center;
                            backgroundImageBrush.TileMode = TileMode.None;
                            backgroundImageBrush.Stretch = Stretch.UniformToFill;
                            break;
                        case CairoWallpaperStyle.Stretch:
                        default:
                            backgroundImageBrush.AlignmentX = AlignmentX.Center;
                            backgroundImageBrush.AlignmentY = AlignmentY.Center;
                            backgroundImageBrush.TileMode = TileMode.None;
                            backgroundImageBrush.Stretch = Stretch.Fill;
                            break;
                    }
                });
                backgroundImageBrush.Freeze();
            }
            return backgroundImageBrush;
        }

        private Brush GetCairoBackgroundBrush_BingImageOfTheDay()
        {
            ImageBrush backgroundImageBrush = null;
            TryAndEat(() =>
            {

                SupportingClasses.BingPhotoOfDayClient.BingWallPaperClient client = new SupportingClasses.BingPhotoOfDayClient.BingWallPaperClient();
                client.DownLoad();

                BitmapImage backgroundBitmapImage = client.WPFPhotoOfTheDay as BitmapImage;
                backgroundBitmapImage.Freeze();
                backgroundImageBrush = new ImageBrush(backgroundBitmapImage);

                CairoWallpaperStyle wallpaperStyle = CairoWallpaperStyle.Stretch;
                if (Enum.IsDefined(typeof(CairoWallpaperStyle), Settings.Instance.BingWallpaperStyle))
                    wallpaperStyle = (CairoWallpaperStyle)Settings.Instance.BingWallpaperStyle;

                switch (wallpaperStyle)
                {
                    case CairoWallpaperStyle.Tile:
                        backgroundImageBrush.AlignmentX = AlignmentX.Left;
                        backgroundImageBrush.AlignmentY = AlignmentY.Top;
                        backgroundImageBrush.TileMode = TileMode.Tile;
                        backgroundImageBrush.Stretch = Stretch.Fill; // stretch to fill viewport, which is pixel size of image, as WPF is DPI-aware
                        backgroundImageBrush.Viewport = new Rect(0, 0, (backgroundImageBrush.ImageSource as BitmapSource).PixelWidth, (backgroundImageBrush.ImageSource as BitmapSource).PixelHeight);
                        backgroundImageBrush.ViewportUnits = BrushMappingMode.Absolute;
                        break;
                    case CairoWallpaperStyle.Center:
                        // need to find a way to ignore image DPI for this case
                        backgroundImageBrush.AlignmentX = AlignmentX.Center;
                        backgroundImageBrush.AlignmentY = AlignmentY.Center;
                        backgroundImageBrush.TileMode = TileMode.None;
                        backgroundImageBrush.Stretch = Stretch.None;
                        break;
                    case CairoWallpaperStyle.Fit:
                        backgroundImageBrush.AlignmentX = AlignmentX.Center;
                        backgroundImageBrush.AlignmentY = AlignmentY.Center;
                        backgroundImageBrush.TileMode = TileMode.None;
                        backgroundImageBrush.Stretch = Stretch.Uniform;
                        break;
                    case CairoWallpaperStyle.Fill:
                    case CairoWallpaperStyle.Span: // TODO: Impliment multiple monitor backgrounds
                        backgroundImageBrush.AlignmentX = AlignmentX.Center;
                        backgroundImageBrush.AlignmentY = AlignmentY.Center;
                        backgroundImageBrush.TileMode = TileMode.None;
                        backgroundImageBrush.Stretch = Stretch.UniformToFill;
                        break;
                    case CairoWallpaperStyle.Stretch:
                    default:
                        backgroundImageBrush.AlignmentX = AlignmentX.Center;
                        backgroundImageBrush.AlignmentY = AlignmentY.Center;
                        backgroundImageBrush.TileMode = TileMode.None;
                        backgroundImageBrush.Stretch = Stretch.Fill;
                        break;
                }
            });

            backgroundImageBrush.Freeze();

            return backgroundImageBrush;
        }

        internal void ReloadBackground()
        {
            BackgroundBrush = null;
            setBackground();
        }
        #endregion

        #region Desktop overlay
        public void ToggleOverlay()
        {
            IsOverlayOpen = !IsOverlayOpen;
        }

        private void ShowOverlay()
        {
            Topmost = true;
            NativeMethods.SetForegroundWindow(helper.Handle);
            grid.Background = new SolidColorBrush(Color.FromArgb(0x88, 0, 0, 0));
            Background = null;
        }

        private void CloseOverlay()
        {
            if (Topmost)
            {
                Topmost = false;
                SendToBottom();
                grid.Background = new SolidColorBrush(Color.FromArgb(0x00, 0, 0, 0));
                setBackground();
            }
        }
        #endregion

        #region Files and folders
        private void executeFileAction(string action, string path, Button sender)
        {
            SystemFile file = new SystemFile(path);

            if (action == "openFolder")
            {
                if (Settings.Instance.EnableDynamicDesktop)
                {
                    Navigate(path);
                }
                else
                {
                    FolderHelper.OpenLocation(path);
                }
            }
            else if (action == "rename" || action == "addStack" || action == "removeStack" || action == "openWithShell")
            {
                CustomCommands.PerformAction(action, path, sender);
            }
            else if (action != "cut" && action != "copy" && action != "link")
            {
                if (Startup.DesktopWindow != null)
                    Startup.DesktopWindow.IsOverlayOpen = false;
            }
        }

        private void executeFolderAction(string action, string path)
        {
            if (action == "paste")
            {
                Icons.Location.PasteFromClipboard();
            }
            else if (action == "addStack" || action == "removeStack")
            {
                // no need to dismiss overlay for these actions
                CustomCommands.PerformAction(action, path);
            }
            else if (action != "")
            {
                CustomCommands.PerformAction(action, path);

                if (Startup.DesktopWindow != null)
                    Startup.DesktopWindow.IsOverlayOpen = false;
            }
        }
        #endregion

        #region Navigation
        public void Navigate(string newLocation)
        {
            navigationManager.NavigateTo(newLocation);
        }

        private void NavigationManager_Navigating(string navigationPath)
        {
            CurrentLocation = navigationPath;
        }

        public void NavigateBackward()
        {
            navigationManager.NavigateBackward();
        }

        public void NavigateForward()
        {
            navigationManager.NavigateForward();
        }

        internal void ClearNavigation()
        {
            navigationManager.Clear();
        }
        #endregion

        #region Drop
        private bool isDropMove = false;
        private void CairoDesktopWindow_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(typeof(SystemFile)))
            {
                if ((e.KeyStates & DragDropKeyStates.RightMouseButton) != 0)
                {
                    e.Effects = DragDropEffects.Copy;
                    isDropMove = false;
                }
                else if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) != 0)
                {
                    if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
                    {
                        e.Effects = DragDropEffects.Copy;
                        isDropMove = false;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                        isDropMove = true;
                    }
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
                isDropMove = false;
            }

            e.Handled = true;
        }

        private void CairoDesktopWindow_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (e.Data.GetDataPresent(typeof(SystemFile)))
            {
                SystemFile dropData = e.Data.GetData(typeof(SystemFile)) as SystemFile;
                fileNames = new string[] { dropData.FullName };
            }

            if (fileNames != null)
            {
                if (!isDropMove) Icons.Location.CopyInto(fileNames);
                else if (isDropMove) Icons.Location.MoveInto(fileNames);

                e.Handled = true;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}