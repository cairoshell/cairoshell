using CairoDesktop.Common.Localization;
using System;
using System.Collections.Generic;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.Common
{
    public class SystemPower
    {
        // Keyed by title so repeated triggers (e.g. Alt+F4 fired from both the desktop and the
        // desktop overlay in quick succession) bring the existing confirmation to the front instead
        // of stacking duplicate dialogs.
        private static readonly Dictionary<string, CairoMessage> _openConfirmations = new Dictionary<string, CairoMessage>();

        private static void ShowActionConfirmation(string message, string title, CairoMessageImage image, string okButtonText, string cancelButtonText, Action systemAction)
        {
            if (_openConfirmations.TryGetValue(title, out CairoMessage existingDialog))
            {
                existingDialog.Activate();
                return;
            }

            CairoMessage msgDialog = CairoMessage.ShowOkCancel(message, title, image, okButtonText, cancelButtonText,
                result =>
                {
                    if (result == true)
                        systemAction();
                });

            _openConfirmations[title] = msgDialog;
            msgDialog.Closed += (sender, e) => _openConfirmations.Remove(title);
        }

        public static void ShowShutdownConfirmation()
        {
            ShowActionConfirmation(DisplayString.sShutDown_Info
                , DisplayString.sShutDown_Title
                , CairoMessageImage.ShutDown
                , DisplayString.sShutDown_ShutDown
                , DisplayString.sInterface_Cancel
                , PowerHelper.Shutdown);
        }

        public static void ShowRebootConfirmation()
        {
            ShowActionConfirmation(DisplayString.sRestart_Info
                , DisplayString.sRestart_Title
                , CairoMessageImage.Restart
                , DisplayString.sRestart_Restart
                , DisplayString.sInterface_Cancel
                , PowerHelper.Reboot);
        }

        public static void ShowLogOffConfirmation()
        {
            ShowActionConfirmation(DisplayString.sLogoff_Info
                , DisplayString.sLogoff_Title
                , CairoMessageImage.LogOff
                , DisplayString.sLogoff_Logoff
                , DisplayString.sInterface_Cancel
                , ShellHelper.Logoff);
        }
    }
}
