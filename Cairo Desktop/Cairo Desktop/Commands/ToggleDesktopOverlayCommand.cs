﻿using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Interfaces;
using CairoDesktop.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CairoDesktop.Commands
{
    public class ToggleDesktopOverlayCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly IDesktopManager _desktopManager;
        private readonly ToggleDesktopOverlayCommandInfo _info = new ToggleDesktopOverlayCommandInfo();

        public ToggleDesktopOverlayCommand(IDesktopManager desktopManager) {
            _desktopManager = desktopManager;

            if (_desktopManager is DesktopManager desktopOverlayManager)
            {
                desktopOverlayManager.PropertyChanged += _desktopManager_PropertyChanged;
            }

            SetLabel(_info, _desktopManager.IsOverlayOpen);
            _info.IsAvailable = _desktopManager != null;
        }

        private void _desktopManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsOverlayOpen")
            {
                SetLabel(_info, _desktopManager.IsOverlayOpen);
            }
        }

        private void SetLabel(ToggleDesktopOverlayCommandInfo info, bool isOpen)
        {
            info.Label = isOpen ? "Close desktop overlay" : "Open desktop overlay";
        }

        public void Setup() { }

        public bool Execute(params (string name, object value)[] parameters)
        {
            _desktopManager?.ToggleOverlay();
            return _desktopManager != null;
        }

        public void Dispose() {
            if (_desktopManager is DesktopManager desktopOverlayManager)
            {
                desktopOverlayManager.PropertyChanged -= _desktopManager_PropertyChanged;
            }
        }
    }

    public class ToggleDesktopOverlayCommandInfo : ICairoCommandInfo, INotifyPropertyChanged
    {
        public string Identifier => "ToggleDesktopOverlay";

        public string Description => "Toggles the Desktop Overlay.";

        private string _label;
        public string Label
        {
            get => _label;
            internal set
            {
                if (value == _label) return;
                _label = value;
                OnPropertyChanged();
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}