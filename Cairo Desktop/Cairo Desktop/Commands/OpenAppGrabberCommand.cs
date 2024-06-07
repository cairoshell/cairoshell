using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;

namespace CairoDesktop.Commands
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

        public void Setup() { }

        public bool Execute(params object[] parameters)
        {
            _appGrabber?.ShowDialog();

            return _appGrabber != null;
        }

        public void Dispose() { }
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

        public List<CairoCommandParameter> Parameters => new List<CairoCommandParameter>();
    }
}
