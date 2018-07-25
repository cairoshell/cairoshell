using CairoDesktop.Common.Logging;
using CairoDesktop.Common.Logging.Observers;
using CairoDesktop.Extensibility.ObjectModel;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace CairoDesktop.Extensions.ConsoleOutput
{
    /// <summary>
    /// Future versions of this should add an item under "Cairo" menu to toggle Console output
    /// For now this is an optional extention and will not be compiled to the Extensions folder
    /// </summary>
    [Export(typeof(ShellExtension))]
    public sealed class ConsoleOutputExtension : ShellExtension
    {
        public ConsoleOutputExtension()
        {

        }

        public override void Start()
        {
            if (Debugger.IsAttached == true)
            {
                // Since there is a debugger attached,
                // assume we are running from the IDE
                // and Console Output is being sent there
            }
            else
            {
                // Assume we aren't running from the IDE
                ConsoleManager.Show(true);
                CairoLogger.Instance.Attach(new ConsoleLog());
                Console.WriteLine("Console Output Extension Started");
            }
        }

        public override void Stop()
        {
            ConsoleManager.Hide();
        }
    }
}