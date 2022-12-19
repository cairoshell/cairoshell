using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Markup;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.Themes
{
    [MarkupExtensionReturnType(typeof(SolidColorBrush))]
    public class WindowsThemeColorManager : MarkupExtension
    {

        public double Opacity
        {
            get;
            set;
        } = 1.0;

        public Color FallbackColor
        {
            get;
            set;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMMERSIVE_COLOR_PREFERENCE
        {
            public uint dwColorSetIndex;
            public uint crStartColor;
            public uint crAccentColor;
        }

        [DllImport("uxtheme.dll", EntryPoint = "#120")]
        private static extern IntPtr GetUserColorPreference(ref IMMERSIVE_COLOR_PREFERENCE pcpPreference, bool fForceReload);


        private static uint ToUint(System.Windows.Media.Color c)
        {
            return (uint)(((int)c.B << 16) | ((int)c.G << 8) | (int)c.R);
        }

        private static Color ToColor(uint c)
        {
            int R = (int)(c & 0xFF) % 256;
            int G = (int)((c >> 8) & 0xFFFF) % 256;
            int B = (int)(c >> 16) % 256;

            return Color.FromArgb(255, (byte)R, (byte)G, (byte)B);
        }

        private Color GetAccent()
        {
            if (!EnvironmentHelper.IsWindows8OrBetter)
                return FallbackColor;

            IMMERSIVE_COLOR_PREFERENCE get = new IMMERSIVE_COLOR_PREFERENCE();
            GetUserColorPreference(ref get, true);
            var res = ToColor(get.crStartColor);
            return ToColor(get.crStartColor);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var result = new SolidColorBrush(GetAccent());
            result.Opacity = this.Opacity;
            return result;
        }
    }
}
