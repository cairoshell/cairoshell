using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CairoDesktop.Commands
{
    public class CopyFileCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly CopyFileCommandInfo _info = new CopyFileCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            string path = "";

            foreach (var parameter in parameters)
            {
                switch (parameter.name)
                {
                    case "Path":
                        if (parameter.value is string _path)
                        {
                            path = _path;
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            System.Windows.Forms.Clipboard.SetFileDropList(new StringCollection { path });

            return true;
        }
    }

    public class CopyFileCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "CopyFile";

        public string Description => "Copies the specified file to the clipboard.";

        public string Label => DisplayString.sInterface_Copy;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "File path to copy",
                IsRequired = true
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;
    }
}
