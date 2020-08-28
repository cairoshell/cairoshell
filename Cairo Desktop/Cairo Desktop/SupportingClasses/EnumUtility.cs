using System;

namespace CairoDesktop.SupportingClasses
{
    public static class EnumUtility
    {
        public static bool TryCast<T>(object value, out T result, T defaultValue) where T : struct
        {
            result = defaultValue;
            try
            {
                // TODO: look at doing cast without throwing exception.
                // typeof(T).IsAssignableFrom(value.GetType());
                result = (T)value;
                return true;
            }
            catch (Exception ex)
            {
            }

            return false;
        }
    }
}