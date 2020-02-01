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
using System;
using System.Collections;
using System.Drawing;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Windows.Forms;

namespace NeoGeniX.Skinning
{
    /// <summary>
    /// This component provides the functionality for skinning a form.
    /// </summary>
    /// <remarks>
    /// Add this component to your form and assign it to the <see cref="ParentForm"/> Property.
    /// </remarks>
    public class SkinningManager : Component
    {
        private DefaultSkin _defaultSkin;
        private Form _parentForm;
        private SkinningForm _form;
        private SkinBase _currentSkin;

        /// <summary>
        /// Gets or sets the default skin to load on startup.
        /// </summary>
        /// <value>The current default style.</value>
        [Category("Appearance")]
        [Description("Gets or sets the default skin to load on startup")]
        public DefaultSkin DefaultSkin
        {
            get { return _defaultSkin; }
            set
            {
                if(_defaultSkin == value) return;
                _defaultSkin = value;
                LoadDefaultSkin(_defaultSkin);
                if(_form != null)
                    _form.Invalidate();
            }
        }

        /// <summary>
        /// Gets the current skin.
        /// </summary>
        /// <value>The current skin.</value>
        public SkinBase CurrentSkin
        {
            get
            {
                return _currentSkin ?? LoadDefaultSkin();
            }
        }


        #region DefaultSkin Loaders

        /// <summary>
        /// Loads a default skin.
        /// </summary>
        /// <param name="skin">The skin to load.</param>
        public void LoadDefaultSkin(DefaultSkin skin)
        {
            // Dont load the skin imemdialy. Wait for first access.
            // This allows a custom skin to be loaded before the default skin
            _currentSkin = null;
            _defaultSkin = skin;
        }

        /// <summary>
        /// Loads a skin implementation
        /// </summary>
        /// <param name="skin">The skin.</param>
        public void LoadSkin(SkinBase skin)
        {
            _currentSkin = skin;
            _currentSkin.OnLoad();
        }

        /// <summary>
        /// Loads the default skin.
        /// </summary>
        /// <returns></returns>
        private SkinBase LoadDefaultSkin()
        {
            // skin implementation
            switch (_defaultSkin)
            {
                case DefaultSkin.Office2007Luna:
                    _currentSkin = new Office2007Skin(Office2007Style.LunaBlue);
                    break;
                case DefaultSkin.Office2007Obsidian:
                    _currentSkin = new Office2007Skin(Office2007Style.ObsidianBlack);
                    break;
                case DefaultSkin.Office2007Silver:
                    _currentSkin = new Office2007Skin(Office2007Style.Silver);
                    break;
            }

            // load skin
            LoadSkin(_currentSkin);
            return _currentSkin;
        }

        #endregion

        /// <summary>
        /// Gets or sets the parent form which should be skinned.
        /// </summary>
        /// <value>The parent form.</value>
        [Category("Behavior")]
        [Description("Gets or sets the parent form which should be skinned")]
        public Form ParentForm
        {
            get { return _parentForm; }
            set
            {
                if(_parentForm == value) return;
                if(_parentForm != null && !DesignMode)
                {
                    _parentForm.Disposed -= OnParentFormDisposed;
                }
                _parentForm = value;
                // Start skinning 
                if (_parentForm != null && !DesignMode)
                {
                    _form = new SkinningForm(_parentForm, this);
                    _parentForm.Disposed += OnParentFormDisposed;
                }
            }
        }

        private void OnParentFormDisposed(object sender, EventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Gets or sets a value indicating whether we are currently in design mode.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this we are in design mode; otherwise, <c>false</c>.
        /// </value>
        internal static bool IsDesignMode { get; set; }
    }

    /// <summary>
    /// This designer is used to determine the design mode state
    /// </summary>
    internal class SkinningManagerDesigner : ComponentDesigner
    {
        public override void Initialize(IComponent component)
        {
            SkinningManager.IsDesignMode = true;
            base.Initialize(component);
        }
    }
}
