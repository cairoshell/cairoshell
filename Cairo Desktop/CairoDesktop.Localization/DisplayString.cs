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

        public static string sProgramsMenu => getString();

        public static string sPlacesMenu => getString();

        public static string sCairoMenu_AboutCairo => getString();

        public static string sCairoMenu_CheckForUpdates => getString();

        public static string sCairoMenu_CairoSettings => getString();

        public static string sCairoMenu_WindowsControlPanel => getString();

        public static string sCairoMenu_WindowsSettings => getString();

        public static string sCairoMenu_Run => getString();

        public static string sCairoMenu_TaskManager => getString();

        public static string sCairoMenu_ExitCairo => getString();

        public static string sCairoMenu_Hibernate => getString();

        public static string sCairoMenu_Sleep => getString();

        public static string sCairoMenu_Restart => getString();

        public static string sCairoMenu_ShutDown => getString();

        public static string sCairoMenu_Lock => getString();

        public static string sCairoMenu_LogOff => getString();

        public static string sAbout_Version => getString();

        public static string sAbout_PreRelease => getString();

        public static string sAbout_Copyright => getString();

        public static string sInterface_OK => getString();

        public static string sInterface_Cancel => getString();

        public static string sInterface_Continue => getString();

        public static string sInterface_Finish => getString();

        public static string sInterface_Yes => getString();

        public static string sInterface_No => getString();

        public static string sInterface_Open => getString();

        public static string sInterface_OpenWith => getString();

        public static string sInterface_OpenAsAdministrator => getString();

        public static string sInterface_RunAsUser => getString();

        public static string sInterface_AddToStacks => getString();

        public static string sInterface_RemoveFromStacks => getString();

        public static string sInterface_Copy => getString();

        public static string sInterface_Paste => getString();

        public static string sInterface_Delete => getString();

        public static string sInterface_Rename => getString();

        public static string sInterface_Properties => getString();

        public static string sInterface_Browse => getString();

        public static string sAppGrabber => getString();

        public static string sAppGrabber_PleaseWait => getString();

        public static string sAppGrabber_Page1Title => getString();

        public static string sAppGrabber_Page1Line1 => getString();

        public static string sAppGrabber_Page1Line2 => getString();

        public static string sAppGrabber_ProgramsMenuItems => getString();

        public static string sAppGrabber_InstalledApplications => getString();

        public static string sAppGrabber_Page2Title => getString();

        public static string sAppGrabber_Page2Line1 => getString();

        public static string sAppGrabber_Page2Line2 => getString();

        public static string sAppGrabber_Page2Directions => getString();

        public static string sAppGrabber_Page2Directions1 => getString();

        public static string sAppGrabber_Page2Directions2 => getString();

        public static string sAppGrabber_Page2Directions3 => getString();

        public static string sAppGrabber_Page2Directions4 => getString();

        public static string sAppGrabber_Page2Directions5 => getString();

        public static string sAppGrabber_Page2Directions6 => getString();

        public static string sAppGrabber_ProgramCategories => getString();

        public static string sAppGrabber_Hide => getString();

        public static string sAppGrabber_QuickLaunch => getString();

        public static string sAppGrabber_Uncategorized => getString();

        public static string sAppGrabber_All => getString();

        public static string sAppGrabber_Untitled => getString();

        public static string sAppGrabber_Category_Accessories => getString();

        public static string sAppGrabber_Category_Productivity => getString();

        public static string sAppGrabber_Category_Development => getString();

        public static string sAppGrabber_Category_Graphics => getString();

        public static string sAppGrabber_Category_Media => getString();

        public static string sAppGrabber_Category_Internet => getString();

        public static string sAppGrabber_Category_Games => getString();

        public static string sRun_Title => getString();

        public static string sRun_Info => getString();

        public static string sExitCairo_Title => getString();

        public static string sExitCairo_Info => getString();

        public static string sExitCairo_ExitCairo => getString();

        public static string sLogoff_Title => getString();

        public static string sLogoff_Info => getString();

        public static string sLogoff_Logoff => getString();

        public static string sRestart_Title => getString();

        public static string sRestart_Info => getString();

        public static string sRestart_Restart => getString();

        public static string sShutDown_Title => getString();

        public static string sShutDown_Info => getString();

        public static string sShutDown_ShutDown => getString();

        public static string sProgramsMenu_Empty => getString();

        public static string sProgramsMenu_UninstallAProgram => getString();

        public static string sProgramsMenu_RemoveFromMenu => getString();

        public static string sProgramsMenu_RemoveTitle => getString();

        public static string sProgramsMenu_RemoveInfo => getString();

        public static string sProgramsMenu_AlwaysAdminTitle => getString();

        public static string sProgramsMenu_AlwaysAdminInfo => getString();

        public static string sProgramsMenu_Remove => getString();

        public static string sProgramsMenu_UWPInfo => getString();

        public static string sProgramsMenu_AddToQuickLaunch => getString();

        public static string sProgramsMenu_ChangeCategory => getString();

        public static string sPlacesMenu_Documents => getString();

        public static string sPlacesMenu_Downloads => getString();

        public static string sPlacesMenu_Music => getString();

        public static string sPlacesMenu_Pictures => getString();

        public static string sPlacesMenu_Videos => getString();

        public static string sPlacesMenu_ThisPC => getString();

        public static string sPlacesMenu_ProgramFiles => getString();

        public static string sPlacesMenu_RecycleBin => getString();

        public static string sStacks_Tooltip => getString();

        public static string sStacks_OpenInNewWindow => getString();

        public static string sStacks_OpenOnDesktop => getString();

        public static string sStacks_Remove => getString();

        public static string sStacks_Empty => getString();

        public static string sMenuBar_OpenDateTimeSettings => getString();

        public static string sMenuBar_OpenActionCenter => getString();

        public static string sMenuBar_ToggleNotificationArea => getString();

        public static string sMenuBar_Volume => getString();

        public static string sMenuBar_OpenSoundSettings => getString();

        public static string sMenuBar_OpenVolumeMixer => getString();

        public static string sSearch_Title => getString();

        public static string sSearch_ViewAllResults => getString();

        public static string sSearch_LastModified => getString();

        public static string sSearch_Error => getString();

        public static string sDesktop_DisplaySettings => getString();

        public static string sDesktop_Personalize => getString();

        public static string sDesktop_DeleteTitle => getString();

        public static string sDesktop_DeleteInfo => getString();

        public static string sDesktop_BrowseTitle => getString();

        public static string sDesktop_CurrentFolder => getString();

        public static string sDesktop_Back => getString();

        public static string sDesktop_Forward => getString();

        public static string sDesktop_Browse => getString();

        public static string sDesktop_Home => getString();

        public static string sDesktop_Up => getString();

        public static string sDesktop_SetHome => getString();

        public static string sDesktop_ClearHistory => getString();

        public static string sDesktop_ResetPosition => getString();

        public static string sTaskbar_Empty => getString();

        public static string sTaskbar_Minimize => getString();

        public static string sTaskbar_Restore => getString();

        public static string sTaskbar_Move => getString();

        public static string sTaskbar_Size => getString();

        public static string sTaskbar_Maximize => getString();

        public static string sTaskbar_NewWindow => getString();

        public static string sTaskbar_Close => getString();

        public static string sTaskbar_TaskView => getString();

        public static string sTaskbar_TaskListToolTip => getString();

        public static string sTaskbar_DesktopOverlayToolTip => getString();

        public static string sError_OhNo => getString();

        public static string sError_FileNotFoundInfo => getString();

        public static string sError_CantOpenAppWiz => getString();

        public static string sSettings_General => getString();

        public static string sSettings_MenuBar => getString();

        public static string sSettings_Desktop => getString();

        public static string sSettings_Taskbar => getString();

        public static string sSettings_Appearance => getString();

        public static string sSettings_Advanced => getString();

        public static string sSettings_Behavior => getString();

        public static string sSettings_Default => getString();

        public static string sSettings_RestartCairo => getString();

        public static string sSettings_Restarting => getString();

        public static string sSettings_General_UpdateCheck => getString();

        public static string sSettings_General_RunAtLogOn => getString();

        public static string sSettings_General_Language => getString();

        public static string sSettings_General_Theme => getString();

        public static string sSettings_General_ThemeTooltip => getString();

        public static string sSettings_General_TimeFormat => getString();

        public static string sSettings_General_TimeFormatTooltip => getString();

        public static string sSettings_General_DateFormat => getString();

        public static string sSettings_General_DateFormatTooltip => getString();

        public static string sSettings_General_FilesAndFolders => getString();

        public static string sSettings_General_ShowFileExtensions => getString();

        public static string sSettings_General_ForceSoftwareRendering => getString();

        public static string sSettings_General_FileManager => getString();

        public static string sSettings_General_FoldersOpenDesktopOverlay => getString();

        public static string sSettings_MenuBar_DefaultProgramsCategory => getString();

        public static string sSettings_MenuBar_EnableMenuBarShadow => getString();

        public static string sSettings_MenuBar_NotificationArea => getString();

        public static string sSettings_MenuBar_EnableNotificationArea => getString();

        public static string sSettings_MenuBar_NotificationAreaError => getString();

        public static string sSettings_MenuBar_NotificationAreaTaskbarWarning => getString();

        public static string sSettings_MenuBar_ShowNotifyIcons => getString();

        public static string sSettings_MenuBar_ShowNotifyIconsCollapsed => getString();

        public static string sSettings_MenuBar_ShowNotifyIconsAlways => getString();

        public static string sSettings_MenuBar_CollapsibleIcons => getString();

        public static string sSettings_MenuBar_PinnedIcons => getString();

        public static string sSettings_MenuBar_NotificationAreaPinHelp => getString();

        public static string sSettings_MenuBar_EnableCairoMenuHotKey => getString();

        public static string sSettings_MenuBar_EnableMenuBarBlur => getString();

        public static string sSettings_MenuBar_EnableMenuBarMultiMon => getString();

        public static string sSettings_MenuBar_ShowHibernate => getString();

        public static string sSettings_MenuBar_ProgramsMenuLayout => getString();

        public static string sSettings_MenuBar_ProgramsMenuLayoutRight => getString();

        public static string sSettings_MenuBar_ProgramsMenuLayoutLeft => getString();

        public static string sSettings_MenuBar_MenuExtras => getString();

        public static string sSettings_MenuBar_EnableMenuExtraVolume => getString();

        public static string sSettings_MenuBar_EnableMenuExtraActionCenter => getString();

        public static string sSettings_MenuBar_EnableMenuExtraClock => getString();

        public static string sSettings_MenuBar_EnableMenuExtraSearch => getString();

        public static string sSettings_Desktop_EnableDesktop => getString();

        public static string sSettings_Desktop_DesktopHome => getString();

        public static string sSettings_Desktop_LabelPosition => getString();

        public static string sSettings_Desktop_LabelPositionRight => getString();

        public static string sSettings_Desktop_LabelPositionBottom => getString();

        public static string sSettings_Desktop_DynamicDesktop => getString();

        public static string sSettings_Desktop_EnableDynamicDesktop => getString();

        public static string sSettings_Desktop_EnableDesktopOverlayHotKey => getString();


        public static string sSettings_Desktop_DesktopBackgroundSettings => getString();

        public static string sSettings_Desktop_BackgroundType => getString();

        public static string sSettings_Desktop_BackgroundType_windowsDefaultBackground => getString();

        public static string sSettings_Desktop_BackgroundType_cairoImageWallpaper => getString();

        public static string sSettings_Desktop_BackgroundType_cairoVideoWallpaper => getString();

        public static string sSettings_Desktop_BackgroundType_bingWallpaper => getString();

        public static string sSettings_Desktop_Background_Path => getString();

        public static string sSettings_Desktop_Background_Style => getString();

        public static string sSettings_Desktop_BackgroundStyle_Tile => getString();

        public static string sSettings_Desktop_BackgroundStyle_Center => getString();

        public static string sSettings_Desktop_BackgroundStyle_Fit => getString();

        public static string sSettings_Desktop_BackgroundStyle_Fill => getString();

        public static string sSettings_Desktop_BackgroundStyle_Stretch => getString();

        public static string sSettings_Desktop_BackgroundStyle_Span => getString();

        public static string sSettings_IconSize => getString();

        public static string sSettings_IconSizeLarge => getString();

        public static string sSettings_IconSizeMedium => getString();

        public static string sSettings_IconSizeSmall => getString();

        public static string sSettings_Taskbar_EnableTaskbar => getString();

        public static string sSettings_Taskbar_TaskbarPosition => getString();

        public static string sSettings_Taskbar_PositionBottom => getString();

        public static string sSettings_Taskbar_PositionTop => getString();

        public static string sSettings_Taskbar_DisplayMode => getString();

        public static string sSettings_Taskbar_DisplayModeAppBar => getString();

        public static string sSettings_Taskbar_DisplayModeOverlap => getString();

        public static string sSettings_Taskbar_DisplayModeAutoHide => getString();

        public static string sSettings_Taskbar_EnableTaskbarMultiMon => getString();

        public static string sSettings_Taskbar_FullWidthTaskBar => getString();

        public static string sSettings_Taskbar_MiddleClick => getString();

        public static string sSettings_Taskbar_MiddleClickNewWindow => getString();

        public static string sSettings_Taskbar_MiddleClickCloseWindow => getString();

        public static string sSettings_Taskbar_EnableThumbnails => getString();

        public static string sSettings_Advanced_LoggingLevel => getString();

        public static string sSettings_Advanced_Shell => getString();

        public static string sSettings_Advanced_CairoIsShell => getString();

        public static string sSettings_Advanced_ExplorerIsShell => getString();

        public static string sSettings_Advanced_SetCairoAsShell => getString();

        public static string sSettings_Advanced_SetExplorerAsShell => getString();

        public static string sSettings_Advanced_ShellChanged => getString();

        public static string sSettings_Advanced_ShellChangedText => getString();

        public static string sSettings_Advanced_LogOffNow => getString();

        public static string sSettings_Advanced_LogOffLater => getString();

        public static string sSettings_Advanced_OpenLogsFolder => getString();

        public static string sWelcome_StartTour => getString();

        public static string sWelcome_FinishTour => getString();

        public static string sWelcome_Welcome => getString();

        public static string sWelcome_SelectLanguage => getString();

        public static string sWelcome_ChangingLanguage => getString();

        public static string sWelcome_ChangingLanguageText => getString();

        public static string sWelcome_MenuBar => getString();

        public static string sWelcome_DynamicDesktop => getString();

        public static string sWelcome_Taskbar => getString();

        public static string sWelcome_MenuBarText => getString();

        public static string sWelcome_MenuBarSec1Heading => getString();

        public static string sWelcome_MenuBarSec1Text => getString();

        public static string sWelcome_MenuBarSec2Heading => getString();

        public static string sWelcome_MenuBarSec2Text => getString();

        public static string sWelcome_MenuBarSec3Heading => getString();

        public static string sWelcome_MenuBarSec3Text => getString();

        public static string sWelcome_MenuBarSec4Heading => getString();

        public static string sWelcome_MenuBarSec4Text => getString();

        public static string sWelcome_MenuBarSec5Heading => getString();

        public static string sWelcome_MenuBarSec5Text => getString();

        public static string sWelcome_MenuBarSec6Heading => getString();

        public static string sWelcome_MenuBarSec6Text => getString();

        public static string sWelcome_DesktopText => getString();

        public static string sWelcome_DesktopSec1Heading => getString();

        public static string sWelcome_DesktopSec1Text => getString();

        public static string sWelcome_DesktopSec2Heading => getString();

        public static string sWelcome_DesktopSec2Text => getString();

        public static string sWelcome_TaskbarText => getString();

        public static string sWelcome_TaskbarSec1Heading => getString();

        public static string sWelcome_TaskbarSec1Text => getString();

        public static string sWelcome_TaskbarSec2Heading => getString();

        public static string sWelcome_TaskbarSec2Text => getString();

        public static string sWelcome_TaskbarSec3Heading => getString();

        public static string sWelcome_TaskbarSec3Text => getString();
    }
}