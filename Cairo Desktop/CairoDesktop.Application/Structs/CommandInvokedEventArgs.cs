using CairoDesktop.Application.Interfaces;
using System.Collections.Generic;

namespace CairoDesktop.Application.Structs
{
    public struct CommandInvokedEventArgs
    {
        public ICairoCommandInfo CommandInfo;
        public List<CairoCommandParameter> Parameters;
        public bool Result;
    }
}
