#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace Tefin.ViewModels.Validations;

public class IsProtoFileAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null) {
            return new ValidationResult("Value cannot be null. Please select a proto file");
        }

        if (!File.Exists(value!.ToString())) {
            return new ValidationResult("Proto file does not exist");
        }

        return ValidationResult.Success;
    }
}