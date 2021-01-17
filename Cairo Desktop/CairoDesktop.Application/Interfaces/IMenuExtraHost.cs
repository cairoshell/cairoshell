using System;
using CairoDesktop.Application.Structs;

namespace CairoDesktop.Application.Interfaces
{
    public interface IMenuExtraHost
    {
        IntPtr GetHandle();
        
        bool GetIsPrimaryDisplay();

        MenuExtraHostDimensions GetDimensions();
    }
}
