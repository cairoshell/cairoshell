using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ManagedShell.Common.Helpers;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for MenuExtraVolume.xaml
    /// </summary>
    public partial class MenuExtraVolume : UserControl
    {
        public MenuExtraVolume()
        {
            InitializeComponent();

            initializeVolumeIcon();
        }

        private void initializeVolumeIcon()
        {
            volumeIcon_Tick();

            // update volume icon periodically
            DispatcherTimer volumeIconTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 2), DispatcherPriority.Background, delegate
            {
                volumeIcon_Tick();
            }, Dispatcher);
        }

        private void volumeIcon_Tick()
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
    }
}
