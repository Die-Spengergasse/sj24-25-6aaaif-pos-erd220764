using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record UpdateConfirmedCommand(
        DateTime Confirmed) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Confirmed > DateTime.Now.AddMinutes(1))
                yield return new ValidationResult("Invalid confirmed date",
                    new string[] { nameof(Confirmed) });
        }
    }
}
