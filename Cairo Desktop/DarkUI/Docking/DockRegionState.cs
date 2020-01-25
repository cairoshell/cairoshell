using System.Collections.Generic;
using System.Drawing;

namespace DarkUI.Docking
{
    public class DockRegionState
    {
        #region Property Region

        public DarkDockArea Area { get; set; }

        public Size Size { get; set; }

        public List<DockGroupState> Groups { get; set; }

        #endregion

        #region Constructor Region

        public DockRegionState()
        {
            Groups = new List<DockGroupState>();
        }

        public DockRegionState(DarkDockArea area)
            : this()
        {
            Area = area;
        }

        public DockRegionState(DarkDockArea area, Size size)
            : this(area)
        {
            Size = size;
        }

        #endregion
    }
}
