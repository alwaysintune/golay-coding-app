using System.Globalization;
using System.Windows.Controls;

namespace CodingApp.Auxiliary.UI.Validation {
    public class MatrixValidation : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value != null) {
                if (!int.TryParse(value.ToString(), out int input) || input < 0 || input > 1)
                    return new ValidationResult(false, "Only 0 or 1 allowed");
            }

            return new ValidationResult(true, null);

        }
    }
}
