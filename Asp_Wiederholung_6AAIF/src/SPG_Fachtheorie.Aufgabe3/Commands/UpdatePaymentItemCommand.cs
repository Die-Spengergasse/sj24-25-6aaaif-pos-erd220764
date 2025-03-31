using System;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    public class UpdatePaymentItemCommand
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Article name is required")]
        public string ArticleName { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public int Amount { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Payment ID must be greater than 0")]
        public int PaymentId { get; set; }
        
        public DateTime? LastUpdated { get; set; }
    }
} 