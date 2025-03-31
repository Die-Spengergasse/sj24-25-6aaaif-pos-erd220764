using SPG_Fachtheorie.Aufgabe1.Model;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    public record NewPaymentCommand(
        [Range(1, int.MaxValue, ErrorMessage = "Invalid cash desk number")]
        int CashDeskNumber,
        DateTime PaymentDateTime,
        string PaymentType,
        [Range(1, int.MaxValue, ErrorMessage = "Invalid employee registration number")]
        int EmployeeRegistrationNumber) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentDateTime > DateTime.Now.AddMinutes(1))
                yield return new ValidationResult("Invalid payment date", new string[] {nameof(PaymentDateTime)});
        }
    }
}
