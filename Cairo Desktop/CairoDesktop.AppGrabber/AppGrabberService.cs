using CairoDesktop.Common;
using CairoDesktop.Interop;
using CairoDesktop.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ManagedShell.Common.Enums;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.ShellFolders;
using ManagedShell.ShellFolders.Enums;

namespace CairoDesktop.AppGrabber
{
    public class AppGrabberService : DependencyObject, IDisposable
    {
        private static DependencyProperty programsListProperty = DependencyProperty.Register("ProgramsList", typeof(List<ApplicationInfo>), typeof(AppGrabberService), new PropertyMetadata(new List<ApplicationInfo>()));
        public static AppGrabberUI uiInstance;

        private static readonly string[] excludedNames = { "documentation", "help", "install", "more info", "read me", "read first", "readme", "remove", "setup", "what's new", "support", "on the web", "safe mode" };

        public static readonly string[] ExecutableExtensions = {
                ".exe",
                ".bat",
                ".com",
                ".lnk",
                ".msc",
                ".appref-ms",
                ".url"
            };

        private static readonly string[] searchLocations = {
                ShellHelper.UsersStartMenuPath,
                ShellHelper.AllUsersStartMenuPath
        };

        public CategoryList CategoryList { get; set; }
        public List<ApplicationInfo> ProgramList
        {
            get
            {
                this.ProgramList = GetApps();
                var retObject = GetValue(programsListProperty) as List<ApplicationInfo>;

                return retObject;
            }
            private set
            {
                if (Dispatcher.CheckAccess())
                {
                    SetValue(programsListProperty, value);
                }
                else
                {
                    Dispatcher.Invoke((Action)(() => ProgramList = value), null);
                }
            }
        }

        public List<ApplicationInfo> NewApps { get; private set; }

        private Category _quickLaunch = null;
        public Category QuickLaunch
        {
            get
            {
                if (_quickLaunch == null)
                    _quickLaunch = CategoryList.GetSpecialCategory(AppCategoryType.QuickLaunch);

                return _quickLaunch;
            }
        }

        public string ConfigFile { get; set; }

        public QuickLaunchManager QuickLaunchManager;

        public AppGrabberService()
            : this(null) { }

        public AppGrabberService(string configFile)
        {
            ConfigFile = configFile ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoAppConfig.xml";

            Load();
            NewApps = new List<ApplicationInfo>();
            QuickLaunchManager = new QuickLaunchManager(this);
        }

        public void Load()
        {
            if (ShellHelper.Exists(ConfigFile))
            {
                CategoryList = CategoryList.Deserialize(ConfigFile);
            }
            else
            {
                // config file not initialized, run first start logic
                CategoryList = new CategoryList(true);

                getPinnedApps();
            }
        }

        public void Save()
        {
            CategoryList.Serialize(ConfigFile);
        }

        private void getPinnedApps()
        {
            // add Windows taskbar pinned apps to QuickLaunch
            string pinnedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar";

            if (ShellHelper.Exists(pinnedPath))
                QuickLaunch.AddRange(generateAppList(pinnedPath));
        }

        private List<ApplicationInfo> GetApps()
        {
            List<List<ApplicationInfo>> listsToMerge = new List<List<ApplicationInfo>>();
            foreach (string location in searchLocations)
            {
                listsToMerge.Add(generateAppList(location));
            }
            
            listsToMerge.Add(getStoreApps());
            
            return mergeLists(listsToMerge);
        }

        private List<ApplicationInfo> getStoreApps()
        {
            List<ApplicationInfo> storeApps = new List<ApplicationInfo>();

            if (!EnvironmentHelper.IsWindows8OrBetter || EnvironmentHelper.IsServerCore)
            {
                // Package management components are not available before Windows 8 or on Server Core installations
                // Just return an empty list
                return storeApps;
            }

            // Fetch all store apps
            ManagedShell.UWPInterop.StoreAppHelper.AppList.FetchApps();

            foreach (ManagedShell.UWPInterop.StoreApp app in ManagedShell.UWPInterop.StoreAppHelper.AppList)
            {
                ApplicationInfo ai = ApplicationInfo.FromStoreApp(app);

                if (ai.Name != "")
                    storeApps.Add(ai);
            }

            return storeApps;
        }

        private List<ApplicationInfo> generateAppList(string directory)
        {
            List<ApplicationInfo> rval = new List<ApplicationInfo>();
            ShellFolder folder = null;

            try
            {
                folder = new ShellFolder(directory, IntPtr.Zero, false, false);

                foreach (var file in folder.Files)
                {
                    if (file.IsFolder)
                    {
                        if (!file.Path.ToLower().EndsWith("\\startup"))
                            rval.AddRange(generateAppList(file.Path));
                    }
                    else
                    {
                        ApplicationInfo app = PathToApp(file.Path, file.DisplayName, false, false);
                        if (!ReferenceEquals(app, null))
                            rval.Add(app);
                    }
                }
            }
            catch (Exception e)
            {
                ShellLogger.Warning($"AppGrabberService: Unable to enumerate files: {e.Message}");
            }
            finally
            {
                folder?.Dispose();
            }

            return rval;
        }

        public static ApplicationInfo PathToApp(string filePath, bool allowNonApps, bool allowExcludedNames)
        {
            return PathToApp(filePath, ShellHelper.GetDisplayName(filePath), allowNonApps, allowExcludedNames);
        }

        public static ApplicationInfo PathToApp(string filePath, string fileDisplayName, bool allowNonApps, bool allowExcludedNames)
        {
            ApplicationInfo ai = new ApplicationInfo();
            string fileExt = Path.GetExtension(filePath);

            if (allowNonApps || ExecutableExtensions.Contains(fileExt, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    ai.Name = fileDisplayName;
                    ai.Path = filePath;
                    string target;

                    if (fileExt.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        Link link = new Link(filePath);
                        target = link.Target;
                    }
                    else
                    {
                        target = filePath;
                    }

                    ai.Target = target;

                    // remove items that we can't execute.
                    if (!allowNonApps)
                    {
                        if (!string.IsNullOrEmpty(target) && !ExecutableExtensions.Contains(Path.GetExtension(target), StringComparer.OrdinalIgnoreCase))
                        {
                            ShellLogger.Debug($"AppGrabberService: Not an app: {filePath}: {target}");
                            return null;
                        }

                        // remove things that aren't apps (help, uninstallers, etc)
                        if (!allowExcludedNames)
                        {
                            foreach (string word in excludedNames)
                            {
                                if (ai.Name.ToLower().Contains(word))
                                {
                                    ShellLogger.Debug($"AppGrabberService: Excluded item: {filePath}: {target}");
                                    return null;
                                }
                            }
                        }
                    }

                    return ai;
                }
                catch (Exception ex)
                {
                    ShellLogger.Error($"AppGrabberService: Error creating ApplicationInfo object: {ex.Message}");
                    return null;
                }
            }

            return null;
        }

        private List<ApplicationInfo> mergeLists(List<List<ApplicationInfo>> listOfApplicationLists)
        {
            List<ApplicationInfo> rval = new List<ApplicationInfo>(listOfApplicationLists[0].Count);
            rval.AddRange(listOfApplicationLists[0]);
            for (int i = 1; i < listOfApplicationLists.Count; i++)
            {
                foreach (ApplicationInfo ai in listOfApplicationLists[i])
                {
                    if (!rval.Contains(ai))
                    {
                        rval.Add(ai);
                    }
                }
            }
            return rval;
        }

        public void ShowDialog()
        {
            try
            {
                if (uiInstance == null)
                {
                    uiInstance = new AppGrabberUI(this);
                    uiInstance.Show();
                }

                uiInstance.Activate();
            }
            catch (Exception e)
            {
                ShellLogger.Error($"AppGrabberService: Error creating AppGrabberUI: {e.Message}", e);
            }
        }

        /* Helper methods */
        public void LaunchProgram(ApplicationInfo app)
        {
            if (app != null)
            {
                // so that we only prompt to always run as admin if it's done consecutively
                if (app.AskAlwaysAdmin)
                {
                    app.AskAlwaysAdmin = false;
                    Save();
                }

                if (!app.IsStoreApp && app.AlwaysAdmin)
                {
                    ShellHelper.StartProcess(app.Path, "", "runas");
                }
                else if (EnvironmentHelper.IsAppRunningAsShell && app.Target.ToLower().EndsWith("explorer.exe"))
                {
                    // special case: if we are shell and launching explorer, give it a parameter so that it doesn't do shell things.
                    if (!ShellHelper.StartProcess(app.Path, ShellFolderPath.ComputerFolder.Value))
                    {
                        CairoMessage.Show(DisplayString.sError_FileNotFoundInfo, DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
                    }
                }
                else if (!ShellHelper.StartProcess(app.Path))
                {
                    CairoMessage.Show(DisplayString.sError_FileNotFoundInfo, DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
                }
            }
        }

        public void LaunchProgramVerb(ApplicationInfo app, string verb)
        {
            if (app != null)
            {
                if (!ShellHelper.StartProcess(app.Path, "", verb))
                {
                    CairoMessage.Show(DisplayString.sError_FileNotFoundInfo, DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
                }
            }
        }

        public void LaunchProgramAdmin(ApplicationInfo app)
        {
            if (app != null)
            {
                if (!app.IsStoreApp)
                {
                    if (!app.AlwaysAdmin)
                    {
                        if (app.AskAlwaysAdmin)
                        {
                            app.AskAlwaysAdmin = false;

                            CairoMessage.Show(string.Format(DisplayString.sProgramsMenu_AlwaysAdminInfo, app.Name), 
                                DisplayString.sProgramsMenu_AlwaysAdminTitle, 
                                MessageBoxButton.YesNo, CairoMessageImage.Information,
                                result =>
                                {
                                    if (result == true)
                                        app.AlwaysAdmin = true;
                                });
                        }
                        else
                            app.AskAlwaysAdmin = true;

                        Save();
                    }

                    ShellHelper.StartProcess(app.Path, "", "runas");
                }
                else
                    LaunchProgram(app);
            }
        }

        public void RemoveAppConfirm(ApplicationInfo app, CairoMessage.DialogResultDelegate resultCallback)
        {
            if (app != null)
            {
                string menu;
                if (app.Category.Type == AppCategoryType.QuickLaunch)
                    menu = DisplayString.sAppGrabber_QuickLaunch;
                else
                    menu = DisplayString.sProgramsMenu;
                
                CairoMessage.ShowOkCancel(string.Format(DisplayString.sProgramsMenu_RemoveInfo, app.Name, menu), 
                    DisplayString.sProgramsMenu_RemoveTitle, CairoMessageImage.Warning, 
                    DisplayString.sProgramsMenu_Remove, DisplayString.sInterface_Cancel,
                    result =>
                    {
                        if (result == true)
                        {
                            app.Category.Remove(app);
                            Save();
                        }

                        resultCallback?.Invoke(result);
                    });
            }
        }

        public void RenameAppDialog(ApplicationInfo app, CairoMessage.DialogResultDelegate resultCallback)
        {
            if (app != null)
            {
                Common.MessageControls.Input inputControl = new Common.MessageControls.Input();
                inputControl.Initialize(app.Name);

                CairoMessage.ShowControl(string.Format(DisplayString.sProgramsMenu_RenameAppInfo, app.Name),
                    string.Format(DisplayString.sProgramsMenu_RenameTitle, app.Name),
                    app.GetIconImageSource(app.IsStoreApp ? IconSize.Jumbo : IconSize.ExtraLarge),
                    app.IsStoreApp,
                    inputControl,
                    DisplayString.sInterface_Rename,
                    DisplayString.sInterface_Cancel,
                    (bool? result) => {
                        if (result == true)
                        {
                            Rename(app, inputControl.Text);
                        }

                        resultCallback?.Invoke(result);
                    });
            }
        }

        public void Rename(ApplicationInfo app, string newName)
        {
            if (app != null)
            {
                app.Name = newName;
                Save();

                CollectionViewSource.GetDefaultView(CategoryList.GetSpecialCategory(AppCategoryType.All)).Refresh();
                CollectionViewSource.GetDefaultView(app.Category).Refresh();
            }
        }

        public void ShowAppProperties(ApplicationInfo app)
        {
            if (app != null)
            {
                if (app.IsStoreApp)
                    CairoMessage.Show(DisplayString.sProgramsMenu_UWPInfo, app.Name, app.GetIconImageSource(IconSize.Jumbo), true);
                else
                    ShellHelper.ShowFileProperties(app.Path);
            }
        }

        public void InsertByPath(string[] fileNames, int index, AppCategoryType categoryType)
        {
            int count = 0;
            foreach (string fileName in fileNames)
            {
                if (ShellHelper.Exists(fileName))
                {
                    ApplicationInfo customApp = PathToApp(fileName, false, true);
                    if (!ReferenceEquals(customApp, null))
                    {
                        Category category;

                        if (categoryType == AppCategoryType.Uncategorized || categoryType == AppCategoryType.Standard)
                        {
                            // if type is standard, drop in uncategorized
                            category = CategoryList.GetSpecialCategory(AppCategoryType.Uncategorized);
                            if (CategoryList.FlatList.Contains(customApp))
                            {
                                // disallow duplicates within all programs menu categories
                                ShellLogger.Debug($"AppGrabberService: Excluded duplicate item: {customApp.Name}: {customApp.Target}");
                                continue;
                            }
                        }
                        else
                        {
                            category = CategoryList.GetSpecialCategory(categoryType);
                            if (category.Contains(customApp))
                            {
                                // disallow duplicates within the category
                                ShellLogger.Debug($"AppGrabberService: Excluded duplicate item: {customApp.Name}: {customApp.Target}");
                                continue;
                            }
                        }

                        if (index >= 0) category.Insert(index, customApp);
                        else category.Add(customApp);
                        count++;
                    }
                }
            }

            if (count > 0)
                Save();
        }

        public void AddByPath(string fileName, AppCategoryType categoryType)
        {
            InsertByPath(new string[] { fileName }, -1, categoryType);
        }

        public void AddByPath(string[] fileNames, AppCategoryType categoryType)
        {
            InsertByPath(fileNames, -1, categoryType);
        }

        public void AddStoreApp(string appUserModelId, AppCategoryType categoryType)
        {
            var storeApp = ManagedShell.UWPInterop.StoreAppHelper.AppList.GetAppByAumid(appUserModelId);

            if (storeApp == null)
            {
                return;
            }

            ApplicationInfo ai = ApplicationInfo.FromStoreApp(storeApp);

            // add it
            if (!ReferenceEquals(ai, null))
            {
                CategoryList.GetSpecialCategory(categoryType).Add(ai);
                Save();
            }
        }

        public void AddToQuickLaunch(ApplicationInfo app)
        {
            if (!QuickLaunch.Contains(app))
            {
                ApplicationInfo appClone = app.Clone();
                appClone.Icon = null;

                QuickLaunch.Add(appClone);

                Save();
            }
        }

        public void Dispose()
        {
            // no work to do here
        }
    }
}
