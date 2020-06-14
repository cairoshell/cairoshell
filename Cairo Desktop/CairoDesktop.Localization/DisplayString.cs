using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CairoDesktop.Localization
{
    public class DisplayString
    {
        public static List<KeyValuePair<string, string>> Languages = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Chinese (Simplified) 简体中文", "zh_CN"),
            new KeyValuePair<string, string>("Czech", "cs_CZ"),
            new KeyValuePair<string, string>("Dutch (Nederlands)", "nl_NL"),
            new KeyValuePair<string, string>("English", "en_US"),
            new KeyValuePair<string, string>("Français", "fr_FR"),
            new KeyValuePair<string, string>("German", "de_DE"),
            new KeyValuePair<string, string>("Polski", "pl_PL"),
            new KeyValuePair<string, string>("Português (Brasil)", "pt_BR"),
            new KeyValuePair<string, string>("Русский", "ru_RU"),
            new KeyValuePair<string, string>("Spanish (España)", "es_ES"),
            new KeyValuePair<string, string>("Svenska", "sv_SE")
        };

        public DisplayString()
        {

        }

        private static string getString([CallerMemberName]string stringName = null)
        {
            Dictionary<string, string> lang;
            bool isDefault = false;
            string useLang = Configuration.Settings.Instance.Language.ToLower();

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
            else if (useLang.StartsWith("ru_"))
            {
                lang = Language.ru_RU;
            }
            else if (useLang.StartsWith("pl_"))
            {
                lang = Language.pl_PL;
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
                return getString();
            }
        }

        public static string sPlacesMenu
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_AboutCairo
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_CheckForUpdates
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_CairoSettings
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_WindowsControlPanel
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_WindowsSettings
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_Run
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_TaskManager
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_ExitCairo
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_Hibernate
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_Sleep
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_Restart
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_ShutDown
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_Lock
        {
            get
            {
                return getString();
            }
        }

        public static string sCairoMenu_LogOff
        {
            get
            {
                return getString();
            }
        }

        public static string sAbout_Version
        {
            get
            {
                return getString();
            }
        }

        public static string sAbout_PreRelease
        {
            get
            {
                return getString();
            }
        }

        public static string sAbout_Copyright
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_OK
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Cancel
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Continue
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Finish
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Yes
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_No
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Open
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_OpenWith
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_OpenAsAdministrator
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_RunAsUser
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_AddToStacks
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_RemoveFromStacks
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Copy
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Paste
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Delete
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Rename
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Properties
        {
            get
            {
                return getString();
            }
        }

        public static string sInterface_Browse
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_PleaseWait
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page1Title
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page1Line1
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page1Line2
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_ProgramsMenuItems
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_InstalledApplications
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Title
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Line1
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Line2
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Directions
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Directions1
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Directions2
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Directions3
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Directions4
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Directions5
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Page2Directions6
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_ProgramCategories
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Hide
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_QuickLaunch
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Uncategorized
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_All
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Untitled
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Category_Accessories
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Category_Productivity
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Category_Development
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Category_Graphics
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Category_Media
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Category_Internet
        {
            get
            {
                return getString();
            }
        }

        public static string sAppGrabber_Category_Games
        {
            get
            {
                return getString();
            }
        }

        public static string sRun_Title
        {
            get
            {
                return getString();
            }
        }

        public static string sRun_Info
        {
            get
            {
                return getString();
            }
        }

        public static string sExitCairo_Title
        {
            get
            {
                return getString();
            }
        }

        public static string sExitCairo_Info
        {
            get
            {
                return getString();
            }
        }

        public static string sExitCairo_ExitCairo
        {
            get
            {
                return getString();
            }
        }

        public static string sLogoff_Title
        {
            get
            {
                return getString();
            }
        }

        public static string sLogoff_Info
        {
            get
            {
                return getString();
            }
        }

        public static string sLogoff_Logoff
        {
            get
            {
                return getString();
            }
        }

        public static string sRestart_Title
        {
            get
            {
                return getString();
            }
        }

        public static string sRestart_Info
        {
            get
            {
                return getString();
            }
        }

        public static string sRestart_Restart
        {
            get
            {
                return getString();
            }
        }

        public static string sShutDown_Title
        {
            get
            {
                return getString();
            }
        }

        public static string sShutDown_Info
        {
            get
            {
                return getString();
            }
        }

        public static string sShutDown_ShutDown
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_Empty
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_UninstallAProgram
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_RemoveFromMenu
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_RemoveTitle
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_RemoveInfo
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_AlwaysAdminTitle
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_AlwaysAdminInfo
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_Remove
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_UWPInfo
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_AddToQuickLaunch
        {
            get
            {
                return getString();
            }
        }

        public static string sProgramsMenu_ChangeCategory
        {
            get
            {
                return getString();
            }
        }

        public static string sPlacesMenu_Documents
        {
            get
            {
                return getString();
            }
        }

        public static string sPlacesMenu_Downloads
        {
            get
            {
                return getString();
            }
        }

        public static string sPlacesMenu_Music
        {
            get
            {
                return getString();
            }
        }

        public static string sPlacesMenu_Pictures
        {
            get
            {
                return getString();
            }
        }

        public static string sPlacesMenu_Videos
        {
            get
            {
                return getString();
            }
        }

        public static string sPlacesMenu_ThisPC
        {
            get
            {
                return getString();
            }
        }

        public static string sPlacesMenu_ProgramFiles
        {
            get
            {
                return getString();
            }
        }

        public static string sPlacesMenu_RecycleBin
        {
            get
            {
                return getString();
            }
        }

        public static string sStacks_Tooltip
        {
            get
            {
                return getString();
            }
        }

        public static string sStacks_OpenInNewWindow
        {
            get
            {
                return getString();
            }
        }

        public static string sStacks_OpenOnDesktop
        {
            get
            {
                return getString();
            }
        }

        public static string sStacks_Remove
        {
            get
            {
                return getString();
            }
        }

        public static string sStacks_Empty
        {
            get
            {
                return getString();
            }
        }

        public static string sMenuBar_OpenDateTimeSettings
        {
            get
            {
                return getString();
            }
        }

        public static string sMenuBar_OpenActionCenter
        {
            get
            {
                return getString();
            }
        }

        public static string sMenuBar_ToggleNotificationArea
        {
            get
            {
                return getString();
            }
        }

        public static string sMenuBar_Volume
        {
            get
            {
                return getString();
            }
        }

        public static string sMenuBar_OpenSoundSettings
        {
            get
            {
                return getString();
            }
        }

        public static string sMenuBar_OpenVolumeMixer
        {
            get
            {
                return getString();
            }
        }

        public static string sSearch_Title
        {
            get
            {
                return getString();
            }
        }

        public static string sSearch_ViewAllResults
        {
            get
            {
                return getString();
            }
        }

        public static string sSearch_LastModified
        {
            get
            {
                return getString();
            }
        }

        public static string sSearch_Error
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_DisplaySettings
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_Personalize
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_DeleteTitle
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_DeleteInfo
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_BrowseTitle
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_CurrentFolder
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_Back
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_Forward
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_Browse
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_Home
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_Up
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_SetHome
        {
            get
            {
                return getString();
            }
        }

        public static string sDesktop_ClearHistory
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_Empty
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_Minimize
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_Restore
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_Move
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_Size
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_Maximize
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_NewWindow
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_Close
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_TaskView
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_TaskListToolTip
        {
            get
            {
                return getString();
            }
        }

        public static string sTaskbar_DesktopOverlayToolTip
        {
            get
            {
                return getString();
            }
        }

        public static string sError_OhNo
        {
            get
            {
                return getString();
            }
        }

        public static string sError_FileNotFoundInfo
        {
            get
            {
                return getString();
            }
        }

        public static string sError_CantOpenAppWiz
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Appearance
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Behavior
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Default
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_RestartCairo
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Restarting
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_UpdateCheck
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_RunAtLogOn
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_Language
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_Theme
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_ThemeTooltip
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_TimeFormat
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_TimeFormatTooltip
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_DateFormat
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_DateFormatTooltip
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_FilesAndFolders
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_ShowSubDirectories
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_ShowFileExtensions
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_ForceSoftwareRendering
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_FileManager
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_General_FoldersOpenDesktopOverlay
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_DefaultProgramsCategory
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableMenuBarShadow
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_NotificationArea
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableNotificationArea
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_NotificationAreaError
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_NotificationAreaTaskbarWarning
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_ShowNotifyIcons
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_ShowNotifyIconsCollapsed
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_ShowNotifyIconsAlways
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_CollapsibleIcons
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_PinnedIcons
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_NotificationAreaPinHelp
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableCairoMenuHotKey
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableMenuBarBlur
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableMenuBarMultiMon
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_ShowHibernate
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_ProgramsMenuLayout
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_ProgramsMenuLayoutRight
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_ProgramsMenuLayoutLeft
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_MenuExtras
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableMenuExtraVolume
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableMenuExtraActionCenter
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableMenuExtraClock
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_MenuBar_EnableMenuExtraSearch
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_EnableDesktop
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_DesktopHome
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_LabelPosition
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_LabelPositionRight
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_LabelPositionBottom
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_DynamicDesktop
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_EnableDynamicDesktop
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_EnableDesktopOverlayHotKey
        {
            get
            {
                return getString();
            }
        }


        public static string sSettings_Desktop_DesktopBackgroundSettings
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundType
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundType_windowsDefaultBackground
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundType_cairoImageWallpaper
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundType_cairoVideoWallpaper
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundType_bingWallpaper
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_Background_Path
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_Background_Style
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundStyle_Tile
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundStyle_Center
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundStyle_Fit
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundStyle_Fill
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundStyle_Stretch
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Desktop_BackgroundStyle_Span
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_IconSize
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_IconSizeLarge
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_IconSizeMedium
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_IconSizeSmall
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_EnableTaskbar
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_TaskbarPosition
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_PositionBottom
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_PositionTop
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_DisplayMode
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_DisplayModeAppBar
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_DisplayModeOverlap
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_DisplayModeAutoHide
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_EnableTaskbarMultiMon
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_FullWidthTaskBar
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_MiddleClick
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_MiddleClickNewWindow
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_MiddleClickCloseWindow
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Taskbar_EnableThumbnails
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_LoggingLevel
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_Shell
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_CairoIsShell
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_ExplorerIsShell
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_SetCairoAsShell
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_SetExplorerAsShell
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_ShellChanged
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_ShellChangedText
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_LogOffNow
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_LogOffLater
        {
            get
            {
                return getString();
            }
        }

        public static string sSettings_Advanced_OpenLogsFolder
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_StartTour
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_FinishTour
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_Welcome
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_SelectLanguage
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_ChangingLanguage
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_ChangingLanguageText
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBar
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_DynamicDesktop
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_Taskbar
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarText
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec1Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec1Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec2Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec2Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec3Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec3Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec4Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec4Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec5Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec5Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec6Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_MenuBarSec6Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_DesktopText
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_DesktopSec1Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_DesktopSec1Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_DesktopSec2Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_DesktopSec2Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_TaskbarText
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_TaskbarSec1Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_TaskbarSec1Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_TaskbarSec2Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_TaskbarSec2Text
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_TaskbarSec3Heading
        {
            get
            {
                return getString();
            }
        }

        public static string sWelcome_TaskbarSec3Text
        {
            get
            {
                return getString();
            }
        }
    }
}