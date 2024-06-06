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
        private readonly ICairoApplication _app;
        private readonly IHost _host;
        private readonly ILogger<CommandService> _logger;

        private IEnumerable<ICairoCommand> _commands;

        public event EventHandler<CommandInvokedEventArgs> CommandInvoked;

        public CommandService(ICairoApplication app, ILogger<CommandService> logger, IHost host)
        {
            _app = app;
            _host = host;
            _logger = logger;
        }

        public void Start()
        {
            // Retrieve all commands. We get them here rather than via the constructor to prevent a circular dependency
            // when a command depends on a service that also invokes commands.
            _commands = _host.Services.GetServices<ICairoCommand>();
            var identifiers = new List<string>();

            foreach (var command in _commands)
            {
                if (identifiers.Contains(command.Info.Identifier))
                {
                    _logger.LogError($"Unable to register command with duplicate identifier {command.Info.Identifier}");
                    continue;
                }
                try
                {
                    command.Setup();
                    _app.Commands.Add(command.Info);
                    identifiers.Add(command.Info.Identifier);
                    _logger.LogDebug($"Registered command {command.Info.Identifier}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unable to register command {command.Info.Identifier}: {ex.Message}");
                }
            }
        }

        public bool InvokeCommand(string identifier, params object[] parameters)
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
                _app.Commands.Remove(command.Info);
                command.Dispose();
            }
        }
    }
}
