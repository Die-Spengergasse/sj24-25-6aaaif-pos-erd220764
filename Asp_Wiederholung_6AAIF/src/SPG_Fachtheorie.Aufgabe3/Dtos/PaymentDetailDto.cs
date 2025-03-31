namespace SPG_Fachtheorie.Aufgabe3.Dtos
{
    public record PaymentDetailDto(
        int Id, string EmployeeFirstName, string EmployeeLastName,
        int CashDeskNumber, string PaymentType, 
        List<PaymentItemDto> PaymentItems);
}
