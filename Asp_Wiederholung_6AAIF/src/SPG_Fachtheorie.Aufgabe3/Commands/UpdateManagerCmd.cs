using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    public record UpdateManagerCmd(
        [Range(1, 999999, ErrorMessage = "Invalid registration numbner")]
        int RegistrationNumber,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid firstname")]
        string FirstName,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid firstname")]
        string LastName,
        AddressCmd? Address,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid car type")]
        string CarType,
        DateTime? LastUpdate
        ) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FirstName.Length + LastName.Length < 3)
                yield return new ValidationResult(
                    "Invalid name",
                    new string[] { nameof(FirstName), nameof(LastName) });
        }
    }
}
