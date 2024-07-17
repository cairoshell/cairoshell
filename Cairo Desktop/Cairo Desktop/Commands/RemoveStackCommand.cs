using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Infrastructure.ObjectModel;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using ManagedShell.ShellFolders;
using System.Collections.Generic;
using System.Linq;

namespace CairoDesktop.Commands
{
    public class RemoveStackCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly RemoveStackCommandInfo _info = new RemoveStackCommandInfo();

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

            StacksManager.Instance.RemoveLocation(path);

            return true;
        }
    }

    public class RemoveStackCommandInfo : ICairoShellItemCommandInfo, ICairoShellFolderCommandInfo
    {
        public string Identifier => "RemoveStack";

        public string Description => "Removes the specified folder as a stack.";

        public string Label => DisplayString.sInterface_RemoveFromStacks;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "Location to remove as a stack",
                IsRequired = true
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;

        public string LabelForShellFolder(ShellFolder folder)
        {
            return Label;
        }

        public string LabelForShellItem(ShellItem item)
        {
            return Label;
        }

        public bool IsAvailableForShellItem(ShellItem item)
        {
            if (!item.IsNavigableFolder)
            {
                return false;
            }
            return StacksManager.Instance.StackLocations.Any(location => location.Path == item.Path);
        }

        public bool IsAvailableForShellFolder(ShellFolder folder)
        {
            return StacksManager.Instance.StackLocations.Any(location => location.Path == folder.Path);
        }
    }
}
