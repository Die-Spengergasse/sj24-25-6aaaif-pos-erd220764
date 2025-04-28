using System;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record NewPaymentItemCommand(
        [StringLength(255, MinimumLength = 1)]
        string ArticleName,
        [Range(1, int.MaxValue)]
        int Amount,
        [Range(0, 1_000_000)]
        decimal Price,
        [Range(1, int.MaxValue)]
        int PaymentId);
}
