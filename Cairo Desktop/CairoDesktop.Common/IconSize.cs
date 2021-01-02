using System;

namespace CairoDesktop.Common
{
    public class IconSize
    {
        public enum Sizes : int
        {
            ///<summary>32 pixels</summary>
            Large = 0,
            ///<summary>16 pixels</summary>
            Small = 1,
            ///<summary>24 pixels</summary>
            Medium = 10,
            ///<summary>48 pixels</summary>
            ExtraLarge = 2,
            ///<summary>96 pixels</summary>
            Jumbo = 4
        }

        public static Sizes ParseSize(int size)
        {
            if (Enum.IsDefined(typeof(Sizes), size)) return (Sizes)size;
            else return Sizes.Small;
        }

        public static int GetSize(int size)
        {
            return GetSize(ParseSize(size));
        }

        public static int GetSize(Sizes size)
        {
            switch (size)
            {
                case Sizes.Large:
                    return 32;
                case Sizes.Small:
                    return 16;
                case Sizes.Medium:
                    return 24;
                case Sizes.ExtraLarge:
                    return 48;
                case Sizes.Jumbo:
                    return 96;
                default:
                    return 16;
            }
        }
    }
}