using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace CairoDesktop.Commands
{
    public class OpenFileCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly OpenFileCommandInfo _info = new OpenFileCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            string path = "";
            string args = "";
            string verb = "";

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
                    case "Arguments":
                        if (parameter.value is string _args)
                        {
                            args = _args;
                        }
                        break;
                    case "Verb":
                        if (parameter.value is string _verb)
                        {
                            verb = _verb;
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return ShellHelper.StartProcess(path, args, verb, getParent(path));
        }

        private string getParent(string fileName)
        {
            string parent = null;

            try
            {
                parent = Directory.GetParent(fileName).FullName;
            }
            catch (Exception e)
            {
                ShellLogger.Warning($"OpenFileCommand: Unable to get parent folder for {fileName}: {e.Message}");
            }

            return parent;
        }
    }

    public class OpenFileCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenFile";

        public string Description => "Opens the specified file.";

        public string Label => DisplayString.sCommand_OpenFile;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "File path to open",
                IsRequired = true
            },
            new CairoCommandParameter {
                Name = "Arguments",
                Description = "String of arguments to pass to the provided path.",
                IsRequired = false
            },
            new CairoCommandParameter {
                Name = "Verb",
                Description = "String of shell verb to execute for the provided path.",
                IsRequired = false
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;
    }
}
