using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace CairoDesktop.AppGrabber {
    public class AppViewSorter {

        /// <summary>
        /// Sorts the default view of the specified collection of ApplicationInfo objects by the specified property.
        /// </summary>
        /// <param name="collection">Collection to sort.</param>
        /// <param name="propertyName">Property to sort by.</param>
        public static void Sort(ICollection<ApplicationInfo> collection, String propertyName) {
            ICollectionView view = CollectionViewSource.GetDefaultView(collection);
            if (view.SortDescriptions.Count > 0) {
                view.SortDescriptions.Clear();
            }
            view.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Ascending));
        }

    }
}
