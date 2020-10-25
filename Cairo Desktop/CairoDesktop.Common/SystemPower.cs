using CairoDesktop.Interop;
using CairoDesktop.Localization;
using System;

namespace CairoDesktop.Common
{
    public class SystemPower
    {
        private static void ShowActionConfirmation(string message, string title, CairoMessageImage image, string okButtonText, string cancelButtonText, Action systemAction)
        {
            CairoMessage.ShowOkCancel(message, title, image, okButtonText, cancelButtonText,
                result =>
                {
                    if (result == true)
                        systemAction();
                });
        }

        public static void ShowShutdownConfirmation()
        {
            ShowActionConfirmation(DisplayString.sShutDown_Info
                , DisplayString.sShutDown_Title
                , CairoMessageImage.ShutDown
                , DisplayString.sShutDown_ShutDown
                , DisplayString.sInterface_Cancel
                , Shell.Shutdown);
        }

        public static void ShowRebootConfirmation()
        {
            ShowActionConfirmation(DisplayString.sRestart_Info
                , DisplayString.sRestart_Title
                , CairoMessageImage.Restart
                , DisplayString.sRestart_Restart
                , DisplayString.sInterface_Cancel
                , Shell.Reboot);
        }

        public static void ShowLogOffConfirmation()
        {
            ShowActionConfirmation(DisplayString.sLogoff_Info
                , DisplayString.sLogoff_Title
                , CairoMessageImage.LogOff
                , DisplayString.sLogoff_Logoff
                , DisplayString.sInterface_Cancel
                , Shell.Logoff);
        }
    }
}
