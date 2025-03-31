using System;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Models
{
    public class PaymentItem
    {
        public int Id { get; set; }
        
        [Required]
        public string ArticleName { get; set; }
        
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }
        
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        
        public int PaymentId { get; set; }
        public Payment Payment { get; set; }
        
        public DateTime? LastUpdated { get; set; }
    }
} 