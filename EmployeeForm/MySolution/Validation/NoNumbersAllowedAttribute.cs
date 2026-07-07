using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EmployeeForm.Validation;

public class NoNumbersAllowedAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && str.Any(char.IsDigit))
        {
            return new ValidationResult("Numbers are not allowed in this field.");
        }
        return ValidationResult.Success;
    }
}
