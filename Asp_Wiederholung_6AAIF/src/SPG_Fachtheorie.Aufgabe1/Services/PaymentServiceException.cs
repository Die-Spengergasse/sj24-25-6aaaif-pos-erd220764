using System;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    [Serializable]
    public class PaymentServiceException : Exception
    {
        public bool NotFoundException { get; set; }
        public PaymentServiceException()
        {
        }

        public PaymentServiceException(string? message) : base(message)
        {
        }

        public PaymentServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}