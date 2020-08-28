using System;
using System.Globalization;
using System.Windows.Controls;

namespace CairoDesktop.SupportingClasses
{
    public class DateTimeFormatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is string @string)
                {
                    var foo = DateTime.Now.ToString(@string);
                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, "Unrecognized Value.");
                }
            }
            catch (FormatException formatException)
            {
                return new ValidationResult(false, "Format Invalid: " + formatException.Message);
            }
            catch (ArgumentOutOfRangeException argumentOutOfRangeException)
            {
                return new ValidationResult(false, "Out of Range: " + argumentOutOfRangeException.Message);
            }
        }
    }
}