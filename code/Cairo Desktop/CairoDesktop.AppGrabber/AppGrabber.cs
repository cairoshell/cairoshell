using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CairoDesktop;
using CairoDesktop.Interop;
using System.Xml.Serialization;
using System.Linq;

namespace CairoDesktop.AppGrabber
{

    public class AppGrabber : DependencyObject
    {
        //private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static DependencyProperty programsListProperty = DependencyProperty.Register("ProgramsList", typeof(List<ApplicationInfo>), typeof(AppGrabber), new PropertyMetadata(new List<ApplicationInfo>()));
        
        private static AppGrabber _instance = new AppGrabber();
        //private List<ApplicationInfo> _programsList;

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
                return this.CategoryList.GetCategory("Quick Launch") ?? new Category("Quick Launch");
            }
        }

        public bool hasNewApps = false;

        // TODO: Need to handle the setter so we can re-load the config file...
        public String ConfigFile { get; set; }

        List<String> executableExtensions = new List<string>();

        String[] searchLocations = {
                Interop.Shell.UsersProgramsPath,
                Interop.Shell.AllUsersProgramsPath,
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
                ".msc"
            });

            this.ConfigFile = configFile ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoAppConfig.xml";

            this.Load();
            //this.ProgramList = this.GetApps();
            this.NewApps = new List<ApplicationInfo>();
        }

        public void Load() {
            //_logger.Debug("Checking for category list config file: {0}", ConfigFile);
            if (Interop.Shell.Exists(ConfigFile)) {
                //_logger.Debug("Loading category list config file: {0}", ConfigFile);
                this.CategoryList = CategoryList.Deserialize(ConfigFile);
            } else {
                this.CategoryList = new CategoryList();
            }
        }

        public void Save() {
            //_logger.Debug("Saving out categories config file");
            this.CategoryList.Serialize(ConfigFile);
        }

        //public event EventHandler StateChanged;

        private List<ApplicationInfo> GetApps()
        {
            //_logger.Debug("Building index of applications.");
            List<List<ApplicationInfo>> listsToMerge = new List<List<ApplicationInfo>>();
            foreach (String location in searchLocations)
            {
                //_logger.Debug("Indexing {0} for applications", location);
                listsToMerge.Add(generateAppListRecursing(new DirectoryInfo(location)));
            }
            List<ApplicationInfo> rval = mergeLists(listsToMerge);
            rval.Sort();
            
            //_logger.Debug("Number of applications indexed: {0}", rval.Count);
            return rval;
        }

        private List<ApplicationInfo> generateAppListRecursing(DirectoryInfo directory)
        {
            //_logger.Debug("Scanning directory {0}", directory.FullName);
            List<ApplicationInfo> rval = new List<ApplicationInfo>();
            
            foreach (DirectoryInfo subfolder in directory.GetDirectories())
            {
                rval.AddRange(generateAppListRecursing(subfolder));
            }
            
            foreach (FileInfo file in directory.GetFiles()) 
            {
                //_logger.Debug("Interrogating file {0}", file.FullName);
                ApplicationInfo ai = new ApplicationInfo();
                String ext = Path.GetExtension(file.FullName);

                if (executableExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) {
                    try
                    {
                        ai.Name = Path.GetFileNameWithoutExtension(file.FullName);
                        ai.Path = file.FullName;
                        string target = string.Empty;

                        if (file.Extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                        {
                            //_logger.Debug("Attempting to interrogate shortcut to application: {0}", file.FullName);
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
                            System.Diagnostics.Debug.WriteLine(file.Name + ": " + target);
                            continue;
                        }

                        //_logger.Debug("Attempting to get associated icon for {0}", file.FullName);
                        ai.Icon = ai.GetAssociatedIcon();
                        rval.Add(ai);
                    }
                    catch
                    {
                        //Output the reason to the debugger
                        //_logger.Debug("Error creating ApplicationInfo object in appgrabber. Details: {0}\n{1}", ex.Message, ex.StackTrace);
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
                    //System.Diagnostics.Debug.WriteLine(ai);
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
                        //System.Diagnostics.Debug.WriteLine(ai);
                        rval.Add(ai);
                    }
                }
            }
            return rval;
        }

        public void ShowDialog() {
            try {
                new AppGrabberUI(this).ShowDialog();
            } catch { }
        }
    }
}
