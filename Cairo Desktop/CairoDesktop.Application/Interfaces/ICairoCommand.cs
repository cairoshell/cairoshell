using System;

namespace CairoDesktop.Application.Interfaces
{
    public interface ICairoCommand
    {
        /// <summary>
        /// Metadata to describe the command, including its unique identifier
        /// </summary>
        ICairoCommandInfo Info { get; }

        /// <summary>
        /// Called by the CommandService when the command is invoked
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        bool Execute(params (string name, object value)[] parameters);
    }
}