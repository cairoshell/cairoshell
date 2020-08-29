using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace CairoDesktop.SupportingClasses
{
    public class FilePathValidationRule : ValidationRule
    {
        public bool CheckInvalidPathChars { get; set; }

        public bool CheckPathLength { get; set; }

        public bool RequireFileExists { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is string stringValue)
                {
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        return new ValidationResult(false, "Path is Blank");
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

                    // Check for Invalid Path Chars.
                    if (CheckInvalidPathChars)
                    {
                        if (Path.GetDirectoryName(stringValue).Any(Path.GetInvalidPathChars().Contains) && // TODO: Should invalid chars in dir and filename be an OR?
                            Path.GetFileName(stringValue).Any(Path.GetInvalidPathChars().Contains))
                        {
                            return new ValidationResult(false, "Invalid Path Chars.");
                        }
                    }

                    if (RequireFileExists && !File.Exists(stringValue))
                    {
                        return new ValidationResult(false, "File doesn't exist.");
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