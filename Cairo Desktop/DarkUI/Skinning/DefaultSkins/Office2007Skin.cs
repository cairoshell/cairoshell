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
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Resources;
using NeoGeniX.Skinning.Painting;
using NeoGeniX.Skinning.Win32;

namespace NeoGeniX.Skinning
{
    public class Office2007Skin : SkinBase
    {
        #region Fields
        private ControlPaintHelper _formCaption;
        private ControlPaintHelper _formBorder;

        private ControlPaintHelper _formCaptionButton;
        private ControlPaintHelper _formCaptionButtonSmall;

        private ImageStrip _formCloseIcon;
        private ImageStrip _formCloseIconSmall;

        private ImageStrip _formRestoreIcon;
        private ImageStrip _formRestoreIconSmall;

        private ImageStrip _formMaximizeIcon;
        private ImageStrip _formMaximizeIconSmall;

        private ImageStrip _formMinimizeIcon;
        private ImageStrip _formMinimizeIconSmall;

        private Color _formActiveTitleColor;
        private Color _formInactiveTitleColor;
        private bool _formIsTextCentered;

        protected ResourceManager _currentManager;
        #endregion

        #region Properties
        public Office2007Style OfficeStyle { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Office2007Skin"/> class.
        /// </summary>
        /// <param name="style">The style.</param>
        public Office2007Skin(Office2007Style style)
        {
            OfficeStyle = style;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the button paint indices.
        /// </summary>
        /// <param name="button">The button paint info.</param>
        /// <param name="active">A value indicating whether the button is active.</param>
        /// <param name="buttonIndex">Index of the button icon image strip.</param>
        /// <param name="rendererIndex">Index of the button background image strip.</param>
        private static void GetButtonData(CaptionButtonPaintData button, bool active, out int buttonIndex, out int rendererIndex)
        {
            if (!button.Enabled)
            {
                rendererIndex = -1;
                buttonIndex = 4;
            }
            else if (button.Pressed)
            {
                buttonIndex = active ? 2 : 3;
                rendererIndex = 1;
            }
            else if (button.Hovered)
            {
                buttonIndex = active ? 1 : 3;
                rendererIndex = 0;
            }
            else
            {
                buttonIndex = active ? 0 : 3;
                rendererIndex = -1;
            }
        }

        /// <summary>
        /// Called when the skin is loaded.
        /// </summary>
        public override void OnLoad()
        {
            try
            {
                LoadResourceManager();
                XmlDocument skinDef = new XmlDocument();
                skinDef.LoadXml(_currentManager.GetString("SkinDefinition"));


                XmlElement elm = skinDef.DocumentElement;
                XmlNode form = elm["Form"];
                XmlNode captionNode = form["Caption"];
                XmlNode normalButton = captionNode["NormalButton"];
                XmlNode smallButton = captionNode["SmallButton"];

                // Background
                _formBorder = new ControlPaintHelper(PaintHelperData.Read(form["Border"], _currentManager, "FormBorder"));
                _formCaption = new ControlPaintHelper(PaintHelperData.Read(captionNode["Background"], _currentManager, "FormCaption"));

                // Big Buttons
                Size imageSize = PaintHelperData.StringToSize(normalButton["IconSize"].InnerText);

                _formCloseIcon = new ImageStrip(true, imageSize, (Bitmap)_currentManager.GetObject("CloseIcon"));
                _formRestoreIcon = new ImageStrip(true, imageSize, (Bitmap)_currentManager.GetObject("RestoreIcon"));
                _formMaximizeIcon = new ImageStrip(true, imageSize, (Bitmap)_currentManager.GetObject("MaximizeIcon"));
                _formMinimizeIcon = new ImageStrip(true, imageSize, (Bitmap)_currentManager.GetObject("MinimizeIcon"));
                _formCaptionButton = new ControlPaintHelper(PaintHelperData.Read(normalButton, _currentManager, "FormCaptionButton"));

                // Small Buttons
                imageSize = PaintHelperData.StringToSize(smallButton["IconSize"].InnerText);

                _formCloseIconSmall = new ImageStrip(true, imageSize, (Bitmap)_currentManager.GetObject("CloseIconSmall"));
                _formRestoreIconSmall = new ImageStrip(true, imageSize, (Bitmap)_currentManager.GetObject("RestoreIconSmall"));
                _formMaximizeIconSmall = new ImageStrip(true, imageSize, (Bitmap)_currentManager.GetObject("MaximizeIconSmall"));
                _formMinimizeIconSmall = new ImageStrip(true, imageSize, (Bitmap)_currentManager.GetObject("MinimizeIconSmall"));
                _formCaptionButtonSmall = new ControlPaintHelper(PaintHelperData.Read(smallButton, _currentManager, "FormCaptionButton"));

                // General Infos
                _formActiveTitleColor = PaintHelperData.StringToColor(form["ActiveCaption"].InnerText);
                _formInactiveTitleColor = PaintHelperData.StringToColor(form["InactiveCaption"].InnerText);
                _formIsTextCentered = PaintHelperData.StringToBool(form["CenterCaption"].InnerText);

            }
            catch (Exception e)
            {
                throw new ApplicationException("Invalid SkinDefinition XML", e);
            }
        }

        /// <summary>
        /// Loads the resource manager assigned to the current <see cref="OfficeStyle"/>.
        /// </summary>
        protected virtual void LoadResourceManager()
        {
            switch (OfficeStyle)
            {
                case Office2007Style.LunaBlue:
                    _currentManager = NeoGeniX.Skinning.DefaultSkins.Office2007Luna.ResourceManager;
                    break;
                case Office2007Style.Silver:
                    _currentManager = NeoGeniX.Skinning.DefaultSkins.Office2007Silver.ResourceManager;
                    break;
                case Office2007Style.ObsidianBlack:
                    _currentManager = NeoGeniX.Skinning.DefaultSkins.Office2007Obsidian.ResourceManager;
                    break;
            }
        }

        /// <summary>
        /// Called when the form region needs to be set.
        /// </summary>
        /// <param name="form">The form whose region need to be set.</param>
        /// <param name="size">The size of the form which should be used for region calculation.</param>
        public override void OnSetRegion(Form form, Size size)
        {
            if (form == null)
                return;

            // Create a rounded rectangle using Gdi
            Size cornerSize = new Size(9, 9);
            IntPtr hRegion = Win32Api.CreateRoundRectRgn(0, 0, size.Width + 1, size.Height + 1, cornerSize.Width, cornerSize.Height);
            Region region = Region.FromHrgn(hRegion);
            form.Region = region;
            region.ReleaseHrgn(hRegion);
        }


        /// <summary>
        /// Called when the non client area of the form needs to be painted.
        /// </summary>
        /// <param name="form">The form which gets drawn.</param>
        /// <param name="paintData">The paint data to use for drawing.</param>
        /// <returns><code>true</code> if the original painting should be suppressed, otherwise <code>false</code></returns>
        public override bool OnNcPaint(Form form, SkinningFormPaintData paintData)
        {
            if (form == null) return false;

            bool isMaximized = form.WindowState == FormWindowState.Maximized;
            bool isMinimized = form.WindowState == FormWindowState.Minimized;

            // prepare bounds
            Rectangle windowBounds = paintData.Bounds;
            windowBounds.Location = Point.Empty;

            Rectangle captionBounds = windowBounds;
            Size borderSize = paintData.Borders;
            captionBounds.Height = borderSize.Height + paintData.CaptionHeight;

            Rectangle textBounds = captionBounds;
            Rectangle iconBounds = captionBounds;
            iconBounds.Inflate(-borderSize.Width, 0);
            iconBounds.Y += borderSize.Height;
            iconBounds.Height -= borderSize.Height;

            // Draw Caption
            bool active = paintData.Active;
            _formCaption.Draw(paintData.Graphics, captionBounds, active ? 0 : 1);

            // Paint Icon
            if (paintData.HasMenu && form.Icon != null)
            {
                iconBounds.Size = paintData.IconSize;
                Icon tmpIcon = new Icon(form.Icon, paintData.IconSize);
                iconBounds.Y = captionBounds.Y + (captionBounds.Height - iconBounds.Height) / 2;
                paintData.Graphics.DrawIcon(tmpIcon, iconBounds);
                textBounds.X = iconBounds.Right;
                iconBounds.Width -= iconBounds.Right;
            }

            // Paint Icons
            foreach (CaptionButtonPaintData data in paintData.CaptionButtons)
            {
                ControlPaintHelper painter = paintData.IsSmallCaption ? _formCaptionButtonSmall : _formCaptionButton;

                // Get Indices for imagestrip
                int iconIndex;
                int backgroundIndex;
                GetButtonData(data, paintData.Active, out iconIndex, out backgroundIndex);

                // get imageStrip for button icon
                ImageStrip iconStrip;
                switch (data.HitTest)
                {
                    case HitTest.HTCLOSE:
                        iconStrip = paintData.IsSmallCaption ? _formCloseIconSmall : _formCloseIcon;
                        break;
                    case HitTest.HTMAXBUTTON:
                        if (isMaximized)
                            iconStrip = paintData.IsSmallCaption ? _formRestoreIconSmall : _formRestoreIcon;
                        else
                            iconStrip = paintData.IsSmallCaption ? _formMaximizeIconSmall : _formMaximizeIcon;
                        break;
                    case HitTest.HTMINBUTTON:
                        if (isMinimized)
                            iconStrip = paintData.IsSmallCaption ? _formRestoreIconSmall : _formRestoreIcon;
                        else
                            iconStrip = paintData.IsSmallCaption ? _formMinimizeIconSmall : _formMinimizeIcon;
                        break;
                    default:
                        continue;
                }

                // draw background
                if (backgroundIndex >= 0)
                    painter.Draw(paintData.Graphics, data.Bounds, backgroundIndex);

                // draw Icon 
                Rectangle b = data.Bounds;
                b.Y += 1;
                if (iconIndex >= 0)
                    iconStrip.Draw(paintData.Graphics, iconIndex, b, Rectangle.Empty,
                                   DrawingAlign.Center, DrawingAlign.Center);
                // Ensure textbounds
                textBounds.Width -= data.Bounds.Width;
            }

            // draw text
            if (!string.IsNullOrEmpty(paintData.Text) && !textBounds.IsEmpty)
            {
                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsClipping;
                if (_formIsTextCentered)
                    flags = flags | TextFormatFlags.HorizontalCenter;
                Font font = paintData.IsSmallCaption ? SystemFonts.SmallCaptionFont : SystemFonts.CaptionFont;
                TextRenderer.DrawText(paintData.Graphics, paintData.Text, font, textBounds,
                    paintData.Active ? _formActiveTitleColor : _formInactiveTitleColor, flags);
            }

            // exclude caption area from painting
            Region region = paintData.Graphics.Clip;
            region.Exclude(captionBounds);
            paintData.Graphics.Clip = region;

            // Paint borders and corners
            _formBorder.DrawFrame(paintData.Graphics, windowBounds, paintData.Active ? 0 : 1);

            paintData.Graphics.ResetClip();
            return true;
        } 
        #endregion
    }

    /// <summary>
    /// Lists all available Office 2007 Color Schemes
    /// </summary>
    public enum Office2007Style
    {
        /// <summary>
        /// The blue color scheme
        /// </summary>
        LunaBlue,
        /// <summary>
        /// The black color scheme
        /// </summary>
        ObsidianBlack,
        /// <summary>
        /// The silver color scheme
        /// </summary>
        Silver
    }

}
