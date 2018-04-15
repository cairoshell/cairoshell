using System;
using System.Collections;
using System.Diagnostics;

namespace CairoDesktop.Common
{
    /// <summary>
	/// Summary description for DisposableCollection.
	/// </summary>
	[DebuggerStepThrough]
    [Serializable]
    public abstract class DisposableCollection : CollectionBase, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed
        /// </summary>
        public bool Disposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Returns an object that can be used to synchronize access to the collection
        /// </summary>
        public object SyncRoot
        {
            get { return InnerList.SyncRoot; }
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
            lock (SyncRoot)
            {
                foreach (object obj in InnerList)
                {
                    var disposable = obj as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }

                InnerList.Clear();
            }
        }

        /// <summary>
        /// Override to dispose of unmanaged resources
        /// </summary>
        protected virtual void DisposeOfUnManagedResources()
        {
        }

        /// <summary>
        /// Sorts the items in the collection using the specified comparer
        /// </summary>
        /// <param name="comparer">The IComparer implementation to use when sorting. -or- null to use the IComparable implementation of the items in the collection</param>
        public virtual void Sort(IComparer comparer)
        {
            lock (SyncRoot)
            {
                InnerList.Sort(comparer);
            }
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
                        // dispose of managed resources here
                        DisposeOfManagedResources();

                    // dispose of unmanaged resources
                    DisposeOfUnManagedResources();
                }
                _disposed = true;
            }
        }
    }
}