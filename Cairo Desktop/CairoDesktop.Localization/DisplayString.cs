using System.Collections.Generic;

namespace CairoDesktop.Localization
{
    public class DisplayString
    {
        public DisplayString()
        {

        }

        private static string getString(string stringName)
        {
            // return stringName; // debug

            Dictionary<string, string> lang;
            bool isDefault = false;

            /*if (Configuration.Settings.Language != "en_US")
            {
                lang = new Dictionary<string, string>();
                isDefault = true;
            }
            else
            {*/
                lang = Language.en_US;
                isDefault = true;
            //}

            if (lang.ContainsKey(stringName))
                return lang[stringName];
            else
            {
                // default is en_US - return string from there if not found in set language
                if (!isDefault)
                {
                    lang = Language.en_US;
                    if (lang.ContainsKey(stringName))
                        return lang[stringName];
                }
            }

            return stringName;
        }

        public static string sProgramsMenu
        {
            get
            {
                return getString("sProgramsMenu");
            }
        }

        public static string sPlacesMenu
        {
            get
            {
                return getString("sPlacesMenu");
            }
        }

        public static string sCairoMenu_AboutCairo
        {
            get
            {
                return getString("sCairoMenu_AboutCairo");
            }
        }

        public static string sCairoMenu_CheckForUpdates
        {
            get
            {
                return getString("sCairoMenu_CheckForUpdates");
            }
        }

        public static string sCairoMenu_CairoSettings
        {
            get
            {
                return getString("sCairoMenu_CairoSettings");
            }
        }

        public static string sCairoMenu_WindowsControlPanel
        {
            get
            {
                return getString("sCairoMenu_WindowsControlPanel");
            }
        }

        public static string sCairoMenu_WindowsSettings
        {
            get
            {
                return getString("sCairoMenu_WindowsSettings");
            }
        }

        public static string sCairoMenu_Run
        {
            get
            {
                return getString("sCairoMenu_Run");
            }
        }

        public static string sCairoMenu_TaskManager
        {
            get
            {
                return getString("sCairoMenu_TaskManager");
            }
        }

        public static string sCairoMenu_ExitCairo
        {
            get
            {
                return getString("sCairoMenu_ExitCairo");
            }
        }

        public static string sCairoMenu_Sleep
        {
            get
            {
                return getString("sCairoMenu_Sleep");
            }
        }

        public static string sCairoMenu_Restart
        {
            get
            {
                return getString("sCairoMenu_Restart");
            }
        }

        public static string sCairoMenu_ShutDown
        {
            get
            {
                return getString("sCairoMenu_ShutDown");
            }
        }

        public static string sCairoMenu_LogOff
        {
            get
            {
                return getString("sCairoMenu_LogOff");
            }
        }

        public static string sAbout_Version
        {
            get
            {
                return getString("sAbout_Version");
            }
        }

        public static string sAbout_PreRelease
        {
            get
            {
                return getString("sAbout_PreRelease");
            }
        }

        public static string sAbout_Copyright
        {
            get
            {
                return getString("sAbout_Copyright");
            }
        }

        public static string sInterface_OK
        {
            get
            {
                return getString("sInterface_OK");
            }
        }

        public static string sInterface_Cancel
        {
            get
            {
                return getString("sInterface_Cancel");
            }
        }

        public static string sInterface_Continue
        {
            get
            {
                return getString("sInterface_Continue");
            }
        }

        public static string sInterface_Finish
        {
            get
            {
                return getString("sInterface_Finish");
            }
        }

        public static string sInterface_Yes
        {
            get
            {
                return getString("sInterface_Yes");
            }
        }

        public static string sInterface_No
        {
            get
            {
                return getString("sInterface_No");
            }
        }

        public static string sInterface_Open
        {
            get
            {
                return getString("sInterface_Open");
            }
        }

        public static string sInterface_OpenWith
        {
            get
            {
                return getString("sInterface_OpenWith");
            }
        }

        public static string sInterface_OpenAsAdministrator
        {
            get
            {
                return getString("sInterface_OpenAsAdministrator");
            }
        }

        public static string sInterface_AddToStacks
        {
            get
            {
                return getString("sInterface_AddToStacks");
            }
        }

        public static string sInterface_Copy
        {
            get
            {
                return getString("sInterface_Copy");
            }
        }

        public static string sInterface_Paste
        {
            get
            {
                return getString("sInterface_Paste");
            }
        }

        public static string sInterface_Delete
        {
            get
            {
                return getString("sInterface_Delete");
            }
        }

        public static string sInterface_Rename
        {
            get
            {
                return getString("sInterface_Rename");
            }
        }

        public static string sInterface_Properties
        {
            get
            {
                return getString("sInterface_Properties");
            }
        }

        public static string sInterface_Browse
        {
            get
            {
                return getString("sInterface_Browse");
            }
        }

        public static string sAppGrabber
        {
            get
            {
                return getString("sAppGrabber");
            }
        }

        public static string sAppGrabber_PleaseWait
        {
            get
            {
                return getString("sAppGrabber_PleaseWait");
            }
        }

        public static string sAppGrabber_Page1Title
        {
            get
            {
                return getString("sAppGrabber_Page1Title");
            }
        }

        public static string sAppGrabber_Page1Line1
        {
            get
            {
                return getString("sAppGrabber_Page1Line1");
            }
        }

        public static string sAppGrabber_Page1Line2
        {
            get
            {
                return getString("sAppGrabber_Page1Line2");
            }
        }

        public static string sAppGrabber_ProgramsMenuItems
        {
            get
            {
                return getString("sAppGrabber_ProgramsMenuItems");
            }
        }

        public static string sAppGrabber_InstalledApplications
        {
            get
            {
                return getString("sAppGrabber_InstalledApplications");
            }
        }

        public static string sAppGrabber_Page2Title
        {
            get
            {
                return getString("sAppGrabber_Page2Title");
            }
        }

        public static string sAppGrabber_Page2Line1
        {
            get
            {
                return getString("sAppGrabber_Page2Line1");
            }
        }

        public static string sAppGrabber_Page2Directions
        {
            get
            {
                return getString("sAppGrabber_Page2Directions");
            }
        }

        public static string sAppGrabber_Page2Directions1
        {
            get
            {
                return getString("sAppGrabber_Page2Directions1");
            }
        }

        public static string sAppGrabber_Page2Directions2
        {
            get
            {
                return getString("sAppGrabber_Page2Directions2");
            }
        }

        public static string sAppGrabber_Page2Directions3
        {
            get
            {
                return getString("sAppGrabber_Page2Directions3");
            }
        }

        public static string sAppGrabber_Page2Directions4
        {
            get
            {
                return getString("sAppGrabber_Page2Directions4");
            }
        }

        public static string sAppGrabber_Page2Directions5
        {
            get
            {
                return getString("sAppGrabber_Page2Directions5");
            }
        }

        public static string sAppGrabber_Page2Directions6
        {
            get
            {
                return getString("sAppGrabber_Page2Directions6");
            }
        }

        public static string sAppGrabber_ProgramCategories
        {
            get
            {
                return getString("sAppGrabber_ProgramCategories");
            }
        }

        public static string sAppGrabber_Hide
        {
            get
            {
                return getString("sAppGrabber_Hide");
            }
        }

        public static string sAppGrabber_QuickLaunch
        {
            get
            {
                return getString("sAppGrabber_QuickLaunch");
            }
        }

        public static string sAppGrabber_Uncategorized
        {
            get
            {
                return getString("sAppGrabber_Uncategorized");
            }
        }

        public static string sAppGrabber_All
        {
            get
            {
                return getString("sAppGrabber_All");
            }
        }

        public static string sRun_Title
        {
            get
            {
                return getString("sRun_Title");
            }
        }

        public static string sRun_Info
        {
            get
            {
                return getString("sRun_Info");
            }
        }

        public static string sExitCairo_Title
        {
            get
            {
                return getString("sExitCairo_Title");
            }
        }

        public static string sExitCairo_Info
        {
            get
            {
                return getString("sExitCairo_Info");
            }
        }

        public static string sExitCairo_ExitCairo
        {
            get
            {
                return getString("sExitCairo_ExitCairo");
            }
        }

        public static string sLogoff_Title
        {
            get
            {
                return getString("sLogoff_Title");
            }
        }

        public static string sLogoff_Info
        {
            get
            {
                return getString("sLogoff_Info");
            }
        }

        public static string sLogoff_Logoff
        {
            get
            {
                return getString("sLogoff_Logoff");
            }
        }

        public static string sRestart_Title
        {
            get
            {
                return getString("sRestart_Title");
            }
        }

        public static string sRestart_Info
        {
            get
            {
                return getString("sRestart_Info");
            }
        }

        public static string sRestart_Restart
        {
            get
            {
                return getString("sRestart_Restart");
            }
        }

        public static string sShutDown_Title
        {
            get
            {
                return getString("sShutDown_Title");
            }
        }

        public static string sShutDown_Info
        {
            get
            {
                return getString("sShutDown_Info");
            }
        }

        public static string sShutDown_ShutDown
        {
            get
            {
                return getString("sShutDown_ShutDown");
            }
        }

        public static string sProgramsMenu_Empty
        {
            get
            {
                return getString("sProgramsMenu_Empty");
            }
        }

        public static string sProgramsMenu_UninstallAProgram
        {
            get
            {
                return getString("sProgramsMenu_UninstallAProgram");
            }
        }

        public static string sProgramsMenu_RemoveFromMenu
        {
            get
            {
                return getString("sProgramsMenu_RemoveFromMenu");
            }
        }

        public static string sProgramsMenu_RemoveTitle
        {
            get
            {
                return getString("sProgramsMenu_RemoveTitle");
            }
        }

        public static string sProgramsMenu_RemoveInfo
        {
            get
            {
                return getString("sProgramsMenu_RemoveInfo");
            }
        }

        public static string sProgramsMenu_AlwaysAdminTitle
        {
            get
            {
                return getString("sProgramsMenu_AlwaysAdminTitle");
            }
        }

        public static string sProgramsMenu_AlwaysAdminInfo
        {
            get
            {
                return getString("sProgramsMenu_AlwaysAdminInfo");
            }
        }

        public static string sProgramsMenu_Remove
        {
            get
            {
                return getString("sProgramsMenu_Remove");
            }
        }

        public static string sProgramsMenu_UWPInfo
        {
            get
            {
                return getString("sProgramsMenu_UWPInfo");
            }
        }

        public static string sPlacesMenu_Documents
        {
            get
            {
                return getString("sPlacesMenu_Documents");
            }
        }

        public static string sPlacesMenu_Downloads
        {
            get
            {
                return getString("sPlacesMenu_Downloads");
            }
        }

        public static string sPlacesMenu_Music
        {
            get
            {
                return getString("sPlacesMenu_Music");
            }
        }

        public static string sPlacesMenu_Pictures
        {
            get
            {
                return getString("sPlacesMenu_Pictures");
            }
        }

        public static string sPlacesMenu_Videos
        {
            get
            {
                return getString("sPlacesMenu_Videos");
            }
        }

        public static string sPlacesMenu_ThisPC
        {
            get
            {
                return getString("sPlacesMenu_ThisPC");
            }
        }

        public static string sPlacesMenu_ProgramFiles
        {
            get
            {
                return getString("sPlacesMenu_ProgramFiles");
            }
        }

        public static string sPlacesMenu_RecycleBin
        {
            get
            {
                return getString("sPlacesMenu_RecycleBin");
            }
        }

        public static string sStacks_Tooltip
        {
            get
            {
                return getString("sStacks_Tooltip");
            }
        }

        public static string sStacks_OpenInNewWindow
        {
            get
            {
                return getString("sStacks_OpenInNewWindow");
            }
        }

        public static string sStacks_Remove
        {
            get
            {
                return getString("sStacks_Remove");
            }
        }

        public static string sStacks_Empty
        {
            get
            {
                return getString("sStacks_Empty");
            }
        }

        public static string sMenuBar_OpenDateTimeSettings
        {
            get
            {
                return getString("sMenuBar_OpenDateTimeSettings");
            }
        }

        public static string sMenuBar_OpenActionCenter
        {
            get
            {
                return getString("sMenuBar_OpenActionCenter");
            }
        }

        public static string sMenuBar_ToggleNotificationArea
        {
            get
            {
                return getString("sMenuBar_ToggleNotificationArea");
            }
        }

        public static string sSearch_Title
        {
            get
            {
                return getString("sSearch_Title");
            }
        }

        public static string sSearch_ViewAllResults
        {
            get
            {
                return getString("sSearch_ViewAllResults");
            }
        }

        public static string sSearch_LastModified
        {
            get
            {
                return getString("sSearch_LastModified");
            }
        }

        public static string sSearch_Error
        {
            get
            {
                return getString("sSearch_Error");
            }
        }

        public static string sDesktop_Personalize
        {
            get
            {
                return getString("sDesktop_Personalize");
            }
        }

        public static string sDesktop_DeleteTitle
        {
            get
            {
                return getString("sDesktop_DeleteTitle");
            }
        }

        public static string sDesktop_DeleteInfo
        {
            get
            {
                return getString("sDesktop_DeleteInfo");
            }
        }

        public static string sDesktop_BrowseTitle
        {
            get
            {
                return getString("sDesktop_BrowseTitle");
            }
        }

        public static string sTaskbar_Empty
        {
            get
            {
                return getString("sTaskbar_Empty");
            }
        }

        public static string sTaskbar_Minimize
        {
            get
            {
                return getString("sTaskbar_Minimize");
            }
        }

        public static string sTaskbar_Restore
        {
            get
            {
                return getString("sTaskbar_Restore");
            }
        }

        public static string sTaskbar_Close
        {
            get
            {
                return getString("sTaskbar_Close");
            }
        }

        public static string sTaskbar_TaskView
        {
            get
            {
                return getString("sTaskbar_TaskView");
            }
        }

        public static string sError_OhNo
        {
            get
            {
                return getString("sError_OhNo");
            }
        }

        public static string sError_FileNotFoundInfo
        {
            get
            {
                return getString("sError_FileNotFoundInfo");
            }
        }

        public static string sSettings_General
        {
            get
            {
                return getString("sSettings_General");
            }
        }

        public static string sSettings_MenuBar
        {
            get
            {
                return getString("sSettings_MenuBar");
            }
        }

        public static string sSettings_Desktop
        {
            get
            {
                return getString("sSettings_Desktop");
            }
        }

        public static string sSettings_Taskbar
        {
            get
            {
                return getString("sSettings_Taskbar");
            }
        }

        public static string sSettings_Appearance
        {
            get
            {
                return getString("sSettings_Appearance");
            }
        }

        public static string sSettings_Default
        {
            get
            {
                return getString("sSettings_Default");
            }
        }

        public static string sSettings_RestartCairo
        {
            get
            {
                return getString("sSettings_RestartCairo");
            }
        }

        public static string sSettings_Restarting
        {
            get
            {
                return getString("sSettings_Restarting");
            }
        }

        public static string sSettings_General_UpdateCheck
        {
            get
            {
                return getString("sSettings_General_UpdateCheck");
            }
        }

        public static string sSettings_General_Language
        {
            get
            {
                return getString("sSettings_General_Language");
            }
        }

        public static string sSettings_General_Theme
        {
            get
            {
                return getString("sSettings_General_Theme");
            }
        }

        public static string sSettings_General_ThemeTooltip
        {
            get
            {
                return getString("sSettings_General_ThemeTooltip");
            }
        }

        public static string sSettings_General_TimeFormat
        {
            get
            {
                return getString("sSettings_General_TimeFormat");
            }
        }

        public static string sSettings_General_TimeFormatTooltip
        {
            get
            {
                return getString("sSettings_General_TimeFormatTooltip");
            }
        }

        public static string sSettings_General_DateFormat
        {
            get
            {
                return getString("sSettings_General_DateFormat");
            }
        }

        public static string sSettings_General_DateFormatTooltip
        {
            get
            {
                return getString("sSettings_General_DateFormatTooltip");
            }
        }

        public static string sSettings_General_FilesAndFolders
        {
            get
            {
                return getString("sSettings_General_FilesAndFolders");
            }
        }

        public static string sSettings_General_ShowSubDirectories
        {
            get
            {
                return getString("sSettings_General_ShowSubDirectories");
            }
        }

        public static string sSettings_General_ShowFileExtensions
        {
            get
            {
                return getString("sSettings_General_ShowFileExtensions");
            }
        }

        public static string sSettings_General_ForceSoftwareRendering
        {
            get
            {
                return getString("sSettings_General_ForceSoftwareRendering");
            }
        }

        public static string sSettings_General_FileManager
        {
            get
            {
                return getString("sSettings_General_FileManager");
            }
        }

        public static string sSettings_MenuBar_DefaultProgramsCategory
        {
            get
            {
                return getString("sSettings_MenuBar_DefaultProgramsCategory");
            }
        }

        public static string sSettings_MenuBar_EnableMenuBarShadow
        {
            get
            {
                return getString("sSettings_MenuBar_EnableMenuBarShadow");
            }
        }

        public static string sSettings_MenuBar_NotificationArea
        {
            get
            {
                return getString("sSettings_MenuBar_NotificationArea");
            }
        }

        public static string sSettings_MenuBar_EnableNotificationArea
        {
            get
            {
                return getString("sSettings_MenuBar_EnableNotificationArea");
            }
        }

        public static string sSettings_MenuBar_NotificationAreaError
        {
            get
            {
                return getString("sSettings_MenuBar_NotificationAreaError");
            }
        }

        public static string sSettings_MenuBar_ShowNotifyIcons
        {
            get
            {
                return getString("sSettings_MenuBar_ShowNotifyIcons");
            }
        }

        public static string sSettings_MenuBar_ShowNotifyIconsCollapsed
        {
            get
            {
                return getString("sSettings_MenuBar_ShowNotifyIconsCollapsed");
            }
        }

        public static string sSettings_MenuBar_ShowNotifyIconsAlways
        {
            get
            {
                return getString("sSettings_MenuBar_ShowNotifyIconsAlways");
            }
        }

        public static string sSettings_MenuBar_PeriodicallyRehook
        {
            get
            {
                return getString("sSettings_MenuBar_PeriodicallyRehook");
            }
        }

        public static string sSettings_Desktop_EnableDesktop
        {
            get
            {
                return getString("sSettings_Desktop_EnableDesktop");
            }
        }

        public static string sSettings_Desktop_DesktopHome
        {
            get
            {
                return getString("sSettings_Desktop_DesktopHome");
            }
        }

        public static string sSettings_Desktop_IconSize
        {
            get
            {
                return getString("sSettings_Desktop_IconSize");
            }
        }

        public static string sSettings_Desktop_IconSizeLarge
        {
            get
            {
                return getString("sSettings_Desktop_IconSizeLarge");
            }
        }

        public static string sSettings_Desktop_IconSizeSmall
        {
            get
            {
                return getString("sSettings_Desktop_IconSizeSmall");
            }
        }

        public static string sSettings_Desktop_LabelPosition
        {
            get
            {
                return getString("sSettings_Desktop_LabelPosition");
            }
        }

        public static string sSettings_Desktop_LabelPositionRight
        {
            get
            {
                return getString("sSettings_Desktop_LabelPositionRight");
            }
        }

        public static string sSettings_Desktop_LabelPositionBottom
        {
            get
            {
                return getString("sSettings_Desktop_LabelPositionBottom");
            }
        }

        public static string sSettings_Desktop_DynamicDesktop
        {
            get
            {
                return getString("sSettings_Desktop_DynamicDesktop");
            }
        }

        public static string sSettings_Desktop_EnableDynamicDesktop
        {
            get
            {
                return getString("sSettings_Desktop_EnableDynamicDesktop");
            }
        }

        public static string sSettings_Taskbar_EnableTaskbar
        {
            get
            {
                return getString("sSettings_Taskbar_EnableTaskbar");
            }
        }

        public static string sSettings_Taskbar_TaskbarPosition
        {
            get
            {
                return getString("sSettings_Taskbar_TaskbarPosition");
            }
        }

        public static string sSettings_Taskbar_PositionBottom
        {
            get
            {
                return getString("sSettings_Taskbar_PositionBottom");
            }
        }

        public static string sSettings_Taskbar_PositionTop
        {
            get
            {
                return getString("sSettings_Taskbar_PositionTop");
            }
        }

        public static string sSettings_Taskbar_DisplayMode
        {
            get
            {
                return getString("sSettings_Taskbar_DisplayMode");
            }
        }

        public static string sSettings_Taskbar_DisplayModeAppBar
        {
            get
            {
                return getString("sSettings_Taskbar_DisplayModeAppBar");
            }
        }

        public static string sSettings_Taskbar_DisplayModeOverlap
        {
            get
            {
                return getString("sSettings_Taskbar_DisplayModeOverlap");
            }
        }
    }
}
