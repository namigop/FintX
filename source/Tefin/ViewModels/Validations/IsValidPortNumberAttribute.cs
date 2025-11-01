using System.ComponentModel.DataAnnotations;

namespace Tefin.ViewModels.Validations;

public class IsValidPortNumberAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null) {
            return new ValidationResult("Value cannot be null. Enter a valid port number");
        }

        var enteredPort = value.ToString()?.Trim();
        if (uint.TryParse(enteredPort, out var port)) {
            return ValidationResult.Success;
        }

        return new ValidationResult($"Value {enteredPort} is not a valid port number");
    }
}