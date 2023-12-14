#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace Tefin.ViewModels.Validations;

public class IsHttpAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null)
            return new ValidationResult("Value cannot be null. Enter a valid http address");

        try {
            var uri = new Uri(value.ToString()!);
            if (!uri.Scheme.StartsWith("http")) {
                return new ValidationResult(errorMessage: "Enter a valid http url");
            }
        }
        catch {
            return new ValidationResult(errorMessage: "Enter a valid Uri");
        }

        return ValidationResult.Success;
    }
}

public class IsProtoFileAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null)
            return new ValidationResult("Value cannot be null. Please select a proto file");

        if (!File.Exists(value!.ToString())) {
            return new ValidationResult(errorMessage: "Proto file does not exist");
        }

        return ValidationResult.Success;
    }
}

public class IsValidFolderNameAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null)
            return new ValidationResult("Value cannot be null. Enter a valid folder name");

        var enteredName = value.ToString();
        var cleanedUp = Core.Utils.makeValidFileName(value.ToString());
        if (enteredName != cleanedUp) {
            return new ValidationResult(errorMessage: "Name contains invalid characters");
        }

        return ValidationResult.Success;
    }
}