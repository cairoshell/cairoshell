using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoDesktop.Common
{
    /// <summary>
    /// Defines a class that is disposable.
    /// </summary>
    /// <remarks>This class is a part of the Carbon Framework.</remarks>
    [DebuggerStepThrough]
    [Serializable]
    public abstract class DisposableObject : IDisposable
    {
        private readonly object _syncRoot = new object();
        private bool _disposed;

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed
        /// </summary>
        [Browsable(false)]
        public bool Disposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Returns an object that can be used to synchronize access to the collection
        /// </summary>
        [Browsable(false)]
        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Override to dispose of managed resources
        /// </summary>
        protected virtual void DisposeOfManagedResources()
        {
        }

        /// <summary>
        /// Override to dispose of unmanaged resources
        /// </summary>
        protected virtual void DisposeOfUnManagedResources()
        {
        }

        /// <summary>
        /// Internal disposal function to manage this object's disposed state
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            lock (SyncRoot)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        // dispose of managed resources here
                        DisposeOfManagedResources();
                    }

                    // dispose of unmanaged resources
                    DisposeOfUnManagedResources();

                    _disposed = true;
                }
            }
        }
    }
}