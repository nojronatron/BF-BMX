using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace BFBMX.Desktop.XamlValidators
{
    public class FilepathRule : ValidationRule
    {
        public int MinimumCharacters { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? charString = value as string;

            if (charString is null || charString.Length < MinimumCharacters)
            {
                return new ValidationResult(false, $"{MinimumCharacters} characters required");
            }
            else
            {
                DirectoryInfo directoryInfo = new(charString);

                if (!directoryInfo.Exists)
                {
                    return new ValidationResult(false, "Invalid directory.");
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
        }
    }
}
