using ManagedShell.Common.Common;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Common
{
    public abstract class StackLocation
    {
        public abstract string Path { get; }
        public abstract string DisplayName { get; }
        public abstract bool IsDesktop { get; }
        public abstract ThreadSafeObservableCollection<ShellFile> Files { get; }
        public abstract bool IsFileSystem { get; }
    }
}