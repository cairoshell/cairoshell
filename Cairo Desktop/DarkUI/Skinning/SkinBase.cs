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
using System.Windows.Forms;
using NeoGeniX.Skinning.Painting;

namespace NeoGeniX.Skinning
{
    /// <summary>
    /// Specifies all available default skins.
    /// </summary>
    public enum DefaultSkin
    {
        Office2007Luna,
        Office2007Obsidian,
        Office2007Silver,
        NeoGeniX,
    }

    /// <summary>
    /// This is the base class for skins. 
    /// </summary>
    public abstract class SkinBase
    {
        #region General

        /// <summary>
        /// Called when the skin is loaded.
        /// </summary>
        public virtual void OnLoad()
        {
            // Optional Override
        } 

        #endregion

        #region Window Skinning

        /// <summary>
        /// Called when the form region needs to be set.
        /// </summary>
        /// <param name="form">The form whose region need to be set.</param>
        /// <param name="size">The size of the form which should be used for region calculation.</param>
        public abstract void OnSetRegion(Form form, Size size);

        /// <summary>
        /// Called when the non client area of the form needs to be painted.
        /// </summary>
        /// <param name="form">The form which gets drawn.</param>
        /// <param name="paintData">The paint data to use for drawing.</param>
        /// <returns><code>true</code> if the original painting should be suppressed, otherwise <code>false</code></returns>
        public abstract bool OnNcPaint(Form form, SkinningFormPaintData paintData);

        #endregion
    }
}
