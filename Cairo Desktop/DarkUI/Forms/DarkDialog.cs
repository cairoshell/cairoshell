using DarkUI.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace DarkUI.Forms
{
    public partial class DarkDialog : DarkForm
    {
        #region Field Region

        private DarkDialogButton _dialogButtons = DarkDialogButton.Ok;
        private List<DarkButton> _buttons;

        #endregion

        #region Button Region

        protected DarkButton btnOk;
        protected DarkButton btnCancel;
        protected DarkButton btnClose;
        protected DarkButton btnYes;
        protected DarkButton btnNo;
        protected DarkButton btnAbort;
        protected DarkButton btnRetry;
        protected DarkButton btnIgnore;

        #endregion

        #region Property Region

        [Description("Determines the type of the dialog window.")]
        [DefaultValue(DarkDialogButton.Ok)]
        public DarkDialogButton DialogButtons
        {
            get { return _dialogButtons; }
            set
            {
                if (_dialogButtons == value)
                    return;

                _dialogButtons = value;
                SetButtons();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TotalButtonSize { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new IButtonControl AcceptButton
        {
            get { return base.AcceptButton; }
            private set { base.AcceptButton = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new IButtonControl CancelButton
        {
            get { return base.CancelButton; }
            private set { base.CancelButton = value; }
        }

        #endregion

        #region Constructor Region

        public DarkDialog()
        {
            InitializeComponent();

            _buttons = new List<DarkButton>
                {
                    btnAbort, btnRetry, btnIgnore, btnOk,
                    btnCancel, btnClose, btnYes, btnNo
                };
        }

        #endregion

        #region Event Handler Region

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            SetButtons();
        }

        #endregion

        #region Method Region

        private void SetButtons()
        {
            foreach (var btn in _buttons)
                btn.Visible = false;

            switch (_dialogButtons)
            {
                case DarkDialogButton.Ok:
                    ShowButton(btnOk, true);
                    AcceptButton = btnOk;
                    break;
                case DarkDialogButton.Close:
                    ShowButton(btnClose, true);
                    AcceptButton = btnClose;
                    CancelButton = btnClose;
                    break;
                case DarkDialogButton.OkCancel:
                    ShowButton(btnOk);
                    ShowButton(btnCancel, true);
                    AcceptButton = btnOk;
                    CancelButton = btnCancel;
                    break;
                case DarkDialogButton.AbortRetryIgnore:
                    ShowButton(btnAbort);
                    ShowButton(btnRetry);
                    ShowButton(btnIgnore, true);
                    AcceptButton = btnAbort;
                    CancelButton = btnIgnore;
                    break;
                case DarkDialogButton.RetryCancel:
                    ShowButton(btnRetry);
                    ShowButton(btnCancel, true);
                    AcceptButton = btnRetry;
                    CancelButton = btnCancel;
                    break;
                case DarkDialogButton.YesNo:
                    ShowButton(btnYes);
                    ShowButton(btnNo, true);
                    AcceptButton = btnYes;
                    CancelButton = btnNo;
                    break;
                case DarkDialogButton.YesNoCancel:
                    ShowButton(btnYes);
                    ShowButton(btnNo);
                    ShowButton(btnCancel, true);
                    AcceptButton = btnYes;
                    CancelButton = btnCancel;
                    break;
            }

            SetFlowSize();
        }

        private void ShowButton(DarkButton button, bool isLast = false)
        {
            button.SendToBack();

            if (!isLast)
                button.Margin = new Padding(0, 0, 10, 0);

            button.Visible = true;
        }

        private void SetFlowSize()
        {
            var width = flowInner.Padding.Horizontal;

            foreach (var btn in _buttons)
            {
                if (btn.Visible)
                    width += btn.Width + btn.Margin.Right;
            }

            flowInner.Width = width;
            TotalButtonSize = width;
        }

        #endregion
    }
}
