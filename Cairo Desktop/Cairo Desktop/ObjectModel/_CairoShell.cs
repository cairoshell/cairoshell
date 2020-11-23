using CairoDesktop.Common.DesignPatterns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Threading;

namespace CairoDesktop.ObjectModel
{
    public sealed class _CairoShell // : SingletonObject<_CairoShell>
    {
        public _CairoShell()
        {
            CairoMenu = new List<MenuItem>();
            PlacesMenu = new List<MenuItem>();
            MenuExtras = new List<MenuExtra>();
            ShellExtensions = new List<ShellExtension>();
            ShellServices = new Dictionary<Type, ShellService>();
        }

        public static event Action<_CairoShell> OnStart;

        public static event Action<_CairoShell> OnQuit;

        public void Start()
        {
            OnStart?.Invoke(this);
        }

        public void Quit()
        {
            OnQuit?.Invoke(this);
        }

        public List<ShellExtension> ShellExtensions { get; private set; }

        public Dictionary<Type, ShellService> ShellServices { get; private set; }

        public List<ICommand> Commands { get; private set; }

        public List<MenuItem> CairoMenu { get; private set; }

        public List<MenuItem> PlacesMenu { get; private set; }

        public List<MenuExtra> MenuExtras { get; private set; }
    }
}