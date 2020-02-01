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

namespace NeoGeniX.Skinning
{
/// <summary>
/// Lists all alignments
/// </summary>
internal enum DrawingAlign
{
    /// <summary>
    /// Left if horizontal, Top if vertical
    /// </summary>
    Near,
    /// <summary>
    /// Centered
    /// </summary>
    Center,
    /// <summary>
    /// Right if horizontal, Bottom if horizontal
    /// </summary>
    Far,
    /// <summary>
    /// Stretch to width / height
    /// </summary>
    Stretch
}


/// <summary>
/// This class contains all data for painting an image strip.
/// </summary>
internal class ImageStrip : IDisposable
{
    #region Fields

    private Bitmap _bitmap;
    private Size _imageSize;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the image which contains the full image strip..
    /// </summary>
    /// <value>The image which contains the full image strip.</value>
    public Bitmap Image
    {
        get
        {
            return _bitmap;
        }
        set
        {
            if (value == _bitmap) return;
            // Delete old data
            if (_bitmap != null)
                _bitmap.Dispose();
            _bitmap = value;
        }
    }

    /// <summary>
    /// Gets or sets the size of the single images in the image strip.
    /// </summary>
    /// <value>The size of images in the image strip.</value>
    public Size ImageSize
    {
        get
        {
            return _imageSize;
        }
        set
        {
            if (value == _imageSize) return;
            _imageSize = value;
        }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the images in the image strip are aligned vertical.
    /// </summary>
    /// <value><c>true</c> if the images in the image strip are aligned vertical; otherwise, <c>false</c>.</value>
    public bool Vertical { get; set; }

    #endregion

    #region Constructor & Destructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageStrip"/> class.
    /// </summary>
    public ImageStrip()
    {
        _imageSize = new Size(16, 16);
    }

    public ImageStrip(bool vertical, Size imageSize, Bitmap image)
        :this()
    {
        Vertical = vertical;
        ImageSize = imageSize;
        Image = image;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (_bitmap != null)
        {
            _bitmap.Dispose();
            _bitmap = null;
        }
    }
    #endregion

    #region Paint Methods
    /// <summary>
    /// Draws a part of the image at the specified index into the given destination.
    /// </summary>
    /// <param name="g">The graphics to draw into.</param>
    /// <param name="index">The index of the image to draw.</param>
    /// <param name="destRect">The destination bounds to draw into.</param>
    /// <param name="partRect">The part of the image which should be drawn.</param>
    private void Draw(Graphics g, int index, Rectangle destRect, Rectangle partRect)
    {
        Rectangle srcRect;
        if (Vertical)
            srcRect = new Rectangle(partRect.Left, index * ImageSize.Height + partRect.Top,
                                    partRect.Width, partRect.Height);
        else
            srcRect = new Rectangle(index * ImageSize.Width + partRect.Left, partRect.Top,
                                    partRect.Width, partRect.Height);

        g.DrawImage(_bitmap, destRect, srcRect, GraphicsUnit.Pixel);
    }

    /// <summary>
    /// Draws the image using the specified data and alignes the image using the 
    /// specified alignments within the specified bounds. 
    /// </summary>
    /// <param name="g">The graphics to draw into.</param>
    /// <param name="index">The index of the image to draw.</param>
    /// <param name="destRect">The target bounds to draw into.</param>
    /// <param name="srcRect">The bounds of the image part to draw</param>
    /// <param name="horizontal">The horizontal alignment of the part image within the specified bounds.</param>
    /// <param name="vertical">The vertical alignment of the part image within the specified bounds.</param>
    /// <returns></returns>
    public void Draw(Graphics g, int index, Rectangle destRect, Rectangle srcRect,
        DrawingAlign horizontal, DrawingAlign vertical)
    {
        if (srcRect.IsEmpty)
            srcRect = new Rectangle(0, 0, ImageSize.Width, ImageSize.Height);

        if (srcRect.IsEmpty)
            return;

        Rectangle dest = destRect;

        // If no stretching adjust destination bounds
        if (vertical != DrawingAlign.Stretch)
        {
            bool update = true;

            switch (vertical)
            {
                // to top
                case DrawingAlign.Near:
                    dest.Height = srcRect.Height;
                    break;
                // at bottom
                case DrawingAlign.Far:
                    dest.Y = dest.Bottom - srcRect.Height;
                    break;
                // center 
                case DrawingAlign.Center:
                    dest.Y += (dest.Height - srcRect.Height) / 2;
                    dest.Height = srcRect.Height;
                    break;
                default:
                    update = false;
                    break;
            }

            // atjust rectangle
            if (update)
            {
                Rectangle rt = dest;
                dest.Intersect(destRect);

                if (dest.Height > 0 && dest.Height != srcRect.Height)
                {
                    srcRect.Y += dest.Y - rt.Y;
                    srcRect.Height = Math.Min(dest.Height, srcRect.Height);
                }
            }
        }


        // Calculate real destination bounds if no stretching
        if (horizontal != DrawingAlign.Stretch)
        {
            bool update = true;

            switch (horizontal)
            {
                // Keep position adjust width
                case DrawingAlign.Near:
                    dest.Width = srcRect.Width;
                    break;
                // Align to the right 
                case DrawingAlign.Far:
                    dest.X = dest.Right - srcRect.Width;
                    dest.Width = srcRect.Width;
                    break;
                // Calculate middle 
                case DrawingAlign.Center:
                    dest.X += (dest.Width - srcRect.Width) / 2;
                    dest.Width = srcRect.Width;
                    break;
                default:
                    update = false;
                    break;
            }

            // Adjust rectangles
            if (update)
            {
                Rectangle rt = dest;
                dest.Intersect(destRect);

                if (dest.Width > 0 && dest.Width != srcRect.Width)
                {
                    srcRect.X += dest.X - rt.X;
                    srcRect.Width = Math.Min(dest.Width, srcRect.Width);
                }
            }
        }

        // Can Paint?
        if (!srcRect.IsEmpty && !dest.IsEmpty)
            Draw(g, index, dest, srcRect);
    } 
    #endregion
}
}
