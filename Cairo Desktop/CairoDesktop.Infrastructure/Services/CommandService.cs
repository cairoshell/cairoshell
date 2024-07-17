using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CairoDesktop.Infrastructure.Services
{
    public class CommandService : ICommandService
    {
        private readonly IHost _host;
        private readonly ILogger<CommandService> _logger;

        private IEnumerable<ICairoCommand> _commands;
        private IReadOnlyCollection<ICairoCommandInfo> _registeredCommands = new List<ICairoCommandInfo>().AsReadOnly();

        public IReadOnlyCollection<ICairoCommandInfo> Commands => _registeredCommands;

        public event EventHandler<CommandInvokedEventArgs> CommandInvoked;

        public CommandService(ILogger<CommandService> logger, IHost host)
        {
            _host = host;
            _logger = logger;
        }

        public void Start()
        {
            // Retrieve all commands. We get them here rather than via the constructor to prevent a circular dependency
            // when a command depends on a service that also invokes commands.
            _commands = _host.Services.GetServices<ICairoCommand>();
            var identifiers = new List<string>();
            var registeredCommands = new List<ICairoCommandInfo>();

            foreach (var command in _commands)
            {
                if (string.IsNullOrEmpty(command.Info.Identifier))
                {
                    _logger.LogError($"Unable to register command with missing identifier");
                    continue;
                }
                if (identifiers.Contains(command.Info.Identifier))
                {
                    _logger.LogError($"Unable to register command with duplicate identifier {command.Info.Identifier}");
                    continue;
                }
                try
                {
                    registeredCommands.Add(command.Info);
                    identifiers.Add(command.Info.Identifier);
                    _logger.LogDebug($"Registered command {command.Info.Identifier}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unable to register command {command.Info.Identifier}: {ex.Message}");
                }
            }

            _registeredCommands = registeredCommands.AsReadOnly();
        }

        public bool InvokeCommand(string identifier, params (string name, object value)[] parameters)
        {
            if (_commands == null)
            {
                _logger.LogError($"Unable to execute command {identifier}: No commands are registered.");
                return false;
            }

            try
            {
                var command = _commands.First(c => c.Info.Identifier == identifier);

                if (!command.Info.IsAvailable)
                {
                    _logger.LogError($"Unable to execute command {identifier}: The command is currently unavailable.");
                    return false;
                }

                var result = command.Execute(parameters);

                CommandInvoked?.Invoke(this, new CommandInvokedEventArgs
                {
                    CommandInfo = command.Info,
                    Parameters = parameters,
                    Result = result
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to execute command {identifier}: {ex.Message}");
            }

            return false;
        }

        public void Dispose()
        {
            if (_commands == null)
            {
                return;
            }

            foreach (var command in _commands)
            {
                if (command is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _registeredCommands = new List<ICairoCommandInfo>().AsReadOnly();
        }
    }
}
