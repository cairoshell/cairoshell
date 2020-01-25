using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Docking
{
    [ToolboxItem(false)]
    public class DarkDockContent : UserControl
    {
        #region Event Handler Region

        public event EventHandler DockTextChanged;

        #endregion

        #region Field Region

        private string _dockText;
        private Image _icon;

        #endregion

        #region Property Region

        [Category("Appearance")]
        [Description("Determines the text that will appear in the content tabs and headers.")]
        public string DockText
        {
            get { return _dockText; }
            set
            {
                var oldText = _dockText;

                _dockText = value;

                if (DockTextChanged != null)
                    DockTextChanged(this, null);

                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines the icon that will appear in the content tabs and headers.")]
        public Image Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                Invalidate();
            }
        }

        [Category("Layout")]
        [Description("Determines the default area of the dock panel this content will be added to.")]
        [DefaultValue(DarkDockArea.Document)]
        public DarkDockArea DefaultDockArea { get; set; }

        [Category("Behavior")]
        [Description("Determines the key used by this content in the dock serialization.")]
        public string SerializationKey { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockPanel DockPanel { get; internal set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockRegion DockRegion { get; internal set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockGroup DockGroup { get; internal set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockArea DockArea { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; }

        #endregion

        #region Constructor Region

        public DarkDockContent()
        { }

        #endregion

        #region Method Region

        public virtual void Close()
        {
            if (DockPanel != null)
                DockPanel.RemoveContent(this);
        }

        #endregion

        #region Event Handler Region

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            if (DockPanel == null)
                return;

            DockPanel.ActiveContent = this;
        }

        #endregion
    }
}
