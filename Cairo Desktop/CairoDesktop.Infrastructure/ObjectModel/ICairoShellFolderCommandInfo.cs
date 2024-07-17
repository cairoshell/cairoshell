using CairoDesktop.Application.Interfaces;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public interface ICairoShellFolderCommandInfo : ICairoCommandInfo
    {
        /// <summary>
        /// Short label which will be the primary visual identification of the command.
        /// <param name="folder">ShellFolder to generate a command label for</param>
        /// <returns>Command label for the given ShellFolder</returns>
        /// </summary>
        string LabelForShellFolder(ShellFolder folder);

        /// <summary>
        /// Returns whether or not the command is available to be invoked.
        /// <param name="folder">ShellFolder to determine command availability for</param>
        /// <returns>Command availability for the given ShellFolder</returns>
        /// </summary>
        bool IsAvailableForShellFolder(ShellFolder folder);
    }
}