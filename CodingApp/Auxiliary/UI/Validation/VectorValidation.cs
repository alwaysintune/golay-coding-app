using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CodingApp.Auxiliary.UI.Validation {
    public class VectorValidation : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value != null) {
                if (!Regex.IsMatch(value as string, "^[01]{12}$"))
                    return new ValidationResult(false, "Enter a bit string of length 12");
            }

            return new ValidationResult(true, null);

        }
    }
}
