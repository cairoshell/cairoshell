using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;
using System.Diagnostics;

namespace CairoDesktop.AppGrabber
{

    public class AppGrabber : DependencyObject
    {
        private static DependencyProperty programsListProperty = DependencyProperty.Register("ProgramsList", typeof(List<ApplicationInfo>), typeof(AppGrabber), new PropertyMetadata(new List<ApplicationInfo>()));
        
        private static AppGrabber _instance = new AppGrabber();

        public static AppGrabberUI uiInstance;

        private static string[] excludedNames = { "documentation", "help", "install", "more info", "read me", "read first", "readme", "remove", "setup", "what's new", "support", "on the web" };

        public static AppGrabber Instance
        {
            get { return _instance; }
        }

        public CategoryList CategoryList { get; set; }
        public List<ApplicationInfo> ProgramList
        { 
            get
            {
                // always get updated list
                //var retObject = GetValue(programsListProperty) as List<ApplicationInfo>;
                //if (retObject.Count == 0)
                //{
                    this.ProgramList = GetApps();
                    var retObject = GetValue(programsListProperty) as List<ApplicationInfo>;
                //}

                return retObject;
            }
            private set
            {
                if(this.Dispatcher.CheckAccess())
                {
                    SetValue(programsListProperty, value);
                }
                else
                {
                    this.Dispatcher.Invoke((Action)(() => this.ProgramList = value), null);
                }
            }
        }
        public List<ApplicationInfo> NewApps { get; private set; }
        
        public Category QuickLaunch
        {
            get
            {
                Category quicklaunch = this.CategoryList.GetCategory("Quick Launch");
                if (quicklaunch == null)
                {
                    this.CategoryList.Add(new Category("Quick Launch"));
                    quicklaunch = this.CategoryList.GetCategory("Quick Launch");
                    quicklaunch.ShowInMenu = false;
                }
                return quicklaunch;
            }
        }

        public bool hasNewApps = false;

        // TODO: Need to handle the setter so we can re-load the config file...
        public String ConfigFile { get; set; }

        public List<String> ExecutableExtensions = new List<string>();

        String[] searchLocations = {
                Interop.Shell.UsersStartMenuPath,
                Interop.Shell.AllUsersStartMenuPath
        };

        public AppGrabber()
            : this(null) {}

        public AppGrabber(String configFile)
        {
            ExecutableExtensions.AddRange(new String[]{
                ".exe",
                ".bat",
                ".com",
                ".lnk",
                ".msc",
                ".appref-ms",
                ".url"
            });

            this.ConfigFile = configFile ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoAppConfig.xml";

            this.Load();
            this.NewApps = new List<ApplicationInfo>();
        }

        public void Load() {
            if (Interop.Shell.Exists(ConfigFile)) {
                this.CategoryList = CategoryList.Deserialize(ConfigFile);
            } else {
                this.CategoryList = new CategoryList();
            }
        }

        public void Save() {
            this.CategoryList.Serialize(ConfigFile);
        }

        private List<ApplicationInfo> GetApps()
        {
            List<List<ApplicationInfo>> listsToMerge = new List<List<ApplicationInfo>>();
            foreach (String location in searchLocations)
            {
                listsToMerge.Add(generateAppList(location));
            }
            List<ApplicationInfo> rval = mergeLists(listsToMerge);
            //rval.Sort();
            if(Environment.OSVersion.Version.Major > 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2))
                rval.AddRange(getStoreApps());
            return rval;
        }
        
        private List<ApplicationInfo> getStoreApps()
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

        private List<ApplicationInfo> generateAppList(string directory)
        {
            List<ApplicationInfo> rval = new List<ApplicationInfo>();

            try
            {
                IEnumerable<string> subs = Directory.EnumerateDirectories(directory);

                foreach (string dir in subs)
                {
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
                    if (!object.ReferenceEquals(app, null))
                        rval.Add(app);
                }
            }
            catch { }

            return rval;
        }

        public ApplicationInfo PathToApp(string file, bool allowNonApps)
        {
            ApplicationInfo ai = new ApplicationInfo();
            string fileExt = Path.GetExtension(file);

            if (allowNonApps || ExecutableExtensions.Contains(fileExt, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    ai.Name = Path.GetFileNameWithoutExtension(file);
                    ai.Path = file;
                    string target = string.Empty;

                    if (fileExt.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        Interop.Shell.Link link = new Interop.Shell.Link(file);
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
                        if (!String.IsNullOrEmpty(target) && !ExecutableExtensions.Contains(Path.GetExtension(target), StringComparer.OrdinalIgnoreCase))
                        {
                            Debug.WriteLine("Not an app: " + file + ": " + target);
                            return null;
                        }

                        // remove things that aren't apps (help, uninstallers, etc)
                        foreach (string word in excludedNames)
                        {
                            if (ai.Name.ToLower().Contains(word))
                            {
                                Debug.WriteLine("Excluded item: " + file + ": " + target);
                                return null;
                            }
                        }
                    }
                    
                    return ai;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error creating ApplicationInfo object in appgrabber. " + ex.Message);
                    return null;
                }
            }

            return null;
        }

        private List<ApplicationInfo> mergeLists(List<ApplicationInfo> a, List<ApplicationInfo> b)
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

        public void ShowDialog() {
            try {
                if (uiInstance == null)
                {
                    uiInstance = new AppGrabberUI(this);
                    uiInstance.Show();
                }

                uiInstance.Activate();
            } catch { }
        }
    }
}
