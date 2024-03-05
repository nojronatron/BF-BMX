using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                return new ValidationResult(false, $"Use at least {MinimumCharacters} characters");
            }
            else
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(charString);
                if (!directoryInfo.Exists)
                {
                    return new ValidationResult(false, "Directory does not exist.");
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
        }
    }
}
