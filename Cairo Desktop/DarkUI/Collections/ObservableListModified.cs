using System;
using System.Collections.Generic;

namespace DarkUI.Collections
{
    public class ObservableListModified<T> : EventArgs
    {
        public IEnumerable<T> Items { get; private set; }

        public ObservableListModified(IEnumerable<T> items)
        {
            Items = items;
        }
    }
}
