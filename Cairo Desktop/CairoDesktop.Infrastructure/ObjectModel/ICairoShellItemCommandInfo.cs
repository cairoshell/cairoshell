using CairoDesktop.Application.Interfaces;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public interface ICairoShellItemCommandInfo : ICairoCommandInfo
    {
        /// <summary>
        /// Short label which will be the primary visual identification of the command.
        /// <param name="item">ShellItem to generate a command label for</param>
        /// <returns>Command label for the given ShellItem</returns>
        /// </summary>
        string LabelForShellItem(ShellItem item);

        /// <summary>
        /// Returns whether or not the command is available to be invoked.
        /// <param name="item">ShellItem to determine command availability for</param>
        /// <returns>Command availability for the given ShellItem</returns>
        /// </summary>
        bool IsAvailableForShellItem(ShellItem item);
    }
}