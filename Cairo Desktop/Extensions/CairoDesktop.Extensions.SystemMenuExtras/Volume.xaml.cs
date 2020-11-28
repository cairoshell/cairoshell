using CairoDesktop.Common;
using CairoDesktop.Interop;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CairoDesktop.Extensions.SystemMenuExtras
{
    public partial class Volume : UserControl
    {
        public Volume()
        {
            InitializeComponent();

            InitializeVolumeIcon();
        }

        private void InitializeVolumeIcon()
        {
            VolumeIcon_Tick();

            DispatcherTimer volumeIconTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 2), DispatcherPriority.Background, delegate
            {
                VolumeIcon_Tick();
            }, Dispatcher);
        }

        private void VolumeIcon_Tick()
        {
            if (VolumeUtilities.IsVolumeMuted())
            {
                imgOpenVolume.Source = FindResource("VolumeMuteIcon") as ImageSource;
            }
            else if (VolumeUtilities.GetMasterVolume() <= 0)
            {
                imgOpenVolume.Source = FindResource("VolumeOffIcon") as ImageSource;
            }
            else if (VolumeUtilities.GetMasterVolume() < 0.5)
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
            Shell.StartProcess("sndvol.exe", "-f " + (int)(((ushort)(System.Windows.Forms.Cursor.Position.X / Shell.DpiScaleAdjustment)) | (uint)((int)ActualHeight << 16)));
        }

        private void miOpenSoundSettings_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("mmsys.cpl");
        }

        private void miOpenVolumeMixer_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("sndvol.exe", "-T " + (int)(((ushort)(System.Windows.Forms.Cursor.Position.X / Shell.DpiScaleAdjustment)) | (uint)((int)ActualHeight << 16)));
        }
    }
}
