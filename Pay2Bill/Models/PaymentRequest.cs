using System.ComponentModel.DataAnnotations;

namespace Pay2Bill.Models
{
    public class PaymentRequest
    {
        [Required]
        public int BillId { get; set; }

        [Required]
        [StringLength(19, MinimumLength = 13)]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}/\d{2}$", ErrorMessage = "Use format MM/YY")]
        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; }

        [Required]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
        [Display(Name = "CVV")]
        public string Cvv { get; set; }

        [Required]
        [Display(Name = "Cardholder Name")]
        public string CardholderName { get; set; }

        public decimal Amount { get; set; }
    }
}
