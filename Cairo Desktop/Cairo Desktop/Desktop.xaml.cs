using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.SupportingClasses;
using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Services;
using ManagedShell.AppBar;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Interop;
using ManagedShell.ShellFolders;
using ManagedShell.ShellFolders.Enums;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;

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
        private readonly AppBarManager _appBarManager;
        private readonly DesktopManager _desktopManager;
        private readonly FullScreenHelper _fullScreenHelper;
        private readonly FileOperationWorker _fileWorker;
        private readonly ISettingsUIService _settingsUiService;

        public bool AllowClose;
        public IntPtr Handle;
        public EventHandler WorkAreaChanged;

        private Brush BackgroundBrush { get; set; }
        #endregion

        public Desktop(DesktopManager desktopManager, AppBarManager appBarManager, FullScreenHelper fullScreenHelper, ISettingsUIService settingsUiService)
        {
            InitializeComponent();

            _appBarManager = appBarManager;
            _desktopManager = desktopManager;
            _fullScreenHelper = fullScreenHelper;
            _fileWorker = new FileOperationWorker();
            _settingsUiService = settingsUiService;

            if (_desktopManager.ShellWindow != null)
            {
                AllowsTransparency = false;
            }

            setSize();
            setGridPosition();
            setBackground();

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;

            _fullScreenHelper.FullScreenApps.CollectionChanged += FullScreenApps_CollectionChanged;
        }

        private void SetupPostInit()
        {
            WindowHelper.HideWindowFromTasks(Handle);

            SendToBottom();

            _desktopManager.ConfigureDesktop();
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
            if (msg == (int)NativeMethods.WM.WINDOWPOSCHANGING)
            {
                if (_desktopManager.ShellWindow == null && !_desktopManager.AllowProgmanChild)
                {
                    // Extract the WINDOWPOS structure corresponding to this message
                    NativeMethods.WINDOWPOS wndPos = NativeMethods.WINDOWPOS.FromMessage(lParam);

                    // Determine if the z-order is changing (absence of SWP_NOZORDER flag)
                    if ((wndPos.flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER) == 0)
                    {
                        // get the lowest window, we want to insert after it
                        IntPtr lowestHwnd = WindowHelper.GetLowestDesktopParentHwnd();

                        if (lowestHwnd != IntPtr.Zero)
                        {
                            wndPos.hwndInsertAfter = NativeMethods.GetWindow(lowestHwnd, NativeMethods.GetWindow_Cmd.GW_HWNDPREV);
                        }
                        else
                        {
                            // this will bring us lower but not always as low as we want to go; better than nothing
                            wndPos.hwndInsertAfter = (IntPtr)NativeMethods.WindowZOrder.HWND_BOTTOM;
                        }

                        wndPos.UpdateMessage(lParam);
                    }
                }
            }
            else if (msg == (int)NativeMethods.WM.SETTINGCHANGE &&
                    wParam.ToInt32() == (int)NativeMethods.SPI.SETWORKAREA)
            {
                WorkAreaChanged?.Invoke(this, new EventArgs());
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (altF4Pressed) // Show the Shutdown Confirmation Window
            {
                SystemPower.ShowShutdownConfirmation();
                altF4Pressed = false;
                e.Cancel = true;
            }
            else if (!AllowClose) // Eat it !!!
            {
                e.Cancel = true;
            }

            if (!e.Cancel)
            {
                // unsubscribe from things
                Settings.Instance.PropertyChanged -= Settings_PropertyChanged;
                _fullScreenHelper.FullScreenApps.CollectionChanged -= FullScreenApps_CollectionChanged;
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
            helper = new WindowInteropHelper(this);
            Handle = helper.Handle;
            HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(WndProc));

            SetupPostInit();
        }

        private void grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(ScrollViewer) && (_desktopManager.DesktopToolbar == null || !_desktopManager.DesktopToolbar.IsContextMenuOpen))
            {
                _desktopManager.IsOverlayOpen = false;
            }
        }

        private void CairoDesktopWindow_LocationChanged(object sender, EventArgs e)
        {
            ResetPosition();
        }

        private void CairoDesktopWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // handle desktop context menu
            // we check source here so that we don't override the rename textbox context menu
            if (_desktopManager.DesktopLocation != null && (e.OriginalSource.GetType() == typeof(ScrollViewer) || e.Source.GetType() == typeof(Desktop) || e.Source.GetType() == typeof(Grid)))
            {
                ShellFolderContextMenu cm = new ShellFolderContextMenu(_desktopManager.DesktopLocation, HandleFolderAction, GetFolderCommandBuilder());
                e.Handled = true;
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
                    _desktopManager.NavigationManager.NavigateBackward();
                    break;
                case MouseButton.XButton2:
                    _desktopManager.NavigationManager.NavigateForward();
                    break;
            }
        }
        #endregion

        #region Change notifications
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                            if (_fullScreenHelper.FullScreenApps.Count > 0)
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
        public void SendToBottom()
        {
            if (_desktopManager.ShellWindow == null && !_desktopManager.AllowProgmanChild)
            {
                WindowHelper.ShowWindowDesktop(Handle);
            }
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
            Width = SystemInformation.VirtualScreen.Width / DpiHelper.DpiScale;
            Height = (SystemInformation.VirtualScreen.Height / DpiHelper.DpiScale) - (EnvironmentHelper.IsAppRunningAsShell ? 0 : 1); // making size of screen causes explorer to send ABN_FULLSCREENAPP
        }

        private void setGridPosition()
        {
            double top = SystemInformation.WorkingArea.Top / DpiHelper.DpiScale;
            double left = SystemInformation.WorkingArea.Left / DpiHelper.DpiScale;

            if (_desktopManager.ShellWindow != null || _desktopManager.AllowProgmanChild)
            {
                top = (0 - SystemInformation.VirtualScreen.Top + SystemInformation.WorkingArea.Top) / DpiHelper.DpiScale;
                left = (0 - SystemInformation.VirtualScreen.Left + SystemInformation.WorkingArea.Left) / DpiHelper.DpiScale;
            }

            grid.Width = WindowManager.PrimaryMonitorWorkArea.Width / DpiHelper.DpiScale;

            if (Settings.Instance.TaskbarMode == 1)
            {
                // special case, since work area is not reduced with this setting
                // this keeps the desktop going beneath the TaskBar
                // get the TaskBar's height
                double dpiScale = 1;
                AppBarScreen screen = AppBarScreen.FromPrimaryScreen();
                NativeMethods.Rect workAreaRect = _appBarManager.GetWorkArea(ref dpiScale, screen, false, false);

                grid.Height = (WindowManager.PrimaryMonitorWorkArea.Height / DpiHelper.DpiScale) - ((screen.Bounds.Bottom - workAreaRect.Bottom) / dpiScale);

                if (Settings.Instance.TaskbarPosition == 1)
                {
                    top += (workAreaRect.Top - SystemInformation.WorkingArea.Top) / dpiScale;
                }
            }
            else
            {
                grid.Height = WindowManager.PrimaryMonitorWorkArea.Height / DpiHelper.DpiScale;
            }

            grid.Margin = new Thickness(left, top, 0, 0);
        }
        #endregion

        #region Background
        private void setBackground()
        {
            if (_desktopManager.ShellWindow != null)
            {
                try
                {
                    if (BackgroundBrush == null)
                    {
                        BackgroundBrush = GetCairoBackgroundBrush();
                    }

                    Background = BackgroundBrush;
                }
                catch
                {
                    ShellLogger.Error("Failed setting desktop background.");
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
                    // return GetCairoBackgroundBrush_Picsum();
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
                ShellLogger.Debug("Problem loading Windows background", ex);
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
            int[] regRgb = { 0, 0, 0 };

            TryAndEat(() =>
            {
                string regColor = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "Background", "0 0 0") as string;
                string[] regRgbStr = regColor.Split(' ');

                if (regRgbStr.Length == 3)
                {
                    for (int i = 0; i < regRgbStr.Length; i++)
                    {
                        if (int.TryParse(regRgbStr[i], out int color))
                        {
                            regRgb[i] = color;
                        }
                    }
                }
            });

            return new SolidColorBrush(Color.FromRgb((byte)regRgb[0], (byte)regRgb[1], (byte)regRgb[2]));
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
            if (!string.IsNullOrWhiteSpace(wallpaper) && ShellHelper.Exists(wallpaper))
            {
                try
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
                            backgroundImageBrush.Stretch =
                                Stretch
                                    .Fill; // stretch to fill viewport, which is pixel size of image, as WPF is DPI-aware
                            backgroundImageBrush.Viewport = new Rect(0, 0,
                                (backgroundImageBrush.ImageSource as BitmapSource).PixelWidth,
                                (backgroundImageBrush.ImageSource as BitmapSource).PixelHeight);
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

                    backgroundImageBrush.Freeze();
                }
                catch
                {
                    backgroundImageBrush = null;
                }
            }
            return backgroundImageBrush;
        }

        private Brush GetCairoBackgroundBrush_BingImageOfTheDay()
        {
            ImageBrush backgroundImageBrush = null;
            try
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

                backgroundImageBrush.Freeze();
            }
            catch
            {
                return GetCairoBackgroundBrush_Windows();
            }

            return backgroundImageBrush;
        }

        private Brush GetCairoBackgroundBrush_Picsum()
        {
            ImageBrush backgroundImageBrush = null;
            try
            {

                PicSumWallpaperClient client = new PicSumWallpaperClient(1920, 1080, true, 8);

                BitmapImage backgroundBitmapImage = client.Wallpaper as BitmapImage;
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

                backgroundImageBrush.Freeze();
            }
            catch
            {
                return GetCairoBackgroundBrush_Windows();
            }

            return backgroundImageBrush;
        }

        internal void ReloadBackground()
        {
            BackgroundBrush = null;
            setBackground();
        }
        #endregion

        #region Desktop context menu
        private ShellMenuCommandBuilder GetFolderCommandBuilder()
        {
            if (_desktopManager.DesktopLocation == null)
            {
                return new ShellMenuCommandBuilder();
            }

            ShellMenuCommandBuilder builder = new ShellMenuCommandBuilder();
            MFT flags = MFT.BYCOMMAND;

            if (!_desktopManager.DesktopLocation.IsFileSystem)
            {
                flags |= MFT.DISABLED;
            }

            builder.AddCommand(new ShellMenuCommand
            {
                Flags = MFT.BYCOMMAND, // enable this entry always
                Label = Localization.DisplayString.sStacks_OpenInNewWindow,
                UID = (uint)CairoContextMenuItem.OpenInNewWindow
            });

            if (StacksManager.Instance.StackLocations.All(i => i.Path != _desktopManager.DesktopLocation.Path))
            {
                builder.AddCommand(new ShellMenuCommand
                {
                    Flags = MFT.BYCOMMAND, // enable this entry always
                    Label = Localization.DisplayString.sInterface_AddToStacks,
                    UID = (uint)CairoContextMenuItem.AddToStacks
                });
            }
            else
            {
                builder.AddCommand(new ShellMenuCommand
                {
                    Flags = MFT.BYCOMMAND, // enable this entry always
                    Label = Localization.DisplayString.sInterface_RemoveFromStacks,
                    UID = (uint)CairoContextMenuItem.RemoveFromStacks
                });
            }
            builder.AddSeparator();

            builder.AddCommand(new ShellMenuCommand
            {
                Flags = flags,
                Label = Localization.DisplayString.sInterface_Paste,
                UID = (uint)CommonContextMenuItem.Paste
            });
            builder.AddSeparator();

            if (_desktopManager.DesktopLocation.IsFileSystem && _desktopManager.DesktopLocation.IsFolder)
            {
                builder.AddShellNewMenu();
                builder.AddSeparator();
            }

            if (EnvironmentHelper.IsWindows10OrBetter || !_desktopManager.DesktopLocation.IsDesktop)
            {
                // On Windows < 10, this does the same thing as Personalize when on the Desktop folder.
                // Show it only if this isn't the Desktop folder, or if we are on Windows 10 (or later).
                builder.AddCommand(new ShellMenuCommand
                {
                    Flags = flags,
                    Label = Localization.DisplayString.sInterface_Properties,
                    UID = (uint)CommonContextMenuItem.Properties
                });
                builder.AddSeparator();
            }

            if (!EnvironmentHelper.IsAppRunningAsShell)
            {
                // Don't show this if we are shell, since this launches a UWP app
                builder.AddCommand(new ShellMenuCommand
                {
                    Flags = MFT.BYCOMMAND, // enable this entry always
                    Label = Localization.DisplayString.sDesktop_DisplaySettings,
                    UID = (uint)CairoContextMenuItem.DisplaySettings
                });
            }

            builder.AddCommand(new ShellMenuCommand
            {
                Flags = MFT.BYCOMMAND, // enable this entry always
                Label = Localization.DisplayString.sDesktop_Personalize,
                UID = (uint)CairoContextMenuItem.Personalize
            });

            return builder;
        }
        
        private void HandleFolderAction(uint action, string path)
        {
            // TODO: Use command system
            switch (action)
            {
                case (uint)CommonContextMenuItem.Paste:
                    _fileWorker.PasteFromClipboard(path);
                    break;
                case (uint)CommonContextMenuItem.Properties:
                    _desktopManager.IsOverlayOpen = false;
                    CustomCommands.PerformAction(CustomCommands.Actions.Properties, path);
                    break;
                case (uint)CairoContextMenuItem.OpenInNewWindow:
                    _desktopManager.IsOverlayOpen = false;
                    CustomCommands.PerformAction(CustomCommands.Actions.OpenWithShell, path);
                    break;
                case (uint)CairoContextMenuItem.DisplaySettings:
                    _desktopManager.IsOverlayOpen = false;
                    CustomCommands.PerformAction(CustomCommands.Actions.DisplaySettings, path);
                    break;
                case (uint)CairoContextMenuItem.Personalize when EnvironmentHelper.IsAppRunningAsShell:
                    _desktopManager.IsOverlayOpen = false;
                    _settingsUiService?.Show("desktop");
                    break;
                case (uint)CairoContextMenuItem.Personalize:
                    _desktopManager.IsOverlayOpen = false;
                    CustomCommands.PerformAction(CustomCommands.Actions.Personalize, path);
                    break;
                case (uint)CairoContextMenuItem.AddToStacks:
                    CustomCommands.PerformAction(CustomCommands.Actions.AddStack, path);
                    break;
                case (uint)CairoContextMenuItem.RemoveFromStacks:
                    CustomCommands.PerformAction(CustomCommands.Actions.RemoveStack, path);
                    break;
                default:
                    // must be "New" menu
                    CairoApplication.Current.Dispatcher.Invoke(() =>
                    {
                        if (_desktopManager.IsOverlayOpen)
                        {
                            _desktopManager.DesktopOverlayWindow.Activate();
                        }
                        else
                        {
                            Activate();
                        }
                    });
                    // watch for new file to be created so we can perform an action
                    _desktopManager.DesktopLocation.Files.CollectionChanged += ShellNew_FileCreated;
                    break;
            }
        }

        private void ShellNew_FileCreated(object sender, NotifyCollectionChangedEventArgs e)
        {
            // file was created due to usage of the shell new menu
            CairoApplication.Current.Dispatcher.Invoke(() =>
            {
                _desktopManager.DesktopLocation.Files.CollectionChanged -= ShellNew_FileCreated;

                _desktopManager.DesktopIconsControl.RenameNewIcon = true;
            });
        }
        #endregion

        #region Drop
        private bool isDropMove;
        private void CairoDesktopWindow_DragOver(object sender, DragEventArgs e)
        {
            if (_desktopManager.DesktopLocation != null && _desktopManager.DesktopLocation.IsFileSystem && (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(typeof(ShellItem))))
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
            if (e.Data.GetDataPresent(typeof(ShellItem)) && e.Data.GetData(typeof(ShellItem)) is ShellItem dropData)
            {
                fileNames = new[] { dropData.Path };
            }

            if (fileNames != null && _desktopManager.DesktopLocation != null && _fileWorker != null)
            {
                _fileWorker.PerformOperation(isDropMove ? FileOperation.Move : FileOperation.Copy, 
                    fileNames, 
                    _desktopManager.DesktopLocation.Path);

                e.Handled = true;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}