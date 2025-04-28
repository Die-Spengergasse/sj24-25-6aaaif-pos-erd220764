using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record NewPaymentCommand(
        [Range(1, int.MaxValue, ErrorMessage = "Invalid cash desk number")]
        int CashDeskNumber,
        string PaymentType,
        [Range(1, int.MaxValue, ErrorMessage = "Invalid employee registration number")]
        int EmployeeRegistrationNumber);
}
