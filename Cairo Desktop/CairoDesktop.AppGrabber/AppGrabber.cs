using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace CairoDesktop.AppGrabber
{
    public class AppGrabber : DependencyObject
    {
        private static DependencyProperty programsListProperty = DependencyProperty.Register("ProgramsList", typeof(List<ApplicationInfo>), typeof(AppGrabber), new PropertyMetadata(new List<ApplicationInfo>()));
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
                Shell.UsersStartMenuPath,
                Shell.AllUsersStartMenuPath
        };

        public static AppGrabber Instance { get; } = new AppGrabber();

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
                    Dispatcher.Invoke((Action)(() => this.ProgramList = value), null);
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

        public bool hasNewApps = false;

        public string ConfigFile { get; set; }

        private AppGrabber()
            : this(null) { }

        private AppGrabber(string configFile)
        {
            this.ConfigFile = configFile ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoAppConfig.xml";

            this.Load();
            this.NewApps = new List<ApplicationInfo>();
        }

        public void Load()
        {
            if (Shell.Exists(ConfigFile))
            {
                this.CategoryList = CategoryList.Deserialize(ConfigFile);
            }
            else
            {
                // config file not initialized, run first start logic
                this.CategoryList = new CategoryList(true);

                getPinnedApps();
            }
        }

        public void Save()
        {
            this.CategoryList.Serialize(ConfigFile);
        }

        private void getPinnedApps()
        {
            // add Windows taskbar pinned apps to QuickLaunch
            string pinnedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar";

            if (Shell.Exists(pinnedPath))
                QuickLaunch.AddRange(generateAppList(pinnedPath));
        }

        private static List<ApplicationInfo> GetApps()
        {
            List<List<ApplicationInfo>> listsToMerge = new List<List<ApplicationInfo>>();
            foreach (String location in searchLocations)
            {
                listsToMerge.Add(generateAppList(location));
            }
            List<ApplicationInfo> rval = mergeLists(listsToMerge);

            if (Shell.IsWindows8OrBetter)
                rval.AddRange(getStoreApps());
            return rval;
        }

        private static List<ApplicationInfo> getStoreApps()
        {
            List<ApplicationInfo> storeApps = new List<ApplicationInfo>();

            foreach (string[] app in UWPInterop.StoreAppHelper.GetStoreApps())
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

        private static List<ApplicationInfo> generateAppList(string directory)
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
                    ApplicationInfo app = PathToApp(file, false);
                    if (!ReferenceEquals(app, null))
                        rval.Add(app);
                }
            }
            catch { }

            return rval;
        }

        public static ApplicationInfo PathToApp(string file, bool allowNonApps)
        {
            ApplicationInfo ai = new ApplicationInfo();
            string fileExt = Path.GetExtension(file);

            if (allowNonApps || ExecutableExtensions.Contains(fileExt, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    ai.Name = Shell.GetDisplayName(file);
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
                            CairoLogger.Instance.Debug("Not an app: " + file + ": " + target);
                            return null;
                        }

                        // remove things that aren't apps (help, uninstallers, etc)
                        foreach (string word in excludedNames)
                        {
                            if (ai.Name.ToLower().Contains(word))
                            {
                                CairoLogger.Instance.Debug("Excluded item: " + file + ": " + target);
                                return null;
                            }
                        }
                    }

                    return ai;
                }
                catch (Exception ex)
                {
                    CairoLogger.Instance.Error("Error creating ApplicationInfo object in appgrabber. " + ex.Message, ex);
                    return null;
                }
            }

            return null;
        }

        private static List<ApplicationInfo> mergeLists(List<ApplicationInfo> a, List<ApplicationInfo> b)
        {
            List<ApplicationInfo> rval = new List<ApplicationInfo>(a.Count);
            rval.AddRange(a);
            foreach (ApplicationInfo ai in b)
            {
                if (!rval.Contains(ai))
                {
                    rval.Add(ai);
                }
            }
            return rval;
        }

        private static List<ApplicationInfo> mergeLists(List<List<ApplicationInfo>> listOfApplicationLists)
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
            catch { }
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
                    Shell.StartProcess(app.Path, "", "runas");
                }
                else if (!Shell.StartProcess(app.Path))
                {
                    CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void LaunchProgramVerb(ApplicationInfo app, string verb)
        {
            if (app != null)
            {
                if (!Shell.StartProcess(app.Path, "", verb))
                {
                    CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
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

                            bool? always = CairoMessage.Show(String.Format(Localization.DisplayString.sProgramsMenu_AlwaysAdminInfo, app.Name), Localization.DisplayString.sProgramsMenu_AlwaysAdminTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (always == true)
                                app.AlwaysAdmin = true;
                        }
                        else
                            app.AskAlwaysAdmin = true;

                        Save();
                    }

                    Shell.StartProcess(app.Path, "", "runas");
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
                bool? deleteChoice = CairoMessage.ShowOkCancel(String.Format(Localization.DisplayString.sProgramsMenu_RemoveInfo, app.Name, menu), Localization.DisplayString.sProgramsMenu_RemoveTitle, "Resources/cairoIcon.png", Localization.DisplayString.sProgramsMenu_Remove, Localization.DisplayString.sInterface_Cancel);
                if (deleteChoice.HasValue && deleteChoice.Value)
                {
                    app.Category.Remove(app);
                    Save();
                }
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

        public static void ShowAppProperties(ApplicationInfo app)
        {
            if (app != null)
            {
                if (app.IsStoreApp)
                    CairoMessage.ShowAlert(Localization.DisplayString.sProgramsMenu_UWPInfo, app.Name, MessageBoxImage.None);
                else
                    Shell.ShowFileProperties(app.Path);
            }
        }

        public void InsertByPath(string[] fileNames, int index, AppCategoryType categoryType)
        {
            int count = 0;
            foreach (string fileName in fileNames)
            {
                if (Shell.Exists(fileName))
                {
                    ApplicationInfo customApp = PathToApp(fileName, false);
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
                                CairoLogger.Instance.Debug("Excluded duplicate item: " + customApp.Name + ": " + customApp.Target);
                                continue;
                            }
                        }
                        else
                        {
                            category = CategoryList.GetSpecialCategory(categoryType);
                            if (category.Contains(customApp))
                            {
                                // disallow duplicates within the category
                                CairoLogger.Instance.Debug("Excluded duplicate item: " + customApp.Name + ": " + customApp.Target);
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
            foreach (string[] app in UWPInterop.StoreAppHelper.GetStoreApps())
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
    }
}
