using CairoDesktop.Interop;
using CairoDesktop.Localization;
using System;

namespace CairoDesktop.Common
{
    public class SystemPower
    {
        private static void ShowActionConfirmation(string message, string title, string imageSource, string okButtonText, string cancelButtonText, Action systemAction)
        {
            bool? actionChoice = CairoMessage.ShowOkCancel(message, title, imageSource, okButtonText, cancelButtonText);
            if (actionChoice.HasValue && actionChoice.Value)
                systemAction();
        }

        public static void ShowShutdownConfirmation()
        {
            ShowActionConfirmation(DisplayString.sShutDown_Info
                , DisplayString.sShutDown_Title
                , "Resources/shutdownIcon.png"
                , DisplayString.sShutDown_ShutDown
                , DisplayString.sInterface_Cancel
                , Shell.Shutdown);
        }

        public static void ShowRebootConfirmation()
        {
            ShowActionConfirmation(DisplayString.sRestart_Info
                , DisplayString.sRestart_Title
                , "Resources/restartIcon.png"
                , DisplayString.sRestart_Restart
                , DisplayString.sInterface_Cancel
                , Shell.Reboot);
        }

        public static void ShowLogOffConfirmation()
        {
            ShowActionConfirmation(DisplayString.sLogoff_Info
                , DisplayString.sLogoff_Title
                , "Resources/logoffIcon.png"
                , DisplayString.sLogoff_Logoff
                , DisplayString.sInterface_Cancel
                , Shell.Logoff);
        }
    }
}
