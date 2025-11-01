using System.ComponentModel.DataAnnotations;

namespace Tefin.ViewModels.Validations;

public class FileExistsAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null) {
            return new ValidationResult("Value cannot be null. Enter a valid file path");
        }

        try {
            if (!File.Exists(value.ToString()!)) {
                return new ValidationResult("File does not exist");
            }
        }
        catch {
            return new ValidationResult("Enter a valid file path");
        }

        return ValidationResult.Success;
    }
}