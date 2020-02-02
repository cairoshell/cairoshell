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
using System.Windows.Forms.VisualStyles;
using NeoGeniX.Skinning.Win32;

namespace NeoGeniX.Skinning
{
    /// <summary>
    /// This class provides some methods which provide drawing specific data.
    /// </summary>
    public static class FormExtenders
    {
        /// <summary>
        /// Gets a value indicating if the maximize box needs to be drawn on the specified form.
        /// </summary>
        /// <param name="form">The form to check.</param>
        /// <returns></returns>
        public static bool IsDrawMaximizeBox(Form form)
        {
            return form.MaximizeBox && form.FormBorderStyle != FormBorderStyle.SizableToolWindow &&
                  form.FormBorderStyle != FormBorderStyle.FixedToolWindow;
        }
        /// <summary>
        /// Gets a value indicating if the minimize box needs to be drawn on the specified form.
        /// </summary>
        /// <param name="form">The form to check .</param>
        /// <returns></returns>
        public static bool IsDrawMinimizeBox(Form form)
        {
            return form.MinimizeBox && form.FormBorderStyle != FormBorderStyle.SizableToolWindow &&
                   form.FormBorderStyle != FormBorderStyle.FixedToolWindow;
        }

        /// <summary>
        /// Calculates the border size for the given form.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        public static Size GetBorderSize(Form form)
        {
            Size border = new Size(0, 0);

            // Check for Caption
            Int32 style = Win32Api.GetWindowLong(form.Handle, GWLIndex.GWL_STYLE);
            bool caption = (style & (int)(WindowStyles.WS_CAPTION)) != 0;
            int factor = SystemInformation.BorderMultiplierFactor - 1;

            OperatingSystem system = Environment.OSVersion;
            bool isVista = system.Version.Major >= 6 && VisualStyleInformation.IsEnabledByUser;
                switch (form.FormBorderStyle)
                {
                    case FormBorderStyle.FixedToolWindow:
                    case FormBorderStyle.FixedSingle:
                    case FormBorderStyle.FixedDialog:
                        border = SystemInformation.FixedFrameBorderSize;
                        break;
                    case FormBorderStyle.SizableToolWindow:
                    case FormBorderStyle.Sizable:
                        if (isVista && form.MdiParent == null)
                            border = SystemInformation.FrameBorderSize;
                        else
                            border = SystemInformation.FixedFrameBorderSize +
                                (caption ? SystemInformation.BorderSize + new Size(factor, factor)
                                    : new Size(factor, factor));
                        break;
                    case FormBorderStyle.Fixed3D:
                        border = SystemInformation.FixedFrameBorderSize + SystemInformation.Border3DSize;
                        break;
                }

            return border;
        }

        /// <summary>
        /// Gets the size for <see cref="CaptionButton"/> the given form.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        public static Size GetCaptionButtonSize(Form form)
        {
            Size buttonSize = form.FormBorderStyle != FormBorderStyle.SizableToolWindow &&
                              form.FormBorderStyle != FormBorderStyle.FixedToolWindow
                                  ? SystemInformation.CaptionButtonSize
                                  : SystemInformation.ToolWindowCaptionButtonSize;
            // looks better with this height
            buttonSize.Height--;
            return buttonSize;
        }

        /// <summary>
        /// Gets the height of the caption.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        public static int GetCaptionHeight(Form form)
        {
            return form.FormBorderStyle != FormBorderStyle.SizableToolWindow &&
                   form.FormBorderStyle != FormBorderStyle.FixedToolWindow
                       ? SystemInformation.CaptionHeight + 2
                       : SystemInformation.ToolWindowCaptionHeight + 1;
        }

        /// <summary>
        /// Gets a value indicating whether the given form has a system menu.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        public static bool HasMenu(Form form)
        {
            return form.FormBorderStyle == FormBorderStyle.Sizable || form.FormBorderStyle == FormBorderStyle.Fixed3D ||
                    form.FormBorderStyle == FormBorderStyle.FixedSingle;
        }

        /// <summary>
        /// Gets the screen rect of the given form
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        public static Rectangle GetScreenRect(Form form)
        {
            return (form.Parent != null) ? form.Parent.RectangleToScreen(form.Bounds) : form.Bounds;
        }
    }
}
