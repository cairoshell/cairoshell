using DarkUI.Config;
using System;
using System.Drawing;

namespace DarkUI.Controls
{
    public class DarkListItem
    {
        #region Event Region

        public event EventHandler TextChanged;

        #endregion

        #region Field Region

        private string _text;

        #endregion

        #region Property Region

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;

                if (TextChanged != null)
                    TextChanged(this, new EventArgs());
            }
        }

        public Rectangle Area { get; set; }

        public Color TextColor { get; set; }

        public FontStyle FontStyle { get; set; }

        public object Tag { get; set; }

        #endregion

        #region Constructor Region

        public DarkListItem()
        {
            TextColor = Colors.LightText;
            FontStyle = FontStyle.Regular;
        }

        public DarkListItem(string text)
            : this()
        {
            Text = text;
        }

        #endregion
    }
}
