using System.Globalization;
using System.Windows.Controls;

namespace CodingApp.Auxiliary.UI.Validation {
    public class ProbabilityValidation : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value != null) {
                if (!double.TryParse(value.ToString().Replace(",", "."),
                                     NumberStyles.Any,
                                     CultureInfo.InvariantCulture,
                                     out double input) ||
                    input < 0.0d ||
                    input > 1.0d)
                    return new ValidationResult(false, "Enter a real number in range of [0,1]");
            }

            return new ValidationResult(true, null);

        }
    }
}