using System;
using CairoDesktop.Application.Structs;

namespace CairoDesktop.Application.Interfaces
{
    public interface IMenuBar
    {
        IntPtr GetHandle();
        
        bool IsMainMenuBar();

        MenuBarDimensions GetDimensions();
    }
}
