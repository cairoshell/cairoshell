using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace CairoDesktop.SupportingClasses
{
    public class DirectoryPathValidationRule : ValidationRule
    {
        public bool CheckInvalidPathChars { get; set; }

        public bool CheckPathLength { get; set; }

        public bool RequireDirectoryExists { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is string)
                {
                    string stringValue = (string)value;

                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        return new ValidationResult(false, "Path is Blank");
                    }

                    // Check for Invalid Path Chars.
                    if (CheckInvalidPathChars && stringValue.Any(Path.GetInvalidPathChars().Contains))
                    {
                        return new ValidationResult(false, "Invalid Path Chars.");
                    }

                    if (CheckPathLength)
                    {
                        try
                        {
                            string fullPath = Path.GetFullPath(stringValue);
                        }
                        catch (PathTooLongException pathTooLongException)
                        {
                            return new ValidationResult(false, "Path too long: " + pathTooLongException.Message);

                        }
                    }

                    if (RequireDirectoryExists && !Directory.Exists(stringValue))
                    {
                        return new ValidationResult(false, "Directory doesn't exist.");
                    }

                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, "Unrecognized Value.");
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, "General Exception: " + ex.Message);
            }
        }
    }
}