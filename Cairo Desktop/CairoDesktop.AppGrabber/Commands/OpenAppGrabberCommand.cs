using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;

namespace CairoDesktop.AppGrabber.Commands
{
    public class OpenAppGrabberCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly IAppGrabber _appGrabber;
        private readonly OpenAppGrabberCommandInfo _info = new OpenAppGrabberCommandInfo();

        public OpenAppGrabberCommand(IAppGrabber appGrabber) {
            _appGrabber = appGrabber;
            _info.IsAvailable = _appGrabber != null;
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            _appGrabber?.ShowDialog();

            return _appGrabber != null;
        }
    }

    public class OpenAppGrabberCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenAppGrabber";

        public string Description => "Opens the App Grabber.";

        public string Label => DisplayString.sAppGrabber;

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
