using CairoDesktop.Application.Structs;
using System;
using System.Collections.Generic;

namespace CairoDesktop.Application.Interfaces
{
   public interface ICommandService : IDisposable
    {
        /// <summary>
        /// List of registered commands which can be invoked via the InvokeCommand method
        /// </summary>
        IReadOnlyCollection<ICairoCommandInfo> Commands { get; }

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
        bool InvokeCommand(string identifier, params (string name, object value)[] parameters);

        /// <summary>
        /// Emitted after a command is invoked.
        /// </summary>
        event EventHandler<CommandInvokedEventArgs> CommandInvoked;
   }
}