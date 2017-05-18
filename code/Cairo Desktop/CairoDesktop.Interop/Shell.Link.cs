using System;
using System.IO;
using IWshRuntimeLibrary;
using io = System.IO;

namespace CairoDesktop.Interop {
    public partial class Shell {
        public class Link {

            public Link(string pathToLink) {
                if (string.IsNullOrEmpty(pathToLink))
                {
                    throw new ArgumentNullException("pathToLink");
                }

                Path = pathToLink;
            }

            private string path;
            public string Path {
                get { return path; }
                set {
                    if (!io.File.Exists(value)) {
                        throw new FileNotFoundException("File " + value + " does not exist.");
                    }
                    if (!value.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)) {
                        throw new FileLoadException("File " + value + " is not a .lnk file.");
                    }
                    path = value;
                    getLinkProperties();
                }
            }

            public string Target { get; private set; }
            public string Arguments { get; private set; }
            public string WorkingDirectory { get; private set; }
            public string IconFile { get; private set; }
            public int IconIndex { get; private set; }
                
            private void getLinkProperties()
            {
                if (string.IsNullOrEmpty(this.Path))
                {
                    throw new ApplicationException("Path is null or empty, please set this value before retrieving the properties.");
                }

                WshShell shell = new WshShell();
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(Path);
                Target = Environment.ExpandEnvironmentVariables(link.TargetPath);
                Arguments = link.Arguments;
                string[] iconParts = link.IconLocation.Split(',');
                if (iconParts[0] == "") {
                    IconFile = Target;
                } else {
                    IconFile = Environment.ExpandEnvironmentVariables(iconParts[0]);
                }
                int index = 0;
                int.TryParse(iconParts[1], out index);
                IconIndex = index;
            }
        }

    }
}
