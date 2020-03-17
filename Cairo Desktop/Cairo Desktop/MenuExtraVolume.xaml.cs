using CairoDesktop.Common;
using CairoDesktop.Interop;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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
            }, this.Dispatcher);
        }

        private void volumeIcon_Tick()
        {
            if (VolumeUtilities.IsVolumeMuted())
            {
                imgOpenVolume.Source = this.FindResource("VolumeMuteIcon") as ImageSource;
            }
            else if (VolumeUtilities.GetMasterVolume() <= 0)
            {
                imgOpenVolume.Source = this.FindResource("VolumeOffIcon") as ImageSource;
            }
            else if (VolumeUtilities.GetMasterVolume() < 0.5)
            {
                imgOpenVolume.Source = this.FindResource("VolumeLowIcon") as ImageSource;
            }
            else
            {
                imgOpenVolume.Source = this.FindResource("VolumeIcon") as ImageSource;
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
