using System.Collections.Generic;

using ManagedShell.AppBar;

namespace CairoDesktop.SupportingClasses
{
    sealed class ScreenEqualityComparer : IEqualityComparer<AppBarScreen>
    {
        public bool Equals(AppBarScreen x, AppBarScreen y) => x?.DeviceName == y?.DeviceName;
        public int GetHashCode(AppBarScreen obj) => obj?.GetHashCode() ?? 42;

        public static ScreenEqualityComparer Instance { get; } = new ScreenEqualityComparer();
        ScreenEqualityComparer() { }
    }
}
