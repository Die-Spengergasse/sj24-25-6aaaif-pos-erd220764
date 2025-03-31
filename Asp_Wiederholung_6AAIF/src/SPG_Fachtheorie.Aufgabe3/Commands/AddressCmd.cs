using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    public record AddressCmd(
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid firstname")]
        string Street,
        [RegularExpression(@"^[0-9]{4,5}$", ErrorMessage = "Invalid zip")]
        string Zip,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid firstname")]
        string City);
}
