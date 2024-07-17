using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class ShowFilePropertiesCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly ShowFilePropertiesCommandInfo _info = new ShowFilePropertiesCommandInfo();

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

            return ShellHelper.ShowFileProperties(path);
        }
    }

    public class ShowFilePropertiesCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "ShowFileProperties";

        public string Description => "Opens the properties window for the given file path.";

        public string Label => DisplayString.sInterface_Properties;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "File path to show properties for",
                IsRequired = true
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;
    }
}
