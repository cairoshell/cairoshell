using CairoDesktop.Application.Interfaces;

namespace CairoDesktop.Application.Structs
{
    public struct CommandInvokedEventArgs
    {
        public ICairoCommandInfo CommandInfo;
        public (string name, object value)[] Parameters;
        public bool Result;
    }
}
