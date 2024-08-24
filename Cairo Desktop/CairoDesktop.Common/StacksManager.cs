using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Common.SupportingClasses;
using ManagedShell.Interop;
using ManagedShell.ShellFolders;
using static ManagedShell.Interop.NativeMethods;

namespace CairoDesktop.Common
{
    public class StacksManager : DependencyObject
    {
        const int DBT_DEVICEARRIVAL = 0x8000;
        const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        const int DBT_DEVTYP_VOLUME = 0x2;

        private readonly string stackConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoStacksConfig.xml";
        private System.Xml.Serialization.XmlSerializer serializer;
        private NativeWindowEx _messageWindow;

        private InvokingObservableCollection<ShellFolder> _stackLocations = new InvokingObservableCollection<ShellFolder>(Application.Current.Dispatcher);
        private InvokingObservableCollection<ShellFolder> _removableDrives = new InvokingObservableCollection<ShellFolder>(Application.Current.Dispatcher);

        public InvokingObservableCollection<ShellFolder> StackLocations
        {
            get => _stackLocations;
            set
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke((Action)(() => StackLocations = value), null);
                    return;
                }

                _stackLocations = value;
            }
        }

        public InvokingObservableCollection<ShellFolder> RemovableDrives
        {
            get => _removableDrives;
            set
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke((Action)(() => RemovableDrives = value), null);
                    return;
                }

                _removableDrives = value;
            }
        }

        private static readonly StacksManager _instance = new StacksManager();
        public static StacksManager Instance => _instance;

        public StacksManager()
        {
            initialize();
        }

        private void initialize()
        {
            // this causes an exception, thanks MS!
            //serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<String>));

            serializer = System.Xml.Serialization.XmlSerializer.FromTypes(new[] { typeof(List<string>) })[0];

            try
            {
                deserializeStacks();
            }
            catch (Exception e)
            {
                ShellLogger.Error($"StacksManager: Unable to deserialize stacks config: {e.Message}");
            }

            StackLocations.CollectionChanged += stackLocations_CollectionChanged;
            Settings.Instance.PropertyChanged += Settings_PropertyChanged;

            if (Settings.Instance.ShowStacksRemovableDrives)
            {
                setupRemovableDrives();
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowStacksRemovableDrives")
            {
                if (Settings.Instance.ShowStacksRemovableDrives)
                {
                    setupRemovableDrives();
                }
                else
                {
                    clearRemovableDrives();
                }
            }
        }

        private void setupRemovableDrives()
        {
            if (_messageWindow == null)
            {
                try
                {
                    // create window to receive device change events
                    _messageWindow = new NativeWindowEx();
                    _messageWindow.CreateHandle(new System.Windows.Forms.CreateParams());
                    _messageWindow.MessageReceived += MessageWindowProc;
                }
                catch (Exception e)
                {
                    ShellLogger.Warning($"StacksManager: Unable to create message window: {e.Message}");
                }
            }

            // Get the initial set of drives
            populateRemovableDrives();
        }

        private void clearRemovableDrives()
        {
            if (_messageWindow != null)
            {
                try
                {
                    _messageWindow.DestroyHandle();
                    _messageWindow = null;
                }
                catch (Exception e)
                {
                    ShellLogger.Warning($"StacksManager: Unable to destroy message window: {e.Message}");
                }
            }

            RemovableDrives.Clear();
        }

        private void MessageWindowProc(System.Windows.Forms.Message msg)
        {
            // Update our drive list when a volume is added or removed

            if ((WM)msg.Msg != WM.DEVICECHANGE)
            {
                return;
            }

            if (msg.WParam != (IntPtr)DBT_DEVICEARRIVAL && msg.WParam != (IntPtr)DBT_DEVICEREMOVECOMPLETE)
            {
                return;
            }

            if (Marshal.ReadInt32(msg.LParam, 4) != DBT_DEVTYP_VOLUME)
            {
                return;
            }

            populateRemovableDrives();
        }

        private void populateRemovableDrives()
        {
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                List<DriveInfo> drivesToAdd = new List<DriveInfo>();
                List<ShellFolder> drivesToRemove = new List<ShellFolder>();

                foreach (DriveInfo drive in drives)
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Removable)
                    {
                        drivesToAdd.Add(drive);
                    }
                }

                foreach (ShellFolder drive in RemovableDrives)
                {
                    int driveIndex = drivesToAdd.FindIndex(d => d.Name == drive.Path);
                    if (driveIndex < 0)
                    {
                        drivesToRemove.Add(drive);
                    }
                    else
                    {
                        drivesToAdd.RemoveAt(driveIndex);
                    }
                }

                foreach (DriveInfo drive in drivesToAdd)
                {
                    // TODO: change watcher disabled so as to not prevent device removal
                    // Need to use RegisterDeviceNotification to receive DBT_DEVICEQUERYREMOVE
                    // and stop change watchers, then resume after DBT_DEVICEQUERYREMOVEFAILED
                    // or DBT_DEVICEREMOVECOMPLETE. ManagedShell needs to expose this
                    RemovableDrives.Add(new ShellFolder(drive.Name, IntPtr.Zero, true, false));
                }

                foreach (ShellFolder drive in drivesToRemove)
                {
                    RemovableDrives.Remove(drive);
                    drive.Dispose();
                }
            }
            catch (Exception e)
            {
                ShellLogger.Warning($"StacksManager: Error populating removable drives: {e.Message}");
            }
        }

        private void stackLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            serializeStacks();
        }

        public bool AddLocation(string path)
        {
            if (CanAdd(path))
            {
                ShellFolder dir = new ShellFolder(path, IntPtr.Zero, true);

                if (dir.IsNavigableFolder)
                {
                    StackLocations.Add(dir);
                    return true;
                }

                dir.Dispose();
            }

            return false;
        }

        public bool CanAdd(string path)
        {
            return !string.IsNullOrEmpty(path) && StackLocations.All(i => i.Path != path);
        }

        public void RemoveLocation(ShellFolder directory)
        {
            directory.Dispose();
            StackLocations.Remove(directory);
        }

        public void RemoveLocation(string path)
        {
            for (int i = 0; i < StackLocations.Count; i++)
            {
                if (StackLocations[i].Path == path)
                {
                    StackLocations.RemoveAt(i);
                    break;
                }
            }
        }

        private void serializeStacks()
        {
            List<string> locationPaths = new List<string>();
            foreach (ShellFolder dir in StackLocations)
            {
                locationPaths.Add(dir.Path);
            }
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    "
            };
            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stackConfigFile, settings);
            serializer.Serialize(writer, locationPaths);
            writer.Close();
        }

        private void deserializeStacks()
        {
            if (ShellHelper.Exists(stackConfigFile))
            {
                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stackConfigFile);

                if (serializer.Deserialize(reader) is List<string> locationPaths)
                {
                    foreach (string path in locationPaths)
                    {
                        AddLocation(path);
                    }
                }

                reader.Close();
            }
            else
            {
                // Add some default folders on first run
                AddLocation(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.None));
                AddLocation(KnownFolders.GetPath(KnownFolder.Downloads, NativeMethods.KnownFolderFlags.None));

                // Save
                serializeStacks();
            }
        }
    }
}
