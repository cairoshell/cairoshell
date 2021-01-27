namespace CairoDesktop.Application.Structs
{
    public struct MenuBarDimensions
    {
        /// <summary>
        /// ABE_LEFT = 0,
        /// ABE_TOP = 1,
        /// ABE_RIGHT = 2,
        /// ABE_BOTTOM = 3
        /// </summary>
        public int ScreenEdge;

        public double DpiScale;
        public double Height;
        public double Width;
        public double Left;
        public double Top;
    }
}