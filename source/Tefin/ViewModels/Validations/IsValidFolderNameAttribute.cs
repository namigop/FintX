using System.ComponentModel.DataAnnotations;

namespace Tefin.ViewModels.Validations;

public class IsValidFolderNameAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null) {
            return new ValidationResult("Value cannot be null. Enter a valid folder name");
        }

        var enteredName = value.ToString();
        var cleanedUp = Core.Utils.makeValidFileName(value.ToString());
        if (enteredName != cleanedUp) {
            return new ValidationResult("Name contains invalid characters");
        }

        return ValidationResult.Success;
    }
}