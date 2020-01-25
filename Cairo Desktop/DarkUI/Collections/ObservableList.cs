using System;
using System.Collections.Generic;
using System.Linq;

namespace DarkUI.Collections
{
    public class ObservableList<T> : List<T>, IDisposable
    {
        #region Field Region

        private bool _disposed;

        #endregion

        #region Event Region

        public event EventHandler<ObservableListModified<T>> ItemsAdded;
        public event EventHandler<ObservableListModified<T>> ItemsRemoved;

        #endregion

        #region Destructor Region

        ~ObservableList()
        {
            Dispose(false);
        }

        #endregion

        #region Dispose Region

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (ItemsAdded != null)
                    ItemsAdded = null;

                if (ItemsRemoved != null)
                    ItemsRemoved = null;

                _disposed = true;
            }
        }

        #endregion

        #region Method Region

        public new void Add(T item)
        {
            base.Add(item);

            if (ItemsAdded != null)
                ItemsAdded(this, new ObservableListModified<T>(new List<T> { item }));
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            var list = collection.ToList();

            base.AddRange(list);

            if (ItemsAdded != null)
                ItemsAdded(this, new ObservableListModified<T>(list));
        }

        public new void Remove(T item)
        {
            base.Remove(item);

            if (ItemsRemoved != null)
                ItemsRemoved(this, new ObservableListModified<T>(new List<T> { item }));
        }

        #endregion
    }
}
