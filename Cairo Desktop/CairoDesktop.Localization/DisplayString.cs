using System.Collections.Generic;

namespace CairoDesktop.Localization
{
    public class DisplayString
    {
        public static List<KeyValuePair<string, string>> Languages = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("English", "en_US"),
            new KeyValuePair<string, string>("Chinese (Simplified) 简体中文", "zh_CN"),
            new KeyValuePair<string, string>("Czech", "cs_CZ"),
            new KeyValuePair<string, string>("Dutch (Nederlands)", "nl_NL"),
            new KeyValuePair<string, string>("Français", "fr_FR"),
            new KeyValuePair<string, string>("German", "de_DE"),
            new KeyValuePair<string, string>("Português (Brasil)", "pt_BR"),
            new KeyValuePair<string, string>("Spanish (España)", "es_ES"),
            new KeyValuePair<string, string>("Svenska", "sv_SE")
        };

        public DisplayString()
        {

        }

        private static string getString(string stringName)
        {
            // return stringName; // debug

            Dictionary<string, string> lang;
            bool isDefault = false;
            string useLang = Configuration.Settings.Language.ToLower();

            if (useLang.StartsWith("fr_"))
            {
                lang = Language.fr_FR;
            }
            else if (useLang.StartsWith("pt_"))
            {
                lang = Language.pt_BR;
            }
            else if (useLang.StartsWith("sv_"))
            {
                lang = Language.sv_SE;
            }
            else if (useLang.StartsWith("zh_"))
            {
                lang = Language.zh_CN;
            }
            else if (useLang.StartsWith("cs_"))
            {
                lang = Language.cs_CZ;
            }
            else if (useLang.StartsWith("de_"))
            {
                lang = Language.de_DE;
            }
            else if (useLang.StartsWith("es_"))
            {
                lang = Language.es_ES;
            }
            else if (useLang.StartsWith("nl_"))
            {
                lang = Language.nl_NL;
            }
            else
            {
                lang = Language.en_US;
                isDefault = true;
            }

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

        public static string sCairoMenu_Hibernate
        {
            get
            {
                return getString("sCairoMenu_Hibernate");
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

        public static string sInterface_RunAsUser
        {
            get
            {
                return getString("sInterface_RunAsUser");
            }
        }

        public static string sInterface_AddToStacks
        {
            get
            {
                return getString("sInterface_AddToStacks");
            }
        }

        public static string sInterface_RemoveFromStacks
        {
            get
            {
                return getString("sInterface_RemoveFromStacks");
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

        public static string sAppGrabber_Untitled
        {
            get
            {
                return getString("sAppGrabber_Untitled");
            }
        }

        public static string sAppGrabber_Category_Accessories
        {
            get
            {
                return getString("sAppGrabber_Category_Accessories");
            }
        }

        public static string sAppGrabber_Category_Productivity
        {
            get
            {
                return getString("sAppGrabber_Category_Productivity");
            }
        }

        public static string sAppGrabber_Category_Development
        {
            get
            {
                return getString("sAppGrabber_Category_Development");
            }
        }

        public static string sAppGrabber_Category_Graphics
        {
            get
            {
                return getString("sAppGrabber_Category_Graphics");
            }
        }

        public static string sAppGrabber_Category_Media
        {
            get
            {
                return getString("sAppGrabber_Category_Media");
            }
        }

        public static string sAppGrabber_Category_Internet
        {
            get
            {
                return getString("sAppGrabber_Category_Internet");
            }
        }

        public static string sAppGrabber_Category_Games
        {
            get
            {
                return getString("sAppGrabber_Category_Games");
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

        public static string sProgramsMenu_AddToQuickLaunch
        {
            get
            {
                return getString("sProgramsMenu_AddToQuickLaunch");
            }
        }

        public static string sProgramsMenu_ChangeCategory
        {
            get
            {
                return getString("sProgramsMenu_ChangeCategory");
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

        public static string sStacks_OpenOnDesktop
        {
            get
            {
                return getString("sStacks_OpenOnDesktop");
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

        public static string sMenuBar_Volume
        {
            get
            {
                return getString("sMenuBar_Volume");
            }
        }

        public static string sMenuBar_OpenSoundSettings
        {
            get
            {
                return getString("sMenuBar_OpenSoundSettings");
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

        public static string sDesktop_CurrentFolder
        {
            get
            {
                return getString("sDesktop_CurrentFolder");
            }
        }

        public static string sDesktop_Back
        {
            get
            {
                return getString("sDesktop_Back");
            }
        }

        public static string sDesktop_Forward
        {
            get
            {
                return getString("sDesktop_Forward");
            }
        }

        public static string sDesktop_Browse
        {
            get
            {
                return getString("sDesktop_Browse");
            }
        }

        public static string sDesktop_Home
        {
            get
            {
                return getString("sDesktop_Home");
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

        public static string sTaskbar_Maximize
        {
            get
            {
                return getString("sTaskbar_Maximize");
            }
        }

        public static string sTaskbar_NewWindow
        {
            get
            {
                return getString("sTaskbar_NewWindow");
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

        public static string sTaskbar_TaskListToolTip
        {
            get
            {
                return getString("sTaskbar_TaskListToolTip");
            }
        }

        public static string sTaskbar_DesktopOverlayToolTip
        {
            get
            {
                return getString("sTaskbar_DesktopOverlayToolTip");
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

        public static string sError_CantOpenAppWiz
        {
            get
            {
                return getString("sError_CantOpenAppWiz");
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

        public static string sSettings_Advanced
        {
            get
            {
                return getString("sSettings_Advanced");
            }
        }

        public static string sSettings_Behavior
        {
            get
            {
                return getString("sSettings_Behavior");
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

        public static string sSettings_General_RunAtLogOn
        {
            get
            {
                return getString("sSettings_General_RunAtLogOn");
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

        public static string sSettings_General_FoldersOpenDesktopOverlay
        {
            get
            {
                return getString("sSettings_General_FoldersOpenDesktopOverlay");
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

        public static string sSettings_MenuBar_EnableCairoMenuHotKey
        {
            get
            {
                return getString("sSettings_MenuBar_EnableCairoMenuHotKey");
            }
        }

        public static string sSettings_MenuBar_EnableMenuBarBlur
        {
            get
            {
                return getString("sSettings_MenuBar_EnableMenuBarBlur");
            }
        }

        public static string sSettings_MenuBar_EnableMenuBarMultiMon
        {
            get
            {
                return getString("sSettings_MenuBar_EnableMenuBarMultiMon");
            }
        }

        public static string sSettings_MenuBar_ShowHibernate
        {
            get
            {
                return getString("sSettings_MenuBar_ShowHibernate");
            }
        }

        public static string sSettings_MenuBar_ProgramsMenuLayout
        {
            get
            {
                return getString("sSettings_MenuBar_ProgramsMenuLayout");
            }
        }

        public static string sSettings_MenuBar_ProgramsMenuLayoutRight
        {
            get
            {
                return getString("sSettings_MenuBar_ProgramsMenuLayoutRight");
            }
        }

        public static string sSettings_MenuBar_ProgramsMenuLayoutLeft
        {
            get
            {
                return getString("sSettings_MenuBar_ProgramsMenuLayoutLeft");
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

        public static string sSettings_Desktop_EnableDesktopOverlayHotKey
        {
            get
            {
                return getString("sSettings_Desktop_EnableDesktopOverlayHotKey");
            }
        }

        public static string sSettings_IconSize
        {
            get
            {
                return getString("sSettings_IconSize");
            }
        }

        public static string sSettings_IconSizeLarge
        {
            get
            {
                return getString("sSettings_IconSizeLarge");
            }
        }

        public static string sSettings_IconSizeMedium
        {
            get
            {
                return getString("sSettings_IconSizeMedium");
            }
        }

        public static string sSettings_IconSizeSmall
        {
            get
            {
                return getString("sSettings_IconSizeSmall");
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

        public static string sSettings_Taskbar_DisplayModeAutoHide
        {
            get
            {
                return getString("sSettings_Taskbar_DisplayModeAutoHide");
            }
        }

        public static string sSettings_Taskbar_EnableTaskbarMultiMon
        {
            get
            {
                return getString("sSettings_Taskbar_EnableTaskbarMultiMon");
            }
        }

        public static string sSettings_Taskbar_FullWidthTaskBar
        {
            get
            {
                return getString("sSettings_Taskbar_FullWidthTaskBar");
            }
        }

        public static string sSettings_Taskbar_MiddleClick
        {
            get
            {
                return getString("sSettings_Taskbar_MiddleClick");
            }
        }

        public static string sSettings_Taskbar_MiddleClickNewWindow
        {
            get
            {
                return getString("sSettings_Taskbar_MiddleClickNewWindow");
            }
        }

        public static string sSettings_Taskbar_MiddleClickCloseWindow
        {
            get
            {
                return getString("sSettings_Taskbar_MiddleClickCloseWindow");
            }
        }

        public static string sSettings_Advanced_LoggingLevel
        {
            get
            {
                return getString("sSettings_Advanced_LoggingLevel");
            }
        }

        public static string sSettings_Advanced_Shell
        {
            get
            {
                return getString("sSettings_Advanced_Shell");
            }
        }

        public static string sSettings_Advanced_CairoIsShell
        {
            get
            {
                return getString("sSettings_Advanced_CairoIsShell");
            }
        }

        public static string sSettings_Advanced_ExplorerIsShell
        {
            get
            {
                return getString("sSettings_Advanced_ExplorerIsShell");
            }
        }

        public static string sSettings_Advanced_SetCairoAsShell
        {
            get
            {
                return getString("sSettings_Advanced_SetCairoAsShell");
            }
        }

        public static string sSettings_Advanced_SetExplorerAsShell
        {
            get
            {
                return getString("sSettings_Advanced_SetExplorerAsShell");
            }
        }

        public static string sSettings_Advanced_ShellChanged
        {
            get
            {
                return getString("sSettings_Advanced_ShellChanged");
            }
        }

        public static string sSettings_Advanced_ShellChangedText
        {
            get
            {
                return getString("sSettings_Advanced_ShellChangedText");
            }
        }

        public static string sSettings_Advanced_LogOffNow
        {
            get
            {
                return getString("sSettings_Advanced_LogOffNow");
            }
        }

        public static string sSettings_Advanced_LogOffLater
        {
            get
            {
                return getString("sSettings_Advanced_LogOffLater");
            }
        }

        public static string sWelcome_StartTour
        {
            get
            {
                return getString("sWelcome_StartTour");
            }
        }

        public static string sWelcome_FinishTour
        {
            get
            {
                return getString("sWelcome_FinishTour");
            }
        }

        public static string sWelcome_Welcome
        {
            get
            {
                return getString("sWelcome_Welcome");
            }
        }

        public static string sWelcome_SelectLanguage
        {
            get
            {
                return getString("sWelcome_SelectLanguage");
            }
        }

        public static string sWelcome_ChangingLanguage
        {
            get
            {
                return getString("sWelcome_ChangingLanguage");
            }
        }

        public static string sWelcome_ChangingLanguageText
        {
            get
            {
                return getString("sWelcome_ChangingLanguageText");
            }
        }

        public static string sWelcome_MenuBar
        {
            get
            {
                return getString("sWelcome_MenuBar");
            }
        }

        public static string sWelcome_DynamicDesktop
        {
            get
            {
                return getString("sWelcome_DynamicDesktop");
            }
        }

        public static string sWelcome_Taskbar
        {
            get
            {
                return getString("sWelcome_Taskbar");
            }
        }

        public static string sWelcome_MenuBarText
        {
            get
            {
                return getString("sWelcome_MenuBarText");
            }
        }

        public static string sWelcome_MenuBarSec1Heading
        {
            get
            {
                return getString("sWelcome_MenuBarSec1Heading");
            }
        }

        public static string sWelcome_MenuBarSec1Text
        {
            get
            {
                return getString("sWelcome_MenuBarSec1Text");
            }
        }

        public static string sWelcome_MenuBarSec2Heading
        {
            get
            {
                return getString("sWelcome_MenuBarSec2Heading");
            }
        }

        public static string sWelcome_MenuBarSec2Text
        {
            get
            {
                return getString("sWelcome_MenuBarSec2Text");
            }
        }

        public static string sWelcome_MenuBarSec3Heading
        {
            get
            {
                return getString("sWelcome_MenuBarSec3Heading");
            }
        }

        public static string sWelcome_MenuBarSec3Text
        {
            get
            {
                return getString("sWelcome_MenuBarSec3Text");
            }
        }

        public static string sWelcome_MenuBarSec4Heading
        {
            get
            {
                return getString("sWelcome_MenuBarSec4Heading");
            }
        }

        public static string sWelcome_MenuBarSec4Text
        {
            get
            {
                return getString("sWelcome_MenuBarSec4Text");
            }
        }

        public static string sWelcome_MenuBarSec5Heading
        {
            get
            {
                return getString("sWelcome_MenuBarSec5Heading");
            }
        }

        public static string sWelcome_MenuBarSec5Text
        {
            get
            {
                return getString("sWelcome_MenuBarSec5Text");
            }
        }

        public static string sWelcome_MenuBarSec6Heading
        {
            get
            {
                return getString("sWelcome_MenuBarSec6Heading");
            }
        }

        public static string sWelcome_MenuBarSec6Text
        {
            get
            {
                return getString("sWelcome_MenuBarSec6Text");
            }
        }

        public static string sWelcome_DesktopText
        {
            get
            {
                return getString("sWelcome_DesktopText");
            }
        }

        public static string sWelcome_DesktopSec1Heading
        {
            get
            {
                return getString("sWelcome_DesktopSec1Heading");
            }
        }

        public static string sWelcome_DesktopSec1Text
        {
            get
            {
                return getString("sWelcome_DesktopSec1Text");
            }
        }

        public static string sWelcome_DesktopSec2Heading
        {
            get
            {
                return getString("sWelcome_DesktopSec2Heading");
            }
        }

        public static string sWelcome_DesktopSec2Text
        {
            get
            {
                return getString("sWelcome_DesktopSec2Text");
            }
        }

        public static string sWelcome_TaskbarText
        {
            get
            {
                return getString("sWelcome_TaskbarText");
            }
        }

        public static string sWelcome_TaskbarSec1Heading
        {
            get
            {
                return getString("sWelcome_TaskbarSec1Heading");
            }
        }

        public static string sWelcome_TaskbarSec1Text
        {
            get
            {
                return getString("sWelcome_TaskbarSec1Text");
            }
        }

        public static string sWelcome_TaskbarSec2Heading
        {
            get
            {
                return getString("sWelcome_TaskbarSec2Heading");
            }
        }

        public static string sWelcome_TaskbarSec2Text
        {
            get
            {
                return getString("sWelcome_TaskbarSec2Text");
            }
        }

        public static string sWelcome_TaskbarSec3Heading
        {
            get
            {
                return getString("sWelcome_TaskbarSec3Heading");
            }
        }

        public static string sWelcome_TaskbarSec3Text
        {
            get
            {
                return getString("sWelcome_TaskbarSec3Text");
            }
        }
    }
}
