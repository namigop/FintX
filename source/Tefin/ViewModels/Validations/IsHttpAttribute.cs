using System.ComponentModel.DataAnnotations;

namespace Tefin.ViewModels.Validations;

public class IsHttpAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null) {
            return new ValidationResult("Value cannot be null. Enter a valid http address");
        }

        try {
            var uri = new Uri(value.ToString()!);
            if (!uri.Scheme.StartsWith("http")) {
                return new ValidationResult("Enter a valid http url");
            }
        }
        catch {
            return new ValidationResult("Enter a valid Uri");
        }

        return ValidationResult.Success;
    }
}