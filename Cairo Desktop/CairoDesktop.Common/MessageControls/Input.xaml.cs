using System.Windows.Controls;

namespace CairoDesktop.Common.MessageControls
{
    /// <summary>
    /// Interaction logic for Input.xaml
    /// </summary>
    public partial class Input : UserControl
    {
        public string Text => InputField.Text;

        public Input()
        {
            InitializeComponent();
        }

        public void Initialize(string initialText)
        {
            InputField.Text = initialText;
            InputField.SelectAll();
            InputField.Focus();
        }
    }
}
