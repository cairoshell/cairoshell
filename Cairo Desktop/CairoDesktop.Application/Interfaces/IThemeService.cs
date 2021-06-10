using System.Collections.Generic;

namespace CairoDesktop.Application.Interfaces
{
    public interface IThemeService
    {
        List<string> GetThemes();

        void SetTheme(string theme);

        void SetThemeFromSettings();
    }
}