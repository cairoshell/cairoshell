using System.Drawing;

namespace CairoDesktop.Common.ExtensionMethods;

public static class SystemDrawingGeometryExtensions
{
    public static Point Center(this Rectangle rectangle)
        => new(rectangle.Left + rectangle.Width / 2, rectangle.Top + rectangle.Height / 2);
}
