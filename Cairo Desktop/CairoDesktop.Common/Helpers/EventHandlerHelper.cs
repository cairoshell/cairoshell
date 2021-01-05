using System;
using ManagedShell.Common.Logging;

namespace CairoDesktop.Common.Helpers
{
    public static class EventHandlerHelper
    {
        /// <summary>
        /// Calls the specified delegate, using traditional event raising methodologies.
        /// </summary>
        /// <typeparam name="TEventArgs">The EventArgs Type that will be provided to the generic method.</typeparam>
        /// <param name="handler">The delegate to call.</param>
        /// <param name="sender">The sender to the delegate..</param>
        /// <param name="e">The EventArgs to the delegate.</param>
        public static void Raise<TEventArgs>(EventHandler<TEventArgs> handler, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            try
            {
                handler?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                ShellLogger.Error("Error raising event.", ex);
            }
        }

        public static void Raise(EventHandler handler, object sender, EventArgs e)
        {
            try
            {
                handler?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                ShellLogger.Error("Error raising event.", ex);
            }
        }
    }
}