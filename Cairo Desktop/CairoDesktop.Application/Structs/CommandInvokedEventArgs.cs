using CairoDesktop.Application.Interfaces;

namespace CairoDesktop.Application.Structs
{
    public struct CommandInvokedEventArgs
    {
        public ICairoCommandInfo CommandInfo;
        public object[] Parameters;
        public bool Result;
    }
}
