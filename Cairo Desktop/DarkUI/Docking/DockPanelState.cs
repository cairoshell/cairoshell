using System.Collections.Generic;

namespace NeoGeniX.Docking
{
    public class DockPanelState
    {
        #region Property Region

        public List<DockRegionState> Regions { get; set; }

        #endregion

        #region Constructor Region

        public DockPanelState()
        {
            Regions = new List<DockRegionState>();
        }

        #endregion
    }
}
