using CairoDesktop.Common;
using CairoDesktop.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ManagedShell.Common.Enums;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;

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
            List<ApplicationInfo> rval = mergeLists(listsToMerge);

            if (EnvironmentHelper.IsWindows8OrBetter)
                rval.AddRange(getStoreApps());
            return rval;
        }

        private List<ApplicationInfo> getStoreApps()
        {
            List<ApplicationInfo> storeApps = new List<ApplicationInfo>();

            foreach (string[] app in ManagedShell.UWPInterop.StoreAppHelper.GetStoreApps())
            {
                string path = app[0];

                ApplicationInfo ai = new ApplicationInfo();
                ai.Name = app[1];
                ai.Path = "appx:" + path;
                ai.Target = path;
                ai.IconPath = app[2];
                ai.IconColor = app[3];

                if (ai.Name != "")
                    storeApps.Add(ai);
            }

            return storeApps;
        }

        private List<ApplicationInfo> generateAppList(string directory)
        {
            List<ApplicationInfo> rval = new List<ApplicationInfo>();

            try
            {
                IEnumerable<string> subs = Directory.EnumerateDirectories(directory);

                foreach (string dir in subs)
                {
                    if (!dir.ToLower().EndsWith("\\startup"))
                        rval.AddRange(generateAppList(dir));
                }
            }
            catch { }

            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles(directory, "*");

                foreach (string file in files)
                {
                    ApplicationInfo app = PathToApp(file, false, false);
                    if (!ReferenceEquals(app, null))
                        rval.Add(app);
                }
            }
            catch { }

            return rval;
        }

        public static ApplicationInfo PathToApp(string file, bool allowNonApps, bool allowExcludedNames)
        {
            ApplicationInfo ai = new ApplicationInfo();
            string fileExt = Path.GetExtension(file);

            if (allowNonApps || ExecutableExtensions.Contains(fileExt, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    ai.Name = ShellHelper.GetDisplayName(file);
                    ai.Path = file;
                    string target = string.Empty;

                    if (fileExt.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        Shell.Link link = new Shell.Link(file);
                        target = link.Target;
                    }
                    else
                    {
                        target = file;
                    }

                    ai.Target = target;

                    // remove items that we can't execute.
                    if (!allowNonApps)
                    {
                        if (!string.IsNullOrEmpty(target) && !ExecutableExtensions.Contains(Path.GetExtension(target), StringComparer.OrdinalIgnoreCase))
                        {
                            ShellLogger.Debug("Not an app: " + file + ": " + target);
                            return null;
                        }

                        // remove things that aren't apps (help, uninstallers, etc)
                        if (!allowExcludedNames)
                        {
                            foreach (string word in excludedNames)
                            {
                                if (ai.Name.ToLower().Contains(word))
                                {
                                    ShellLogger.Debug("Excluded item: " + file + ": " + target);
                                    return null;
                                }
                            }
                        }
                    }

                    return ai;
                }
                catch (Exception ex)
                {
                    ShellLogger.Error("Error creating ApplicationInfo object in appgrabber. " + ex.Message, ex);
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
                ShellLogger.Error($"Error creating AppGrabberUI: {e.Message}", e);
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
                    // this opens My Computer
                    if (!ShellHelper.StartProcess(app.Path, "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"))
                    {
                        CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
                    }
                }
                else if (!ShellHelper.StartProcess(app.Path))
                {
                    CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
                }
            }
        }

        public void LaunchProgramVerb(ApplicationInfo app, string verb)
        {
            if (app != null)
            {
                if (!ShellHelper.StartProcess(app.Path, "", verb))
                {
                    CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
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

                            CairoMessage.Show(string.Format(Localization.DisplayString.sProgramsMenu_AlwaysAdminInfo, app.Name), 
                                Localization.DisplayString.sProgramsMenu_AlwaysAdminTitle, 
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

        public void RemoveAppConfirm(ApplicationInfo app)
        {
            if (app != null)
            {
                string menu;
                if (app.Category.Type == AppCategoryType.QuickLaunch)
                    menu = Localization.DisplayString.sAppGrabber_QuickLaunch;
                else
                    menu = Localization.DisplayString.sProgramsMenu;
                
                CairoMessage.ShowOkCancel(string.Format(Localization.DisplayString.sProgramsMenu_RemoveInfo, app.Name, menu), 
                    Localization.DisplayString.sProgramsMenu_RemoveTitle, CairoMessageImage.Warning, 
                    Localization.DisplayString.sProgramsMenu_Remove, Localization.DisplayString.sInterface_Cancel,
                    result =>
                    {
                        if (result == true)
                        {
                            app.Category.Remove(app);
                            Save();
                        }
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
                    CairoMessage.Show(Localization.DisplayString.sProgramsMenu_UWPInfo, app.Name, app.GetIconImageSource(IconSize.Jumbo, false), true);
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
                                ShellLogger.Debug("Excluded duplicate item: " + customApp.Name + ": " + customApp.Target);
                                continue;
                            }
                        }
                        else
                        {
                            category = CategoryList.GetSpecialCategory(categoryType);
                            if (category.Contains(customApp))
                            {
                                // disallow duplicates within the category
                                ShellLogger.Debug("Excluded duplicate item: " + customApp.Name + ": " + customApp.Target);
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
            bool success = false;
            // not great but gets the job done I suppose
            foreach (string[] app in ManagedShell.UWPInterop.StoreAppHelper.GetStoreApps())
            {
                if (app[0] == appUserModelId)
                {
                    // bringo
                    ApplicationInfo ai = new ApplicationInfo();
                    ai.Name = app[1];
                    ai.Path = "appx:" + appUserModelId;
                    ai.Target = appUserModelId;
                    ai.IconPath = app[2];
                    ai.IconColor = app[3];

                    // add it
                    if (!ReferenceEquals(ai, null))
                    {
                        CategoryList.GetSpecialCategory(categoryType).Add(ai);
                        success = true;
                    }

                    break;
                }
            }

            if (success)
                Save();
        }

        public void AddToQuickLaunch(ApplicationInfo app)
        {
            if (!QuickLaunch.Contains(app))
            {
                ApplicationInfo appClone = app.Clone();
                appClone.Icon = null;
                appClone.IconPath = null;

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
