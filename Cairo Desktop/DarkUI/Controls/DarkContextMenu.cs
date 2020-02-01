using NeoGeniX.Renderers;
using System.Windows.Forms;

namespace NeoGeniX.Controls
{
    public class DarkContextMenu : ContextMenuStrip
    {
        #region Constructor Region

        public DarkContextMenu()
        {
            Renderer = new DarkMenuRenderer();
        }

        #endregion
    }
}
