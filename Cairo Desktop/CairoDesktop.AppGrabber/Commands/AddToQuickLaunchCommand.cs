using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.ShellFolders;
using System.Collections.Generic;

namespace CairoDesktop.AppGrabber.Commands
{
    public class AddToQuickLaunchCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly IAppGrabber _appGrabber;
        private readonly AddToQuickLaunchCommandInfo _info;

        public AddToQuickLaunchCommand(IAppGrabber appGrabber)
        {
            _appGrabber = appGrabber;
            _info = new AddToQuickLaunchCommandInfo(_appGrabber);
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

            _appGrabber.AddByPath(path, AppCategoryType.QuickLaunch);

            return true;
        }
    }

    public class AddToQuickLaunchCommandInfo : ICairoShellItemCommandInfo
    {
        public string Identifier => "AddToQuickLaunch";

        public string Description => "Adds the specified application to quick launch.";

        public string Label => DisplayString.sCommand_AddToQuickLaunch;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "Application to add to quick launch",
                IsRequired = true
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;

        private readonly IAppGrabber _appGrabber;

        public AddToQuickLaunchCommandInfo(IAppGrabber appGrabber)
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

            return _appGrabber.CanAddPathToCategory(item.Path, AppCategoryType.QuickLaunch);
        }
    }
}
