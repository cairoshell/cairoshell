using DarkUI.Controls;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Docking
{
    internal class DarkDockTabArea
    {
        #region Field Region

        private Dictionary<DarkDockContent, DarkDockTab> _tabs = new Dictionary<DarkDockContent, DarkDockTab>();

        private List<ToolStripMenuItem> _menuItems = new List<ToolStripMenuItem>();
        private DarkContextMenu _tabMenu = new DarkContextMenu();

        #endregion

        #region Property Region

        public DarkDockArea DockArea { get; private set; }

        public Rectangle ClientRectangle { get; set; }

        public Rectangle DropdownRectangle { get; set; }

        public bool DropdownHot { get; set; }

        public int Offset { get; set; }

        public int TotalTabSize { get; set; }

        public bool Visible { get; set; }

        public DarkDockTab ClickedCloseButton { get; set; }

        #endregion
        
        #region Constructor Region

        public DarkDockTabArea(DarkDockArea dockArea)
        {
            DockArea = dockArea;
        }

        #endregion

        #region Method Region

        public void ShowMenu(Control control, Point location)
        {
            _tabMenu.Show(control, location);
        }

        public void AddMenuItem(ToolStripMenuItem menuItem)
        {
            _menuItems.Add(menuItem);
            RebuildMenu();
        }

        public void RemoveMenuItem(ToolStripMenuItem menuItem)
        {
            _menuItems.Remove(menuItem);
            RebuildMenu();
        }

        public ToolStripMenuItem GetMenuItem(DarkDockContent content)
        {
            ToolStripMenuItem menuItem = null;
            foreach (ToolStripMenuItem item in _menuItems)
            {
                var menuContent = item.Tag as DarkDockContent;
                if (menuContent == null)
                    continue;

                if (menuContent == content)
                    menuItem = item;
            }

            return menuItem;
        }

        public void RebuildMenu()
        {
            _tabMenu.Items.Clear();

            var orderedItems = new List<ToolStripMenuItem>();

            var index = 0;
            for (var i = 0; i < _menuItems.Count; i++)
            {
                foreach (var item in _menuItems)
                {
                    var content = (DarkDockContent)item.Tag;
                    if (content.Order == index)
                        orderedItems.Add(item);
                }
                index++;
            }

            foreach (var item in orderedItems)
                _tabMenu.Items.Add(item);
        }

        #endregion
    }
}
