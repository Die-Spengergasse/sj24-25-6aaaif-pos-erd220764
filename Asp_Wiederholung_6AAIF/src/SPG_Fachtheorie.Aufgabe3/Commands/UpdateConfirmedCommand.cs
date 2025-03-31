using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    public class UpdateConfirmedCommand : IValidatableObject
    {
        public DateTime Confirmed { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Confirmed > DateTime.UtcNow.AddMinutes(1))
            {
                yield return new ValidationResult(
                    "Confirmed date cannot be more than 1 minute in the future",
                    new[] { nameof(Confirmed) });
            }
        }
    }
} 