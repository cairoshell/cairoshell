using ManagedShell.Common.Common;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Common
{
    public class ShellFolderStackLocation : StackLocation
    {
        private readonly ShellFolder _shellFolder;

        public ShellFolderStackLocation(ShellFolder shellFolder)
        {
            _shellFolder = shellFolder;
        }

        public override string Path { get { return _shellFolder.Path; } }
        public override string DisplayName
        {
            get { return _shellFolder.DisplayName; }
        }
        public override bool IsDesktop { get { return _shellFolder.IsDesktop; } }

        public override ThreadSafeObservableCollection<ShellFile> Files
        {
            get { return _shellFolder.Files; }
        }

        public override bool IsFileSystem
        {
            get { return _shellFolder.IsFileSystem; }
        }
    }
}