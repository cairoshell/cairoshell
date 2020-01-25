using System.Drawing;

namespace DarkUI.Docking
{
    internal class DarkDockTab
    {
        #region Property Region

        public DarkDockContent DockContent { get; set; }

        public Rectangle ClientRectangle { get; set; }

        public Rectangle CloseButtonRectangle { get; set; }

        public bool Hot { get; set; }

        public bool CloseButtonHot { get; set; }

        public bool ShowSeparator { get; set; }

        #endregion

        #region Constructor Region

        public DarkDockTab(DarkDockContent content)
        {
            DockContent = content;
        }

        #endregion

        #region Method Region

        public int CalculateWidth(Graphics g, Font font)
        {
            var width = (int)g.MeasureString(DockContent.DockText, font).Width;
            width += 10;

            return width;
        }

        #endregion
    }
}
