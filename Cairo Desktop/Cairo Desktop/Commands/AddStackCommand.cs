using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.ShellFolders;
using System.Collections.Generic;
using System.Linq;

namespace CairoDesktop.Commands
{
    public class AddStackCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly AddStackCommandInfo _info = new AddStackCommandInfo();

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

            return StacksManager.Instance.AddLocation(path);
        }
    }

    public class AddStackCommandInfo : ICairoShellItemCommandInfo, ICairoShellFolderCommandInfo
    {
        public string Identifier => "AddStack";

        public string Description => "Adds the specified folder as a stack.";

        public string Label => DisplayString.sInterface_AddToStacks;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "Location to add as a stack",
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
            return !StacksManager.Instance.StackLocations.Any(location => location.Path == item.Path);
        }

        public bool IsAvailableForShellFolder(ShellFolder folder)
        {
            return !StacksManager.Instance.StackLocations.Any(location => location.Path == folder.Path);
        }
    }
}
