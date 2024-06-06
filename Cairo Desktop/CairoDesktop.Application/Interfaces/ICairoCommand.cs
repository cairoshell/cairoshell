using System;

namespace CairoDesktop.Application.Interfaces
{
    public interface ICairoCommand : IDisposable
    {
        /// <summary>
        /// Metadata to describe the command, including its unique identifier
        /// </summary>
        ICairoCommandInfo Info { get; }

        /// <summary>
        /// For command setup tasks that aren't part of the constructor
        /// </summary>
        void Setup();

        /// <summary>
        /// Called by the CommandService when the command is invoked
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        bool Execute(params object[] parameters);
    }
}