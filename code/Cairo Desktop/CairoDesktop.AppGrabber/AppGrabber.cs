using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;

namespace CairoDesktop.AppGrabber
{

    public class AppGrabber : DependencyObject
    {
        private static DependencyProperty programsListProperty = DependencyProperty.Register("ProgramsList", typeof(List<ApplicationInfo>), typeof(AppGrabber), new PropertyMetadata(new List<ApplicationInfo>()));
        
        private static AppGrabber _instance = new AppGrabber();

        public static AppGrabber Instance
        {
            get { return _instance; }
        }

        public CategoryList CategoryList { get; set; }
        public List<ApplicationInfo> ProgramList
        { 
            get
            {
                var retObject = GetValue(programsListProperty) as List<ApplicationInfo>;
                if (retObject.Count == 0)
                {
                    this.ProgramList = GetApps();
                    retObject = GetValue(programsListProperty) as List<ApplicationInfo>;
                }

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

        List<String> executableExtensions = new List<string>();

        String[] searchLocations = {
                Interop.Shell.UsersProgramsPath,
                Interop.Shell.AllUsersProgramsPath,
                Interop.Shell.UsersStartMenuPath,
                Interop.Shell.AllUsersStartMenuPath
                /*
                 * Sam doesn't like Desktop apps being found
                 */
                //Interop.Shell.UsersDesktopPath,
                //Interop.Shell.AllUsersDesktopPath,
        };

        public AppGrabber()
            : this(null) {}

        public AppGrabber(String configFile)
        {
            executableExtensions.AddRange(new String[]{
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
            rval.Sort();
            
            return rval;
        }

        /*private List<ApplicationInfo> generateAppListRecursing(DirectoryInfo directory)
        {
            List<ApplicationInfo> rval = new List<ApplicationInfo>();

            foreach (DirectoryInfo subfolder in directory.GetDirectories())
            {
                rval.AddRange(generateAppListRecursing(subfolder));
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                ApplicationInfo ai = new ApplicationInfo();
                String ext = Path.GetExtension(file.FullName);

                if (executableExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        ai.Name = Path.GetFileNameWithoutExtension(file.FullName);
                        ai.Path = file.FullName;
                        string target = string.Empty;

                        if (file.Extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                        {
                            Interop.Shell.Link link = new Interop.Shell.Link(file.FullName);
                            target = link.Target;
                        }
                        else
                        {
                            target = file.FullName;
                        }

                        // remove items that we can't execute. also remove uninstallers
                        if (!executableExtensions.Contains(Path.GetExtension(target), StringComparer.OrdinalIgnoreCase) || ai.Name == "Uninstall" || ai.Name.StartsWith("Uninstall "))
                        {
                            System.Diagnostics.Debug.WriteLine("Not an app: " + file.Name + ": " + target);
                            continue;
                        }

                        ai.Icon = ai.GetAssociatedIcon();
                        rval.Add(ai);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error creating ApplicationInfo object in appgrabber. " + ex.Message);
                    }
                }
            }

            return rval;
        }*/

        private List<ApplicationInfo> generateAppList(string directory)
        {
            List<ApplicationInfo> rval = new List<ApplicationInfo>();

            foreach (string file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                ApplicationInfo ai = new ApplicationInfo();
                String ext = Path.GetExtension(file);

                if (executableExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        ai.Name = Path.GetFileNameWithoutExtension(file);
                        ai.Path = file;
                        string target = string.Empty;
                        string fileExt = Path.GetExtension(file);

                        if (fileExt.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                        {
                            Interop.Shell.Link link = new Interop.Shell.Link(file);
                            target = link.Target;
                        }
                        else
                        {
                            target = file;
                        }

                        // remove items that we can't execute. also remove uninstallers
                        if (!executableExtensions.Contains(Path.GetExtension(target), StringComparer.OrdinalIgnoreCase) || ai.Name == "Uninstall" || ai.Name.StartsWith("Uninstall "))
                        {
                            System.Diagnostics.Debug.WriteLine("Not an app: " + file + ": " + target);
                            continue;
                        }

                        ai.Icon = ai.GetAssociatedIcon();
                        rval.Add(ai);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error creating ApplicationInfo object in appgrabber. " + ex.Message);
                    }
                }
            }

            return rval;
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
                new AppGrabberUI(this).Show();
            } catch { }
        }
    }
}
