namespace SPG_Fachtheorie.Aufgabe3.Dtos
{
    public record PaymentDto(
        int Id, string EmployeeFirstName, string EmployeeLastName,
        DateTime PaymentDateTime,
        int CashDeskNumber, string PaymentType, decimal TotalAmount);
}
