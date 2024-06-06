using CairoDesktop.Application.Structs;
using System;

namespace CairoDesktop.Application.Interfaces
{
   public interface ICommandService : IDisposable
   {
        /// <summary>
        /// Retrieves and registers all commands so that they are available.
        /// </summary>
        void Start();

        /// <summary>
        /// Invokes a registered command using the specified parameters.
        /// </summary>
        /// <param name="identifier">Unique identifier of command to invoke</param>
        /// <param name="parameters">Parameters to pass to the command, as defined in the command info</param>
        /// <returns></returns>
        bool InvokeCommand(string identifier, params object[] parameters);

        /// <summary>
        /// Emitted after a command is invoked.
        /// </summary>
        event EventHandler<CommandInvokedEventArgs> CommandInvoked;
   }
}