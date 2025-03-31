using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    /*
    {
      "registrationNumber": 1003,
      "firstName": "FN1",
      "lastName": "LN1",
      "address": {
        "street": "Spengergasse 20",
        "zip": "1050",
        "city": "Wien"
      },
      "carType": "SUV"
    }
    */
    public record NewManagerCmd(
        [Range(1, 999999, ErrorMessage = "Invalid registration numbner")]
        int RegistrationNumber,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid firstname")]
        string FirstName,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid firstname")]
        string LastName,
        AddressCmd? Address,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid car type")]
        string CarType
        ) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FirstName.Length + LastName.Length < 3)
                yield return new ValidationResult(
                    "Invalid name",
                    new string[] {nameof(FirstName), nameof(LastName)});
        }
    }
}
