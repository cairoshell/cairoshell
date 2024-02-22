using System;
using System.Globalization;
using System.Windows.Controls;

namespace CairoDesktop.SupportingClasses
{
    public class IntegerValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is string @string)
                {
                    if (!int.TryParse(@string, out int result))
                    {
                        return new ValidationResult(false, "Unrecognized value.");
                    }

                    if (result < 0)
                    {
                        return new ValidationResult(false, "Negative value.");
                    }

                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, "Unrecognized value.");
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Error: " + e.Message);
            }
        }
    }
}