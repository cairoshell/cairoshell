// This file is part of CoderLine NeoGeniX.Skinning.
//
// CoderLine NeoGeniX.Skinning is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// CoderLine NeoGeniX.Skinning is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with CoderLine NeoGeniX.Skinning.  If not, see <http://www.gnu.org/licenses/>.
//
// (C) 2010 Daniel Kuschny, (http://www.coderline.net)
using System.Drawing;
using System.ComponentModel;
using NeoGeniX.Skinning.Win32;

namespace NeoGeniX.Skinning
{
    /// <summary>
    /// This class represents a caption button used for SysCommands like Close, Maximize and Minimize.
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    internal class CaptionButton : Component, INotifyPropertyChanged
    {
        #region Fields
        private Rectangle _bounds;
        private bool _pressed;
        private bool _hovered;
        private bool _enabled;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the hit test result of this button.
        /// </summary>
        /// <value>The hit test result of this button.</value>
        public HitTest HitTest { get; private set; }


        /// <summary>
        /// Gets or sets the bounds where this button gets painted into.
        /// </summary>
        /// <value>The paint bounds.</value>
        public Rectangle Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                if (value == _bounds) return;
                _bounds = value;
                PropertyChangedEventArgs e = new PropertyChangedEventArgs("Bounds");
                OnPropertyChanged(e);
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is hovered by the cursor.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is hovered; otherwise, <c>false</c>.
        /// </value>
        public bool Hovered
        {
            get
            {
                return _hovered;
            }
            set
            {
                if (value == _hovered) return;
                _hovered = value;
                PropertyChangedEventArgs e = new PropertyChangedEventArgs("Hovered");
                OnPropertyChanged(e);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is pressed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pressed; otherwise, <c>false</c>.
        /// </value>
        public bool Pressed
        {
            get
            {
                return _pressed;
            }
            set
            {
                if (value == _pressed) return;
                _pressed = value;
                PropertyChangedEventArgs e = new PropertyChangedEventArgs("Pressed");
                OnPropertyChanged(e);
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CaptionButton"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (value == _enabled) return;
                _enabled = value;
                PropertyChangedEventArgs e = new PropertyChangedEventArgs("Enabled");
                OnPropertyChanged(e);
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CaptionButton"/> class.
        /// </summary>
        /// <param name="hitTest">The hit test result of this button.</param>
        public CaptionButton(HitTest hitTest)
        {
            HitTest = hitTest;
            _hovered = false;
            _enabled = true;
            _pressed = false;
        }
        #endregion


        #region Events
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        #endregion
    }
}