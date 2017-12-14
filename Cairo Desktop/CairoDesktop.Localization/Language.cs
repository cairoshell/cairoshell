﻿using System.Collections.Generic;

namespace CairoDesktop.Localization
{
    static class Language
    {
        public static Dictionary<string, string> en_US = new Dictionary<string, string>
        {
            { "sProgramsMenu", "Programs" },
            { "sPlacesMenu", "Places" },
            { "sCairoMenu_AboutCairo", "About Cairo" },
            { "sCairoMenu_CheckForUpdates", "Check for updates" },
            { "sCairoMenu_CairoSettings", "Cairo Settings" },
            { "sCairoMenu_WindowsControlPanel", "Windows Control Panel" },
            { "sCairoMenu_WindowsSettings", "Windows Settings" },
            { "sCairoMenu_Run", "Run..." },
            { "sCairoMenu_TaskManager", "Task Manager" },
            { "sCairoMenu_ExitCairo", "Exit Cairo..." },
            { "sCairoMenu_Sleep", "Sleep" },
            { "sCairoMenu_Restart", "Restart..." },
            { "sCairoMenu_ShutDown", "Shut Down..." },
            { "sCairoMenu_LogOff", "Log Off..." },
            { "sAbout_Version", "Version" },
            { "sAbout_PreRelease", "Pre-release" },
            { "sAbout_Copyright", "Copyright © 2007-{0} Cairo Development Team and community contributors.  All rights reserved." },
            { "sInterface_OK", "OK" },
            { "sInterface_Cancel", "Cancel" },
            { "sInterface_Continue", "Continue" },
            { "sInterface_Finish", "Finish" },
            { "sInterface_Yes", "Yes" },
            { "sInterface_No", "No" },
            { "sInterface_Open", "Open" },
            { "sInterface_OpenWith", "Open with..." },
            { "sInterface_OpenAsAdministrator", "Open as administrator" },
            { "sInterface_AddToStacks", "Add to Stacks" },
            { "sInterface_Copy", "Copy" },
            { "sInterface_Paste", "Paste" },
            { "sInterface_Delete", "Delete" },
            { "sInterface_Rename", "Rename" },
            { "sInterface_Properties", "Properties" },
            { "sInterface_Browse", "Browse..." },
            { "sAppGrabber", "App Grabber" },
            { "sAppGrabber_PleaseWait", "Please wait while Cairo finds your apps..." },
            { "sAppGrabber_Page1Title", "Cairo has found the following applications installed on your computer." },
            { "sAppGrabber_Page1Line1", "Please drag applications you would like Cairo to store in your Programs menu to the left.  If you would like to keep applications out of this menu, please drag them to the right." },
            { "sAppGrabber_Page1Line2", "Cairo has pre-selected applications that might be useful for your Programs menu.  To add or remove applications from this menu at any time, run the App Grabber from the Cairo menu or simply drag items into the menu." },
            { "sAppGrabber_ProgramsMenuItems", "Programs Menu Items" },
            { "sAppGrabber_InstalledApplications", "Installed Applications" },
            { "sAppGrabber_Page2Title", "Categories" },
            { "sAppGrabber_Page2Line1", "Categories allow you to group applications by type so they are easier to find. Once you finish, you can add, remove, or edit categories and applications through the Programs menu itself. For convenience, you may also run the App Grabber from the Cairo menu." },
            { "sAppGrabber_Page2Directions", "Directions:" },
            { "sAppGrabber_Page2Directions1", "Drag and drop applications into the categories you desire." },
            { "sAppGrabber_Page2Directions2", "Click the plus button to add a new category." },
            { "sAppGrabber_Page2Directions3", "Click the X button on a category to delete it. (Applications in a deleted category will become uncategorized.)" },
            { "sAppGrabber_Page2Directions4", "Right-click a category to rename it." },
            { "sAppGrabber_Page2Directions5", "Click the \"Hide\" button to show only applications that need categorization." },
            { "sAppGrabber_Page2Directions6", "Items placed in the Quick Launch category will appear in the quick launch area of the Cairo taskbar. Items placed here will not be removed from the Programs menu.  To remove an item, drag it out from the Quick Launch category." },
            { "sAppGrabber_ProgramCategories", "Program Categories" },
            { "sAppGrabber_Hide", "Hide" },
            { "sAppGrabber_QuickLaunch", "Quick Launch" },
            { "sAppGrabber_Uncategorized", "Uncategorized" },
            { "sAppGrabber_All", "All" },
            { "sAppGrabber_Category_Accessories", "Accessories" },
            { "sAppGrabber_Category_Productivity", "Productivity" },
            { "sAppGrabber_Category_Development", "Development" },
            { "sAppGrabber_Category_Graphics", "Graphics" },
            { "sAppGrabber_Category_Media", "Media" },
            { "sAppGrabber_Category_Internet", "Internet" },
            { "sAppGrabber_Category_Games", "Games" },
            { "sRun_Title", "Run" },
            { "sRun_Info", "Type the name of a program, folder, document, or Internet resource, and Cairo will open it for you." },
            { "sExitCairo_Title", "Are you sure you want to exit Cairo?" },
            { "sExitCairo_Info", "You will need to reboot or use the start menu shortcut in order to run Cairo again." },
            { "sExitCairo_ExitCairo", "Exit Cairo" },
            { "sLogoff_Title", "Are you sure you want to log off now?" },
            { "sLogoff_Info", "You will lose all unsaved documents and be logged off." },
            { "sLogoff_Logoff", "Log Off" },
            { "sRestart_Title", "Are you sure you want to restart now?" },
            { "sRestart_Info", "You will lose all unsaved documents and your computer will restart." },
            { "sRestart_Restart", "Restart" },
            { "sShutDown_Title", "Are you sure you want to shut down now?" },
            { "sShutDown_Info", "You will lose all unsaved documents and your computer will turn off." },
            { "sShutDown_ShutDown", "Shut Down" },
            { "sProgramsMenu_Empty", "No programs have been selected yet. Open App Grabber to get started!" },
            { "sProgramsMenu_UninstallAProgram", "Uninstall a Program" },
            { "sProgramsMenu_RemoveFromMenu", "Remove from menu" },
            { "sProgramsMenu_RemoveTitle", "Remove this app from the menu?" },
            { "sProgramsMenu_RemoveInfo", "\"{0}\" will be removed from the Programs menu. This will not uninstall the program." },
            { "sProgramsMenu_AlwaysAdminTitle", "Always run as administrator?" },
            { "sProgramsMenu_AlwaysAdminInfo", "You've run \"{0}\" as an administrator before. Would you like Cairo to automatically run it as an administrator every time?" },
            { "sProgramsMenu_Remove", "Remove" },
            { "sProgramsMenu_UWPInfo", "This is a Universal Windows Platform (UWP) application." },
            { "sPlacesMenu_Documents", "Documents" },
            { "sPlacesMenu_Downloads", "Downloads" },
            { "sPlacesMenu_Music", "Music" },
            { "sPlacesMenu_Pictures", "Pictures" },
            { "sPlacesMenu_Videos", "Videos" },
            { "sPlacesMenu_ThisPC", "This PC" },
            { "sPlacesMenu_ProgramFiles", "Program Files" },
            { "sPlacesMenu_RecycleBin", "Recycle Bin" },
            { "sStacks_Tooltip", "Drag directories here for easy access." },
            { "sStacks_OpenInNewWindow", "Open in new window" },
            { "sStacks_Remove", "Remove" },
            { "sStacks_Empty", "This folder is empty." },
            { "sMenuBar_OpenDateTimeSettings", "Open Date & Time Settings" },
            { "sMenuBar_OpenActionCenter", "Open Action Center" },
            { "sMenuBar_ToggleNotificationArea", "Toggle notification area icons" },
            { "sSearch_Title", "Desktop Search" },
            { "sSearch_ViewAllResults", "View All Results" },
            { "sSearch_LastModified", "Last Modified: {0}" },
            { "sSearch_Error", "We were unable to open the search result." },
            { "sDesktop_Personalize", "Personalize" },
            { "sDesktop_DeleteTitle", "Are you sure you want to delete this?" },
            { "sDesktop_DeleteInfo", "\"{0}\" will be sent to the Recycle Bin." },
            { "sDesktop_BrowseTitle", "Select a folder to display as your desktop:" },
            { "sTaskbar_Empty", "Open windows to see them listed here." },
            { "sTaskbar_Minimize", "Minimize" },
            { "sTaskbar_Restore", "Restore" },
            { "sTaskbar_Close", "Close" },
            { "sTaskbar_TaskView", "Task View" },
            { "sError_OhNo", "Oops!" },
            { "sError_FileNotFoundInfo", "The file could not be found.  If you just removed this program, try removing it using the right-click menu to make the icon go away." },
            { "sSettings_General", "General" },
            { "sSettings_MenuBar", "Menu Bar" },
            { "sSettings_Desktop", "Desktop" },
            { "sSettings_Taskbar", "Taskbar" },
            { "sSettings_Appearance", "Appearance" },
            { "sSettings_Default", "Default" },
            { "sSettings_RestartCairo", "Restart Cairo to apply" },
            { "sSettings_Restarting", "Restarting..." },
            { "sSettings_General_UpdateCheck", "Automatically check for updates" },
            { "sSettings_General_Language", "Language:" },
            { "sSettings_General_Theme", "Theme:" },
            { "sSettings_General_ThemeTooltip", "Place themes into your Cairo installation directory." },
            { "sSettings_General_TimeFormat", "Time format:" },
            { "sSettings_General_TimeFormatTooltip", "Some values you can use here:&#x0a;d	Day of month&#x0a;ddd	Day of week&#x0a;h	Hour (12-hour)&#x0a;hh	Hour (12-hour, leading zero)&#x0a;H	Hour (24-hour)&#x0a;HH	Hour (24-hour, leading zero)&#x0a;m	Minutes&#x0a;mm	Minutes (leading zero)&#x0a;s	Seconds&#x0a;ss	Seconds (leading zero)&#x0a;tt	AM/PM if applicable" },
            { "sSettings_General_DateFormat", "Date format:" },
            { "sSettings_General_DateFormatTooltip", "Some values you can use here:&#x0a;d	Day of month&#x0a;dd	Day of month (leading zero)&#x0a;ddd	Weekday (abbreviated)&#x0a;dddd	Weekday (text)&#x0a;M	Month&#x0a;MM	Month (leading zero)&#x0a;MMM	Month (abbreviated)&#x0a;MMMM	Month (text)&#x0a;yy	Year (2-digit)&#x0a;yyyy	Year (4-digit)" },
            { "sSettings_General_FilesAndFolders", "Files and folders" },
            { "sSettings_General_ShowSubDirectories", "Show sub-directories" },
            { "sSettings_General_ShowFileExtensions", "Show file extensions" },
            { "sSettings_General_ForceSoftwareRendering", "Force software rendering (advanced)" },
            { "sSettings_General_FileManager", "File manager:" },
            { "sSettings_MenuBar_DefaultProgramsCategory", "Default programs category:" },
            { "sSettings_MenuBar_EnableMenuBarShadow", "Enable Menu Bar shadow" },
            { "sSettings_MenuBar_NotificationArea", "Notification area" },
            { "sSettings_MenuBar_EnableNotificationArea", "Enable notification area" },
            { "sSettings_MenuBar_NotificationAreaError", "The notification area component could not be loaded. Please install the Microsoft Visual C++ 2015 Redistributable." },
            { "sSettings_MenuBar_ShowNotifyIcons", "Show notification area icons:" },
            { "sSettings_MenuBar_ShowNotifyIconsCollapsed", "Collapsed" },
            { "sSettings_MenuBar_ShowNotifyIconsAlways", "Always" },
            { "sSettings_MenuBar_PeriodicallyRehook", "Periodically re-establish notification area hook" },
            { "sSettings_Desktop_EnableDesktop", "Enable Desktop" },
            { "sSettings_Desktop_DesktopHome", "Desktop home:" },
            { "sSettings_Desktop_IconSize", "Icon size:" },
            { "sSettings_Desktop_IconSizeLarge", "Large" },
            { "sSettings_Desktop_IconSizeSmall", "Small" },
            { "sSettings_Desktop_LabelPosition", "Label position:" },
            { "sSettings_Desktop_LabelPositionRight", "Right" },
            { "sSettings_Desktop_LabelPositionBottom", "Bottom" },
            { "sSettings_Desktop_DynamicDesktop", "Dynamic Desktop" },
            { "sSettings_Desktop_EnableDynamicDesktop", "Use Dynamic Desktop" },
            { "sSettings_Taskbar_EnableTaskbar", "Enable Taskbar" },
            { "sSettings_Taskbar_TaskbarPosition", "Taskbar position:" },
            { "sSettings_Taskbar_PositionBottom", "Bottom" },
            { "sSettings_Taskbar_PositionTop", "Top" },
            { "sSettings_Taskbar_DisplayMode", "Display mode:" },
            { "sSettings_Taskbar_DisplayModeAppBar", "Taskbar reserves space" },
            { "sSettings_Taskbar_DisplayModeOverlap", "Taskbar overlaps windows" }
        };

        public static Dictionary<string, string> fr_FR = new Dictionary<string, string>
        {
            { "sProgramsMenu", "Applications" },
            { "sPlacesMenu", "Dossiers" },
            { "sCairoMenu_AboutCairo", "À propos de Cairo" },
            { "sCairoMenu_CheckForUpdates", "Rechercher des mises à jour" },
            { "sCairoMenu_CairoSettings", "Paramètres Cairo" },
            { "sCairoMenu_WindowsControlPanel", "Panneau de configuration Windows" },
            { "sCairoMenu_WindowsSettings", "Paramètres Windows" },
            { "sCairoMenu_Run", "Exécuter..." },
            { "sCairoMenu_TaskManager", "Gestionnaire de tâches" },
            { "sCairoMenu_ExitCairo", "Quitter Cairo..." },
            { "sCairoMenu_Sleep", "Mettre en veille" },
            { "sCairoMenu_Restart", "Redémarrer..." },
            { "sCairoMenu_ShutDown", "Arrêter..." },
            { "sCairoMenu_LogOff", "Se déconnecter..." },
            { "sAbout_Version", "Version" },
            { "sAbout_PreRelease", "Pré-version" },
            { "sAbout_Copyright", "Copyright © 2007-{0} Équipe de développement Cairo et contributeurs de la communauté.  Tous droits réservés." },
            { "sInterface_OK", "OK" },
            { "sInterface_Cancel", "Annuler" },
            { "sInterface_Continue", "Continuer" },
            { "sInterface_Finish", "Terminer" },
            { "sInterface_Yes", "Oui" },
            { "sInterface_No", "Non" },
            { "sInterface_Open", "Ouvrir" },
            { "sInterface_OpenWith", "Ouvrir avec..." },
            { "sInterface_OpenAsAdministrator", "Exécuter en tant qu'administrateur" },
            { "sInterface_AddToStacks", "Ajouter aux piles" },
            { "sInterface_Copy", "Copier" },
            { "sInterface_Paste", "Coller" },
            { "sInterface_Delete", "Supprimer" },
            { "sInterface_Rename", "Renommer" },
            { "sInterface_Properties", "Propriétés" },
            { "sInterface_Browse", "Rechercher..." },
            { "sAppGrabber", "Sélecteur d'applications" },
            { "sAppGrabber_PleaseWait", "Veuillez patienter pendant que Cairo trouve vos applications..." },
            { "sAppGrabber_Page1Title", "Cairo a trouvé les applications suivantes installées sur votre ordinateur." },
            { "sAppGrabber_Page1Line1", "Veuillez glisser les applications que vous souhaitez que Cairo range dans le menu Applications à gauche.  Si vous souhaitez conserver les applications en dehors de ce menu, veuillez les glisser à droite." },
            { "sAppGrabber_Page1Line2", "Cairo a pré-sélectionné des applications qui pourraient être utiles pour votre menu Applications.  Pour ajouter ou supprimer des applications de ce menu à tout moment, lancez le sélecteur d'applications depuis le menu Cairo ou glissez simplement des éléments dans le menu." },
            { "sAppGrabber_ProgramsMenuItems", "Éléments du menu Applications" },
            { "sAppGrabber_InstalledApplications", "Applications installées" },
            { "sAppGrabber_Page2Title", "Catégories" },
            { "sAppGrabber_Page2Line1", "Les catégories vous permettent de grouper les applications par type afin qu'elles soient plus faciles à trouver. Une fois que vous avez terminé, vous pouvez ajouter, supprimer ou modifier les catégories et applications à travers le menu Applications lui-même. Pour plus de commodité, vous pouvez aussi lancer le sélecteur d'applications depuis le menu Cairo." },
            { "sAppGrabber_Page2Directions", "Instructions:" },
            { "sAppGrabber_Page2Directions1", "Glissez-déposez des applications dans les catégories que vous desirez." },
            { "sAppGrabber_Page2Directions2", "Cliquez sur le bouton plus pour ajouter une nouvelle catégorie." },
            { "sAppGrabber_Page2Directions3", "Cliquez le bouton X sur une catégorie pour la supprimer. (Les applications dans une catégorie supprimée deviendront non classées.)" },
            { "sAppGrabber_Page2Directions4", "Clic droit sur une catégorie pour la renommer." },
            { "sAppGrabber_Page2Directions5", "Cliquez sur le bouton \"Replier\" pour afficher uniquement les applications qui nécessitent d'être classées." },
            { "sAppGrabber_Page2Directions6", "Les éléments placés dans la catégorie Lancement rapide vont apparaître dans la zone de lancement rapide de la barre de tâches de Cairo. Les éléments placés ici ne seront pas supprimés du menu Applications.  Pour supprimer un élément, glissez-le en dehors de la catégorie Lancement rapide." },
            { "sAppGrabber_ProgramCategories", "Catégories d'applications" },
            { "sAppGrabber_Hide", "Replier" },
            { "sAppGrabber_QuickLaunch", "Lancement rapide" },
            { "sAppGrabber_Uncategorized", "Non classé" },
            { "sAppGrabber_All", "Tous" },
            { "sRun_Title", "Exécuter" },
            { "sRun_Info", "Tapez le nom d'une application, d'un dossier, d'un document ou d'une ressource Internet, et Cairo l'ouvrira pour vous." },
            { "sExitCairo_Title", "Êtes-vous sûr de vouloir quitter Cairo ?" },
            { "sExitCairo_Info", "Vous devrez redémarrer ou utiliser le raccourci du menu démarrer afin de lancer Cairo à nouveau." },
            { "sExitCairo_ExitCairo", "Quitter Cairo" },
            { "sLogoff_Title", "Êtes-vous sûr de vouloir vous déconnecter maintenant ?" },
            { "sLogoff_Info", "Vous allez perdre tous les documents non sauvegardés et être déconnecté." },
            { "sLogoff_Logoff", "Se déconnecter" },
            { "sRestart_Title", "Êtes-vous sûr de vouloir redémarrer maintenant ?" },
            { "sRestart_Info", "Vous allez perdre tous les documents non sauvegardés et votre ordinateur va redémarrer." },
            { "sRestart_Restart", "Redémarrer" },
            { "sShutDown_Title", "Êtes-vous sûr de vouloir arrêter l'ordinateur maintenant ?" },
            { "sShutDown_Info", "Vous allez perdre tous les documents non sauvegardés et votre ordinateur va s'arrêter." },
            { "sShutDown_ShutDown", "Arrêter" },
            { "sProgramsMenu_Empty", "Aucune application n'a encore été sélectionnée. Ouvrez le sélecteur d'applications pour commencer !" },
            { "sProgramsMenu_UninstallAProgram", "Désinstaller une application" },
            { "sProgramsMenu_RemoveFromMenu", "Supprimer du menu" },
            { "sProgramsMenu_RemoveTitle", "Supprimer cette application du menu ?" },
            { "sProgramsMenu_RemoveInfo", "\"{0}\" va être supprimé du menu Applications. Cela ne désinstallera pas l'application." },
            { "sProgramsMenu_AlwaysAdminTitle", "Toujours exécuter cette application en tant qu'administrateur ?" },
            { "sProgramsMenu_AlwaysAdminInfo", "Vous avez précédemment exécuté \"{0}\" en tant qu'administrateur. Souhaitez-vous que Cairo l'exécute automatiquement en tant qu'administrateur à chaque fois ?" },
            { "sProgramsMenu_Remove", "Supprimer" },
            { "sProgramsMenu_UWPInfo", "C'est une application universelle (UWP)." },
            { "sPlacesMenu_Documents", "Documents" },
            { "sPlacesMenu_Downloads", "Téléchargements" },
            { "sPlacesMenu_Music", "Musique" },
            { "sPlacesMenu_Pictures", "Images" },
            { "sPlacesMenu_Videos", "Vidéos" },
            { "sPlacesMenu_ThisPC", "Ce PC" },
            { "sPlacesMenu_ProgramFiles", "Program Files" },
            { "sPlacesMenu_RecycleBin", "Corbeille" },
            { "sStacks_Tooltip", "Glissez des répertoires ici pour un accès facile." },
            { "sStacks_OpenInNewWindow", "Ouvrir dans une nouvelle fenêtre" },
            { "sStacks_Remove", "Supprimer" },
            { "sStacks_Empty", "Ce répertoire est vide." },
            { "sMenuBar_OpenDateTimeSettings", "Ouvrir les paramètres de date et heure" },
            { "sMenuBar_OpenActionCenter", "Ouvrir le centre de notifications" },
            { "sMenuBar_ToggleNotificationArea", "Afficher/masquer les icônes de la zone de notification" },
            { "sSearch_Title", "Rechercher sur le bureau" },
            { "sSearch_ViewAllResults", "Voir tous les résultats" },
            { "sSearch_LastModified", "Modifié le : {0}" },
            { "sSearch_Error", "Nous n'avons pas pu ouvrir le résultat de la recherche." },
            { "sDesktop_Personalize", "Personnaliser" },
            { "sDesktop_DeleteTitle", "Êtes-vous sûr de vouloir supprimer ceci ?" },
            { "sDesktop_DeleteInfo", "\"{0}\" va être envoyé à la corbeille." },
            { "sDesktop_BrowseTitle", "Choisissez un dossier à afficher en tant que votre bureau :" },
            { "sTaskbar_Empty", "Ouvrez des fenêtres pour les voir listées ici." },
            { "sTaskbar_Minimize", "Réduire" },
            { "sTaskbar_Restore", "Agrandir" },
            { "sTaskbar_Close", "Fermer" },
            { "sTaskbar_TaskView", "Affichage des tâches" },
            { "sError_OhNo", "Oups !" },
            { "sError_FileNotFoundInfo", "Le fichier n'a pas pu être trouvé.  Si vous venez juste de supprimer ce programme, essayez de le supprimer en utilisant le menu clic droit pour faire disparaître l'icône." },
            { "sSettings_General", "Général" },
            { "sSettings_MenuBar", "Barre de menu" },
            { "sSettings_Desktop", "Bureau" },
            { "sSettings_Taskbar", "Barre des tâches" },
            { "sSettings_Appearance", "Apparence" },
            { "sSettings_Default", "Par défaut" },
            { "sSettings_RestartCairo", "Redémarrer Cairo pour appliquer" },
            { "sSettings_Restarting", "Redémarrage en cours..." },
            { "sSettings_General_UpdateCheck", "Rechercher automatiquement les mises à jour" },
            { "sSettings_General_Language", "Langue :" },
            { "sSettings_General_Theme", "Thème :" },
            { "sSettings_General_ThemeTooltip", "Placez les thèmes dans votre répertoire d'installation Cairo." },
            { "sSettings_General_TimeFormat", "Format de l'heure :" },
            { "sSettings_General_TimeFormatTooltip", "Voici quelques valeurs que vous pouvez utiliser ici :&#x0a;d	Jour du mois&#x0a;ddd	Jour de la semaine&#x0a;h	Heure (12 heures)&#x0a;hh	Heure (12 heures, précédée d'un zéro)&#x0a;H	Heure (24 jeures)&#x0a;HH	Heure (24 heures, précédée d'un zéro)&#x0a;m	Minutes&#x0a;mm	Minutes (précédées d'un zéro)&#x0a;s	Secondes&#x0a;ss	Secondes (précédées d'un zéro)&#x0a;tt	AM/PM le cas échéant" },
            { "sSettings_General_DateFormat", "Format de la date :" },
            { "sSettings_General_DateFormatTooltip", "Voici quelques valeurs que vous pouvez utiliser ici :&#x0a;d	Jour du mois&#x0a;dd	Jour du mois (précédé d'un zéro)&#x0a;ddd	Jour de la semaine (abrégé)&#x0a;dddd	Jour de la semaine (texte)&#x0a;M	Mois&#x0a;MM	Mois (précédé d'un zéro)&#x0a;MMM	Mois (abrégé)&#x0a;MMMM	Mois (texte)&#x0a;yy	Année (2 chiffres)&#x0a;yyyy	Année (4 chiffres)" },
            { "sSettings_General_FilesAndFolders", "Fichiers et dossiers" },
            { "sSettings_General_ShowSubDirectories", "Afficher les sous-répertoires" },
            { "sSettings_General_ShowFileExtensions", "Afficher les extensions des fichiers" },
            { "sSettings_General_ForceSoftwareRendering", "Forcer le rendu logiciel (avancé)" },
            { "sSettings_General_FileManager", "Gestionnaire de fichiers :" },
            { "sSettings_MenuBar_DefaultProgramsCategory", "Catégorie de programmes par défaut :" },
            { "sSettings_MenuBar_EnableMenuBarShadow", "Activer l'ombre de la barre de menu" },
            { "sSettings_MenuBar_NotificationArea", "Zone de notification" },
            { "sSettings_MenuBar_EnableNotificationArea", "Activer la zone de notification" },
            { "sSettings_MenuBar_NotificationAreaError", "Le composent de la zone de notification n'a pas pu être chargé. Veuillez installer le redistribuable Microsoft Visual C++ 2015." },
            { "sSettings_MenuBar_ShowNotifyIcons", "Afficher les icônes de la zone de notification :" },
            { "sSettings_MenuBar_ShowNotifyIconsCollapsed", "Repliées" },
            { "sSettings_MenuBar_ShowNotifyIconsAlways", "Toujours" },
            { "sSettings_MenuBar_PeriodicallyRehook", "Ré-établir périodiquement le hook de la zone de notification" },
            { "sSettings_Desktop_EnableDesktop", "Activer le bureau" },
            { "sSettings_Desktop_DesktopHome", "Accueil du bureau :" },
            { "sSettings_Desktop_IconSize", "Taille des icônes :" },
            { "sSettings_Desktop_IconSizeLarge", "Grand" },
            { "sSettings_Desktop_IconSizeSmall", "Petit" },
            { "sSettings_Desktop_LabelPosition", "Position de l'étiquette :" },
            { "sSettings_Desktop_LabelPositionRight", "Droite" },
            { "sSettings_Desktop_LabelPositionBottom", "Bas" },
            { "sSettings_Desktop_DynamicDesktop", "Bureau dynamique" },
            { "sSettings_Desktop_EnableDynamicDesktop", "Utiliser le bureau dynamique" },
            { "sSettings_Taskbar_EnableTaskbar", "Activer la barre des tâches" },
            { "sSettings_Taskbar_TaskbarPosition", "Position de la barre des tâches :" },
            { "sSettings_Taskbar_PositionBottom", "Bas" },
            { "sSettings_Taskbar_PositionTop", "Haut" },
            { "sSettings_Taskbar_DisplayMode", "Mode d'affichage :" },
            { "sSettings_Taskbar_DisplayModeAppBar", "La barre des tâches réserve l'espace" },
            { "sSettings_Taskbar_DisplayModeOverlap", "La barre des tâches chevauche les fenêtres" }
        };
    }
}
