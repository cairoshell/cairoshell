using DarkUI.Icons;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Forms
{
    public partial class DarkMessageBox : DarkDialog
    {
        #region Field Region

        private string _message;
        private int _maximumWidth = 350;

        #endregion

        #region Property Region

        [Description("Determines the maximum width of the message box when it autosizes around the displayed message.")]
        [DefaultValue(350)]
        public int MaximumWidth
        {
            get { return _maximumWidth; }
            set
            {
                _maximumWidth = value;
                CalculateSize();
            }
        }

        #endregion

        #region Constructor Region

        public DarkMessageBox()
        {
            InitializeComponent();
        }

        public DarkMessageBox(string message, string title, DarkMessageBoxIcon icon, DarkDialogButton buttons)
            : this()
        {
            Text = title;
            _message = message;

            DialogButtons = buttons;
            SetIcon(icon);
        }

        public DarkMessageBox(string message)
            : this(message, null, DarkMessageBoxIcon.None, DarkDialogButton.Ok)
        { }

        public DarkMessageBox(string message, string title)
            : this(message, title, DarkMessageBoxIcon.None, DarkDialogButton.Ok)
        { }

        public DarkMessageBox(string message, string title, DarkDialogButton buttons)
            : this(message, title, DarkMessageBoxIcon.None, buttons)
        { }

        public DarkMessageBox(string message, string title, DarkMessageBoxIcon icon)
            : this(message, title, icon, DarkDialogButton.Ok)
        { }

        #endregion

        #region Static Method Region

        public static DialogResult ShowInformation(string message, string caption, DarkDialogButton buttons = DarkDialogButton.Ok)
        {
            return ShowDialog(message, caption, DarkMessageBoxIcon.Information, buttons);
        }

        public static DialogResult ShowWarning(string message, string caption, DarkDialogButton buttons = DarkDialogButton.Ok)
        {
            return ShowDialog(message, caption, DarkMessageBoxIcon.Warning, buttons);
        }

        public static DialogResult ShowError(string message, string caption, DarkDialogButton buttons = DarkDialogButton.Ok)
        {
            return ShowDialog(message, caption, DarkMessageBoxIcon.Error, buttons);
        }

        private static DialogResult ShowDialog(string message, string caption, DarkMessageBoxIcon icon, DarkDialogButton buttons)
        {
            using (var dlg = new DarkMessageBox(message, caption, icon, buttons))
            {
                var result = dlg.ShowDialog();
                return result;
            }
        }

        #endregion

        #region Method Region

        private void SetIcon(DarkMessageBoxIcon icon)
        {
            switch (icon)
            {
                case DarkMessageBoxIcon.None:
                    picIcon.Visible = false;
                    lblText.Left = 10;
                    break;
                case DarkMessageBoxIcon.Information:
                    picIcon.Image = MessageBoxIcons.info;
                    break;
                case DarkMessageBoxIcon.Warning:
                    picIcon.Image = MessageBoxIcons.warning;
                    break;
                case DarkMessageBoxIcon.Error:
                    picIcon.Image = MessageBoxIcons.error;
                    break;
            }
        }

        private void CalculateSize()
        {
            var width = 260; var height = 124;

            // Reset form back to original size
            Size = new Size(width, height);

            lblText.Text = string.Empty;
            lblText.AutoSize = true;
            lblText.Text = _message;

            // Set the minimum dialog size to whichever is bigger - the original size or the buttons.
            var minWidth = Math.Max(width, TotalButtonSize + 15);

            // Calculate the total size of the message
            var totalWidth = lblText.Right + 25;

            // Make sure we're not making the dialog bigger than the maximum size
            if (totalWidth < _maximumWidth)
            {
                // Width is smaller than the maximum width.
                // This means we can have a single-line message box.
                // Move the label to accomodate this.
                width = totalWidth;
                lblText.Top = picIcon.Top + (picIcon.Height / 2) - (lblText.Height / 2);
            }
            else
            {
                // Width is larger than the maximum width.
                // Change the label size and wrap it.
                width = _maximumWidth;
                var offsetHeight = Height - picIcon.Height;
                lblText.AutoUpdateHeight = true;
                lblText.Width = width - lblText.Left - 25;
                height = offsetHeight + lblText.Height;
            }

            // Force the width to the minimum width
            if (width < minWidth)
                width = minWidth;

            // Set the new size of the dialog
            Size = new Size(width, height);
        }

        #endregion

        #region Event Handler Region

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CalculateSize();
        }

        #endregion
    }
}
