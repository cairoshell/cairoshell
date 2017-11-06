using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace CairoDesktop.AppGrabber {

    public class CategoryList : ObservableCollection<Category> {

        /// <summary>
        /// Simple wrapper around an ObservableCollection of Category objects.
        /// </summary>
        public CategoryList(bool firstStart = false) {
            // add default categories
            Add(new Category("All"));

            if (firstStart)
            {
                Add(new Category("Uncategorized", false));
                Add(new Category("Quick Launch", false));
            }
        }

        /// <summary>
        /// Returns the category with the specified name.
        /// </summary>
        public Category GetCategory(string categoryName) {
            foreach (Category c in this) {
                if (c.Name == categoryName) {
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// Add a Category object to this CategoryList.
        /// </summary>
        /// <param name="category">Category to add.</param>
        public new void Add(Category category) {
            base.Add(category);
            category.ParentCategoryList = this;
        }

        /// <summary>
        /// Changes the index of the specified Category by the specified amount.
        /// Returns false if the index is outside the bounds of the list. Otherwise, true.
        /// </summary>
        /// <param name="category">Category to move.</param>
        /// <param name="delta">Number of places to move relative to starting index.</param>
        public bool MoveCategory(Category category, int delta) {
            int currentIndex = this.IndexOf(category);
            int requestedIndex = currentIndex + delta;
            if (requestedIndex < 0 || requestedIndex > this.Count - 1) {
                return false;
            } else {
                this.Move(currentIndex, requestedIndex);
            }
            return true;
        }

        public ObservableCollection<ApplicationInfo> FlatList {
            get {
                ObservableCollection<ApplicationInfo> rval = new ObservableCollection<ApplicationInfo>();
                foreach (Category cat in this) {
                    if (cat.Name == "Quick Launch" || cat.Name == "All") continue;
                    foreach (ApplicationInfo app in cat) {
                        rval.Add(app);
                    }
                }
                return rval;
            }
        }

        public void Serialize(string ConfigFile) {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("CategoryList");
            doc.AppendChild(root);
            foreach (Category cat in this) {
                if (cat.Name != "All")
                {
                    XmlElement catElement = doc.CreateElement("Category");
                    XmlAttribute catNameAttribute = doc.CreateAttribute("Name");
                    catNameAttribute.Value = cat.Name;
                    catElement.Attributes.Append(catNameAttribute);
                    XmlAttribute catShowInMenuAttribute = doc.CreateAttribute("ShowInMenu");
                    catShowInMenuAttribute.Value = cat.ShowInMenu.ToString();
                    catElement.Attributes.Append(catShowInMenuAttribute);
                    root.AppendChild(catElement);
                    foreach (ApplicationInfo app in cat)
                    {
                        XmlElement appElement = doc.CreateElement("Application");
                        catElement.AppendChild(appElement);
                        XmlAttribute appAskAlwaysAdminAttribute = doc.CreateAttribute("AskAlwaysAdmin");
                        appAskAlwaysAdminAttribute.Value = app.AskAlwaysAdmin.ToString();
                        appElement.Attributes.Append(appAskAlwaysAdminAttribute);
                        XmlAttribute appAlwaysAdminAttribute = doc.CreateAttribute("AlwaysAdmin");
                        appAlwaysAdminAttribute.Value = app.AlwaysAdmin.ToString();
                        appElement.Attributes.Append(appAlwaysAdminAttribute);
                        XmlElement appNameElement = doc.CreateElement("Name");
                        appNameElement.InnerText = app.Name;
                        appElement.AppendChild(appNameElement);
                        XmlElement pathElement = doc.CreateElement("Path");
                        pathElement.InnerText = app.Path;
                        appElement.AppendChild(pathElement);
                        XmlElement targetElement = doc.CreateElement("Target");
                        targetElement.InnerText = app.Target;
                        appElement.AppendChild(targetElement);
                    }
                }
            }
            doc.Save(ConfigFile);
        }

        public static CategoryList Deserialize(string ConfigFile) {
            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigFile);
            XmlElement root = doc.ChildNodes[0] as XmlElement;
            CategoryList catList = new CategoryList();
            foreach (XmlElement catElement in root.ChildNodes) {
                // get category
                Category cat = new Category();
                cat.Name = catElement.Attributes["Name"].Value;
                if (catElement.Attributes["ShowInMenu"] != null)
                    cat.ShowInMenu = Convert.ToBoolean(catElement.Attributes["ShowInMenu"].Value);
                else
                {
                    // force hide quick launch and uncategorized
                    if (cat.Name == "Uncategorized")
                        cat.ShowInMenu = false;
                    if (cat.Name == "Quick Launch")
                        cat.ShowInMenu = false;
                }

                catList.Add(cat);

                foreach (XmlElement appElement in catElement.ChildNodes) {
                    // get application
                    ApplicationInfo app = new ApplicationInfo();
                    app.Name = appElement.ChildNodes[0].InnerText;
                    app.Path = appElement.ChildNodes[1].InnerText;
                    if (appElement.Attributes["AskAlwaysAdmin"] != null)
                        app.AskAlwaysAdmin = Convert.ToBoolean(appElement.Attributes["AskAlwaysAdmin"].Value);
                    if (appElement.Attributes["AlwaysAdmin"] != null)
                        app.AlwaysAdmin = Convert.ToBoolean(appElement.Attributes["AlwaysAdmin"].Value);
                    if (appElement.ChildNodes.Count > 2)
                        app.Target = appElement.ChildNodes[2].InnerText;
                    if (!app.IsStoreApp && !Interop.Shell.Exists(app.Path)) {
                        System.Diagnostics.Debug.WriteLine(app.Path + " does not exist");
                        continue;
                    }
                    
                    cat.Add(app);
                }
            }
            return catList;
        }
    }
}
