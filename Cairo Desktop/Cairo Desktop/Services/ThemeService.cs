using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Interop;

namespace CairoDesktop.Services
{
    public class ThemeService : IDisposable
    {
        private readonly Func<ICairoApplication> _cairoApplicationFactory;

        private const string THEME_DEFAULT_FILE = "Cairo";
        private const string THEME_DEFAULT = "Default";
        private const string THEME_FOLDER = "Themes";
        private const string THEME_EXT = "xaml";

        private static readonly string[] THEME_LOCATIONS = {
            AppDomain.CurrentDomain.BaseDirectory,
            CairoApplication.CairoApplicationDataFolder
        };

        public ThemeService(Func<ICairoApplication> cairoApplicationFactory)
        {
            _cairoApplicationFactory = cairoApplicationFactory;
            MigrateSettings();

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        private void MigrateSettings()
        {
            if (Settings.Instance.CairoTheme.EndsWith(".xaml"))
            {
                Settings.Instance.CairoTheme = Settings.Instance.CairoTheme.Replace(".xaml", "");
            }
        }

        public void SetThemeFromSettings(ICairoApplication cairoApplication)
        {
            SetTheme(THEME_DEFAULT, cairoApplication);
            if (Settings.Instance.CairoTheme != THEME_DEFAULT)
            {
                SetTheme(Settings.Instance.CairoTheme, cairoApplication);
            }
            SetDarkMode(cairoApplication);
        }

        private void SetTheme(string theme, ICairoApplication cairoApplication)
        {
            string themeFilePath = "";

            if (theme == THEME_DEFAULT)
            {
                cairoApplication.ClearResources();
                themeFilePath = Path.ChangeExtension(Path.Combine(THEME_FOLDER, THEME_DEFAULT_FILE), THEME_EXT);
            }
            else
            {
                foreach (var location in THEME_LOCATIONS)
                {
                    themeFilePath = Path.ChangeExtension(Path.Combine(location, THEME_FOLDER, theme), THEME_EXT);

                    if (File.Exists(themeFilePath))
                    {
                        break;
                    }
                }

                if (string.IsNullOrEmpty(themeFilePath))
                {
                    return;
                }
            }

            try
            {
                ResourceDictionary newRes = new ResourceDictionary
                {
                    Source = new Uri(themeFilePath, UriKind.RelativeOrAbsolute)
                };
                cairoApplication.AddResource(newRes);
            }
            catch (Exception e)
            {
                ShellLogger.Error($"ThemeService: Unable to load theme {theme}: {e.Message}");
            }
        }

        public List<string> GetThemes()
        {
            List<string> themes = new List<string>();
            themes.Add(THEME_DEFAULT);

            foreach (var location in THEME_LOCATIONS)
            {
                string themePath = Path.Combine(location, THEME_FOLDER);
                if (ShellHelper.Exists(themePath))
                {
                    foreach (string subStr in Directory.GetFiles(themePath).Where(s => Path.GetExtension(s).Contains(THEME_EXT)))
                    {
                        string theme = Path.GetFileNameWithoutExtension(subStr);
                        if (!themes.Contains(theme))
                        {
                            themes.Add(theme);
                        }
                    }
                }
            }

            return themes;
        }

        private void SetDarkMode(ICairoApplication cairoApplication)
        {
            // Enable dark mode support if specified by theme
            
            if (!EnvironmentHelper.IsWindows10DarkModeSupported)
            {
                return;
            }
            
            // Unfortunately, the dark mode API does not allow setting this more than once,
            // so once we go dark, there's no going back. As such, there's no reason to
            // specify light mode here for now (as that's the default).
            bool? darkMode = cairoApplication.FindResource("EnableDarkMode") as bool?;
            if (darkMode == true)
            {
                WindowHelper.SetDarkModePreference(NativeMethods.PreferredAppMode.ForceDark);
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName) || e.PropertyName != "CairoTheme")
            {
                return;
            }

            SetThemeFromSettings(_cairoApplicationFactory?.Invoke());
        }

        public void Dispose()
        {
            Settings.Instance.PropertyChanged -= Settings_PropertyChanged;
        }
    }
}
