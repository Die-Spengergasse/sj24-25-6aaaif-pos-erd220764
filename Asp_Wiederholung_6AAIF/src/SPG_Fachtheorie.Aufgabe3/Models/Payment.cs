using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Models
{
    public class Payment
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public ICollection<PaymentItem> PaymentItems { get; set; }
        
        public DateTime? Confirmed { get; set; }
    }
} 