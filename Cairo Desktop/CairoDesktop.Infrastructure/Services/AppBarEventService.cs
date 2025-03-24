using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.AppBar;
using System;

namespace CairoDesktop.Infrastructure.Services
{
    public class AppBarEventService
    {
        public EventHandler<AppBarEventArgs> AppBarEvent;

        public void NotifyAppBarEvent(AppBarWindow sender, AppBarEventReason reason)
        {
            AppBarEventArgs args = new AppBarEventArgs { Reason = reason };
            AppBarEvent?.Invoke(sender, args);
        }
    }
}
