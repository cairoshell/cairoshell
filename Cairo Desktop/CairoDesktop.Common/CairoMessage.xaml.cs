using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace CairoDesktop.Common
{
    /// <summary>
    /// Provides a custom message dialog for the Cairo Desktop.
    /// </summary>
    public partial class CairoMessage : Window
    {
        public delegate void DialogResultDelegate(bool? result);

        private static DependencyProperty buttonsProperty = DependencyProperty.Register("Buttons", typeof(MessageBoxButton), typeof(CairoMessage));
        private static DependencyProperty imageProperty = DependencyProperty.Register("Image", typeof(CairoMessageImage), typeof(CairoMessage));
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
        public CairoMessageImage Image
        {
            get
            {
                return (CairoMessageImage)GetValue(imageProperty);
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

        public DialogResultDelegate ResultCallback;
        #endregion

        #region Static Methods
        /// <summary>
        /// Displays the Cairo Message as an alert with implicit settings.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="image">The image to display.</param>
        /// <returns>void</returns>
        [STAThread]
        public static void Show(string message, string title, CairoMessageImage image)
        {
            Show(message, title, MessageBoxButton.OK, image, null);
        }

        /// <summary>
        /// Displays the Cairo Message Dialog with implicit settings.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="buttons">The buttons configuration to use.</param>
        /// <param name="image">The image to display.</param>
        /// <returns>void</returns>
        public static void Show(string message, string title, MessageBoxButton buttons, CairoMessageImage image)
        {
            Show(message, title, buttons, image, null);
        }

        /// <summary>
        /// Displays the Cairo Message Dialog with implicit settings.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="buttons">The buttons configuration to use.</param>
        /// <param name="image">The image to display.</param>
        /// <param name="resultCallback">The delegate to execute upon user action.</param>
        /// <returns>void</returns>
        public static void Show(string message, string title, MessageBoxButton buttons, CairoMessageImage image, DialogResultDelegate resultCallback)
        {
            CairoMessage msgDialog = new CairoMessage
            {
                Message = message,
                Title = title,
                Image = image,
                Buttons = buttons,
                ResultCallback = resultCallback
            };

            msgDialog.Show();
        }

        /// <summary>
        /// Displays the Cairo Message as an alert with implicit settings.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="imageSource">The image source to display.</param>
        /// <param name="useShadow">Enable a drop shadow if the image may blend with the background.</param>
        /// <returns>void</returns>
        [STAThread]
        public static void Show(string message, string title, ImageSource imageSource, bool useShadow)
        {
            CairoMessage msgDialog = new CairoMessage
            {
                Message = message,
                Title = title,
                Buttons = MessageBoxButton.OK
            };

            msgDialog.MessageIconImage.Source = imageSource;

            if (useShadow)
            {
                msgDialog.MessageIconImage.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 2,
                    BlurRadius = 10,
                    Opacity = 0.5
                };
            }

            msgDialog.Show();
        }

        /// <summary>
        /// Displays the Cairo Message Dialog with OK/Cancel buttons, implicit settings, custom image and button text.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="image">The path to the image for the dialog.</param>
        /// <param name="OkButtonText">The text for the OK button.</param>
        /// <param name="CancelButtonText">The text for the cancel button.</param>
        /// <param name="resultCallback">The delegate to execute upon user action.</param>
        /// <returns>void</returns>
        public static void ShowOkCancel(string message, string title, CairoMessageImage image, string OkButtonText, string CancelButtonText, DialogResultDelegate resultCallback)
        {
            if (string.IsNullOrEmpty(CancelButtonText))
            {
                CancelButtonText = Localization.DisplayString.sInterface_Cancel;
            }

            if(string.IsNullOrEmpty(OkButtonText))
            {
                OkButtonText = Localization.DisplayString.sInterface_OK;
            }

            CairoMessage msgDialog = new CairoMessage();
            msgDialog.Message = message;
            msgDialog.Title = title;
            msgDialog.Buttons = MessageBoxButton.OKCancel;
            msgDialog.Image = image;
            msgDialog.ResultCallback = resultCallback;
            msgDialog.OkButton.Content = OkButtonText;
            msgDialog.CancelButton.Content = CancelButtonText;
            
            msgDialog.Show();
        }
        #endregion

        private bool IsModal()
        {
            return (bool)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsModal())
                DialogResult = true;
            else
                Close();

            ResultCallback?.Invoke(true);
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsModal())
                DialogResult = false;
            else
                Close();

            ResultCallback?.Invoke(false);
        }

        private void messageWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
