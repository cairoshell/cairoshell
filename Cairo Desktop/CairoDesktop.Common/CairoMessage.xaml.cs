namespace CairoDesktop.Common
{
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Provides a custom message dialog for the Cairo Desktop.
    /// </summary>
    public partial class CairoMessage : Window
    {
        private static DependencyProperty buttonsProperty = DependencyProperty.Register("Buttons", typeof(MessageBoxButton), typeof(CairoMessage));
        private static DependencyProperty imageProperty = DependencyProperty.Register("Image", typeof(MessageBoxImage), typeof(CairoMessage));
        private static DependencyProperty messageProperty = DependencyProperty.Register("Message", typeof(string), typeof(CairoMessage));

        /// <summary>
        /// Initializes a new instance of the CairoMessage class.
        /// </summary>
        public CairoMessage()
        {
            InitializeComponent();
        }

        #region Public Properties
        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        /// <value>The contents of the message.</value>
        public string Message
        {
            get
            {
                return (string)GetValue(messageProperty);
            }

            set
            {
                SetValue(messageProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the type of image to display in the dialog.
        /// </summary>
        /// <value>The type of image to display.</value>
        public MessageBoxImage Image
        {
            get
            {
                return (MessageBoxImage)GetValue(imageProperty);
            }

            set
            {
                SetValue(imageProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the type of buttons to display.
        /// </summary>
        /// <value>The button combination to use.</value>
        public MessageBoxButton Buttons
        {
            get
            {
                return (MessageBoxButton)GetValue(buttonsProperty);
            }

            set
            {
                SetValue(buttonsProperty, value);
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Displays the Cairo Message Dialog with the default Ok/Cancel button and Icon.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <returns>Nullable bool indicating user response.</returns>
        public static bool? Show(string message, string title)
        {
            CairoMessage msgDialog = new CairoMessage();
            msgDialog.Message = message;
            msgDialog.Title = title;
            msgDialog.Image = MessageBoxImage.None;
            msgDialog.Buttons = MessageBoxButton.OKCancel;

            return msgDialog.ShowDialog();
        }

        /// <summary>
        /// Displays the Cairo Message Dialog with implicit settings.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="buttons">The buttons configuration to use.</param>
        /// <param name="image">The image to display.</param>
        /// <returns>Nullable bool indicating user response.</returns>
        public static bool? Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            CairoMessage msgDialog = new CairoMessage();
            msgDialog.Message = message;
            msgDialog.Title = title;
            msgDialog.Image = image;
            msgDialog.Buttons = buttons;

            return msgDialog.ShowDialog();
        }

        /// <summary>
        /// Displays the Cairo Message as an alert with implicit settings.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="image">The image to display.</param>
        /// <returns>void</returns>
        [STAThread]
        public static void ShowAlert(string message, string title, MessageBoxImage image)
        {
            CairoMessage msgDialog = new CairoMessage();
            msgDialog.Message = message;
            msgDialog.Title = title;
            msgDialog.Image = image;
            msgDialog.Buttons = MessageBoxButton.OK;

            msgDialog.Show();

            return;
        }

        /// <summary>
        /// Displays the Cairo Message Dialog with OK/Cancel buttons, implicit settings, custom image and button text.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="ImageSource">The path to the image for the dialog.</param>
        /// <param name="OkButtonText">The text for the OK button.</param>
        /// <param name="CancelButtonText">The text for the cancel button.</param>
        /// <returns>Nullable bool indicating the user response.</returns>
        public static bool? ShowOkCancel(string message, string title, string ImageSource, string OkButtonText, string CancelButtonText)
        {
            if (string.IsNullOrEmpty(CancelButtonText))
            {
                CancelButtonText = CairoDesktop.Localization.DisplayString.sInterface_Cancel;
            }

            if(string.IsNullOrEmpty(OkButtonText))
            {
                OkButtonText = CairoDesktop.Localization.DisplayString.sInterface_OK;
            }

            if(string.IsNullOrEmpty(ImageSource))
            {
                ImageSource = "Resources/cairoIcon.png";
            }

            CairoMessage msgDialog = new CairoMessage();
            msgDialog.Message = message;
            msgDialog.Title = title;
            msgDialog.Buttons = MessageBoxButton.OKCancel;
            msgDialog.OkButton.Content = OkButtonText;
            msgDialog.CancelButton.Content = CancelButtonText;
            msgDialog.MessageIconImage.Source = new BitmapImage(new System.Uri(ImageSource, System.UriKind.RelativeOrAbsolute));
            
            return msgDialog.ShowDialog();
        }
        #endregion

        private bool IsModal()
        {
            return (bool)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsModal())
                this.DialogResult = true;
            else
                this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void messageWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
