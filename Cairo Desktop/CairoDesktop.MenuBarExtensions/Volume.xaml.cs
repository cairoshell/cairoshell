using ManagedShell.Common.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class Volume : UserControl
    {
        private bool _isLoaded;
        private DispatcherTimer _volumeIconTimer;

        public Volume()
        {
            InitializeComponent();
        }

        private void VolumeIcon_Tick()
        {
            if (VolumeHelper.IsVolumeMuted())
            {
                imgOpenVolume.Source = FindResource("VolumeMuteIcon") as ImageSource;
            }
            else if (VolumeHelper.GetMasterVolume() <= 0)
            {
                imgOpenVolume.Source = FindResource("VolumeOffIcon") as ImageSource;
            }
            else if (VolumeHelper.GetMasterVolume() < 0.5)
            {
                imgOpenVolume.Source = FindResource("VolumeLowIcon") as ImageSource;
            }
            else
            {
                imgOpenVolume.Source = FindResource("VolumeIcon") as ImageSource;
            }
        }

        private void miOpenVolume_Click(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartProcess("sndvol.exe", "-f " + (int)(((ushort)(System.Windows.Forms.Cursor.Position.X / DpiHelper.DpiScaleAdjustment)) | (uint)((int)ActualHeight << 16)));
        }

        private void miOpenSoundSettings_Click(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartProcess("mmsys.cpl");
        }

        private void miOpenVolumeMixer_Click(object sender, RoutedEventArgs e)
        {
            ShellHelper.StartProcess("sndvol.exe", "-T " + (int)(((ushort)(System.Windows.Forms.Cursor.Position.X / DpiHelper.DpiScaleAdjustment)) | (uint)((int)ActualHeight << 16)));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }

            VolumeIcon_Tick();

            _volumeIconTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 2), DispatcherPriority.Background, delegate
            {
                VolumeIcon_Tick();
            }, Dispatcher);

            _isLoaded = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                return;
            }

            _volumeIconTimer?.Stop();
            _volumeIconTimer = null;

            _isLoaded = false;
        }
    }
}
