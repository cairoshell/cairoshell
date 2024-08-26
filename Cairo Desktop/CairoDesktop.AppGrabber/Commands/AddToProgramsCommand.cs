using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.ShellFolders;
using System.Collections.Generic;

namespace CairoDesktop.AppGrabber.Commands
{
    public class AddToProgramsCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly IAppGrabber _appGrabber;
        private readonly AddToProgramsCommandInfo _info;

        public AddToProgramsCommand(IAppGrabber appGrabber)
        {
            _appGrabber = appGrabber;
            _info = new AddToProgramsCommandInfo(_appGrabber);
        }

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

            _appGrabber.AddByPath(path, AppCategoryType.Uncategorized);

            return true;
        }
    }

    public class AddToProgramsCommandInfo : ICairoShellItemCommandInfo
    {
        public string Identifier => "AddToPrograms";

        public string Description => "Adds the specified application to the programs menu.";

        public string Label => DisplayString.sCommand_AddToPrograms;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "Application to add to the programs menu",
                IsRequired = true
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;

        private readonly IAppGrabber _appGrabber;

        public AddToProgramsCommandInfo(IAppGrabber appGrabber)
        {
            _appGrabber = appGrabber;
        }

        public string LabelForShellItem(ShellItem item)
        {
            return Label;
        }

        public bool IsAvailableForShellItem(ShellItem item)
        {
            if (item.IsNavigableFolder)
            {
                return false;
            }

            return _appGrabber.CanAddPathToCategory(item.Path, AppCategoryType.All);
        }
    }
}
