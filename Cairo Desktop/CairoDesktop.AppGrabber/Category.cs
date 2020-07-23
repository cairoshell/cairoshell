using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Data;

namespace CairoDesktop.AppGrabber {

    [Serializable()]
    public class Category : IList<ApplicationInfo>, INotifyPropertyChanged, INotifyCollectionChanged {

        private bool showInMenu;
        public bool ShowInMenu {
            get {
                return showInMenu;
            }
            set {
                showInMenu = value;

                // Notify data bindings of property change
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("ShowInMenu"));
                }
            }
        }

        private List<ApplicationInfo> appsList;

        private List<ApplicationInfo> internalList
        {
            get
            {
                return appsList;
            }

            set
            {
                appsList = value;
            }
        }

        private String name;
        /// <summary>
        /// Gets/Sets the name of this Category.
        /// </summary>
        public String Name {
            get {
                return name;
            }
            set {
                name = value;

                // Notify data bindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayName")); // because DisplayName returns Name if not a special category type
                }
            }
        }

        private AppCategoryType type = AppCategoryType.Standard;
        /// <summary>
        /// Gets/Sets the type of this Category. 0 = standard; 1 = all; 2 = uncategorized; 3 = quick launch
        /// </summary>
        public AppCategoryType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;

                if (type == AppCategoryType.QuickLaunch)
                    CollectionViewSource.GetDefaultView(this).SortDescriptions.Clear();

                // Notify data bindings of property change
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Type"));
                }
            }
        }

        /// <summary>
        /// Returns category display name. If special category, use localized name
        /// </summary>
        public string DisplayName
        {
            get
            {
                switch (Type)
                {
                    case AppCategoryType.All:
                        return Localization.DisplayString.sAppGrabber_All;
                    case AppCategoryType.Uncategorized:
                        return Localization.DisplayString.sAppGrabber_Uncategorized;
                    case AppCategoryType.QuickLaunch:
                        return Localization.DisplayString.sAppGrabber_QuickLaunch;
                    default:
                        if (Name.Length > 0)
                            return Name;
                        else
                            return Localization.DisplayString.sAppGrabber_Untitled;
                }
            }
        }

        /// <summary>
        /// Object that represents a named list of ApplicationInfos. Default name is "Unknown".
        /// </summary>
        public Category() {
            this.Name = "Unknown";
            this.ShowInMenu = true;
            this.appsList = new List<ApplicationInfo>();
            AppViewSorter.Sort(this, "Name");
        }

        /// <summary>
        /// Object that represents a named list of ApplicationInfos.
        /// </summary>
        /// <param name="name">The name of the category - retrievable from the Name property.</param>
        /// <param name="apps">An IList of AppliationInfo objects to initialize this list to. (Internally uses the AddRange function to set the list)</param>
        public Category(String name, IList<ApplicationInfo> apps) {
            this.Name = name;
            this.ShowInMenu = true;
            this.appsList = new List<ApplicationInfo>();
            this.AddRange(apps);
            AppViewSorter.Sort(this, "Name");
        }

        /// <summary>
        /// Object that represents a named list of ApplicationInfos.
        /// </summary>
        /// <param name="name">The name of the category - retrievable from the Name property.</param>
        /// <param name="showInMenu">Should the category appear in the Programs menu</param>
        public Category(String name, bool showInMenu = true) {
            this.Name = name;
            this.ShowInMenu = showInMenu;
            this.appsList = new List<ApplicationInfo>();
            AppViewSorter.Sort(this, "Name");
        }

        /// <summary>
        /// Object that represents a named list of ApplicationInfos.
        /// </summary>
        /// <param name="name">The name of the category - retrievable from the Name property.</param>
        /// <param name="showInMenu">Should the category appear in the Programs menu</param>
        /// <param name="type">Type of category</param>
        public Category(String name, bool showInMenu, AppCategoryType type)
        {
            this.Name = name;
            this.ShowInMenu = showInMenu;
            this.appsList = new List<ApplicationInfo>();
            AppViewSorter.Sort(this, "Name");
            this.Type = type;
        }

        /// <summary>
        /// Gets or Sets a parent CategoryList object.
        /// </summary>
        public CategoryList ParentCategoryList { get; set; }

        /// <summary>
        /// Removes the item from the All category, if it isn't an item currently in the All or Quick Launch categories.
        /// </summary>
        /// <param name="item">Application info to remove.</param>
        private void RemoveFromAllCategory(ApplicationInfo item)
        {
            // remove corresponding item from the all category
            if (this.Type != AppCategoryType.All && this.Type != AppCategoryType.QuickLaunch)
                this.ParentCategoryList.GetSpecialCategory(AppCategoryType.All).Remove(item);
        }

        /// <summary>
        /// Adds the items to this Category.
        /// </summary>
        /// <param name="items">List of Application infos to add.</param>
        public void Add(ApplicationInfo ai) {
            this.internalList.Add(ai);
            if (this.Type != AppCategoryType.All)
                ai.Category = this;

            // Notify data bindings of property change
            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ai));
            }
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }

            // add corresponding item to the all category (excluding quick launch apps because they are essentially duplicates)
            if (this.Type != AppCategoryType.All && this.Type != AppCategoryType.QuickLaunch)
                this.ParentCategoryList.GetSpecialCategory(AppCategoryType.All).Add(ai);
        }

        /// <summary>
        /// Adds the items in the IList to this Category.
        /// </summary>
        /// <param name="items">List of Application infos to add.</param>
        public void AddRange(IList<ApplicationInfo> items) {
            foreach (ApplicationInfo app in items) {
                this.Add(app);
            }
        }

        /// <summary>
        /// String representation of this Category.
        /// </summary>
        /// <returns>The category name.</returns>
        public override string ToString() {
            return this.DisplayName;
        }

        /// <summary>
        /// PropertyChanged event is necessary for WPF (and WinForms) to be notified of changes while performing data binding.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #region IList<ApplicationInfo> Members

        /// <summary>
        /// Gets the index of the 1st occurance of item
        /// </summary>
        /// <param name="item">ApplicationInfo object to look for.</param>
        /// <returns>Zero-based index of the item or -1 if not found.</returns>
        public int IndexOf(ApplicationInfo item) {
            return internalList.IndexOf(item);
        }

        /// <summary>
        /// Insert the selected item into the list at the given index.
        /// </summary>
        /// <param name="index">Position of insertion.</param>
        /// <param name="item">ApplicationInfo object to insert.</param>
        public void Insert(int index, ApplicationInfo item) {
            internalList.Insert(index, item);
            if (this.Type != AppCategoryType.All)
                item.Category = this;

            // Notify data bindings of property change
            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }

            // add corresponding item to the all category (excluding quick launch apps because they are essentially duplicates)
            if (this.Type != AppCategoryType.All && this.Type != AppCategoryType.QuickLaunch)
                this.ParentCategoryList.GetSpecialCategory(AppCategoryType.All).Insert(0, item);
        }

        /// <summary>
        /// Removes the object stored at the given location in the list.
        /// </summary>
        /// <param name="index">Position of object to be removed.</param>
        public void RemoveAt(int index) {
            ApplicationInfo app = internalList[index];
            internalList[index].Category = null;
            internalList.RemoveAt(index);

            // Notify data bindings of property change
            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, app, index));
            }
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }

            RemoveFromAllCategory(app);
        }

        public void Move(int oldIndex, int newIndex)
        {
            ApplicationInfo item = internalList[oldIndex];
            internalList.RemoveAt(oldIndex);
            internalList.Insert(newIndex, item);

            // Notify data bindings of property change
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }
        }

        /// <summary>
        /// Gets or Sets the item at a given location.
        /// </summary>
        /// <param name="index">Given Location</param>
        /// <returns>Either returns the ApplicationInfo object at the given location or (when setting the value) returns void.</returns>
        public ApplicationInfo this[int index] {
            get {
                return internalList[index];
            }
            set {
                ApplicationInfo oldApp = internalList[index];
                internalList[index] = value;
                internalList[index].Category = this;

                // Notify data bindings of property change
                if (CollectionChanged != null) {
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldApp));
                }
            }
        }

        #endregion

        #region ICollection<ApplicationInfo> Members

        /// <summary>
        /// Removes all ApplicationInfo objects from this list.
        /// </summary>
        public void Clear() {
            foreach (ApplicationInfo ai in internalList) {
                ai.Category = null;

                RemoveFromAllCategory(ai);
            }
            internalList.Clear();

            // Notify data bindings of property change
            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }
        }

        /// <summary>
        /// Determines whether the selected object is contained in this list.
        /// </summary>
        /// <param name="item">Item to search for.</param>
        /// <returns>True if in this list, False if not.</returns>
        public bool Contains(ApplicationInfo item) {
            return internalList.Contains(item);
        }

        /// <summary>
        /// Copies this list to an array of ApplicationInfo objects starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">Array to populate.</param>
        /// <param name="arrayIndex">Index to start from.</param>
        public void CopyTo(ApplicationInfo[] array, int arrayIndex) {
            internalList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of ApplicationInfo objects in this Category.
        /// </summary>
        public int Count {
            get { return internalList.Count; }
        }

        /// <summary>
        /// Always false.
        /// </summary>
        public bool IsReadOnly {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurance of the specified item.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True is successful. Otherwise, False. </returns>
        public bool Remove(ApplicationInfo item) {
            // Need to work with a numbered instance, or else there could be strange issues with duplicates after setting the Category to null
            int index = internalList.IndexOf(item);
            ApplicationInfo info = internalList[index];
            info.Category = null;
            try {
                internalList.RemoveAt(index);

                // Notify data bindings of property change
                if (CollectionChanged != null)
                {
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, info, index));
                }
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("Count"));
                }
                
                RemoveFromAllCategory(item);

                return true;
            } catch {
                return false;
            }
        }

        #endregion

        #region IEnumerable<ApplicationInfo> Members

        /// <summary>
        /// Returns an Enumerator for this list.
        /// </summary>
        public IEnumerator<ApplicationInfo> GetEnumerator() {
            return internalList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an Enumerator for this list.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return internalList.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        /// <summary>
        /// This event is raised whenever an object in the list is added, removed, or replaced. Necessary to sync state when binding.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }

    public enum AppCategoryType: int
    {
        Standard = 0,
        All = 1,
        Uncategorized = 2,
        QuickLaunch = 3
    }
}
