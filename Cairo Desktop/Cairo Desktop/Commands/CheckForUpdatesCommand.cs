using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class CheckForUpdatesCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly IApplicationUpdateService _applicationUpdateService;
        private readonly CheckForUpdatesCommandInfo _info = new CheckForUpdatesCommandInfo();

        public CheckForUpdatesCommand(IApplicationUpdateService applicationUpdateService) {
            _applicationUpdateService = applicationUpdateService;
            _info.IsAvailable = _applicationUpdateService?.IsAvailable == true;
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            _applicationUpdateService?.CheckForUpdates();

            return _applicationUpdateService != null;
        }
    }

    public class CheckForUpdatesCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "CheckForUpdates";

        public string Description => "Checks for Cairo updates.";

        public string Label => DisplayString.sCairoMenu_CheckForUpdates;

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            internal set
            {
                _isAvailable = value;
            }
        }

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
