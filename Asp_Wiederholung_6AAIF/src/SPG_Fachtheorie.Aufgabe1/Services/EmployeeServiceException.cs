using System;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    [Serializable]
    public class EmployeeServiceException : Exception
    {
        public bool NotFoundException { get; set; }
        public EmployeeServiceException()
        {
        }

        public EmployeeServiceException(string? message) : base(message)
        {
        }

        public EmployeeServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}