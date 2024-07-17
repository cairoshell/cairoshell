using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class DeleteFileCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly DeleteFileCommandInfo _info = new DeleteFileCommandInfo();

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

            if (KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
            {
                return ShellHelper.SendToRecycleBin(path);
            }

            string displayName = ShellHelper.GetDisplayName(path);
            CairoMessage.ShowOkCancel(string.Format(DisplayString.sDesktop_DeleteInfo, displayName),
                DisplayString.sDesktop_DeleteTitle, CairoMessageImage.Warning,
                DisplayString.sInterface_Delete, DisplayString.sInterface_Cancel,
                result =>
                {
                    if (result == true)
                    {
                        ShellHelper.SendToRecycleBin(path);
                    }
                });

            return true;
        }
    }

    public class DeleteFileCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "DeleteFile";

        public string Description => "Prompts to user to the specified file.";

        public string Label => DisplayString.sInterface_Delete;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "File path to delete",
                IsRequired = true
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;
    }
}
