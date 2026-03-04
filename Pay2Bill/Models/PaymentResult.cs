using System;

namespace Pay2Bill.Models
{
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string CorrelationId { get; set; }

        public static PaymentResult Success(decimal amount, string correlationId)
        {
            return new PaymentResult
            {
                IsSuccess = true,
                TransactionId = "TXN-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                Message = "Payment processed successfully.",
                AmountPaid = amount,
                ProcessedAt = DateTime.UtcNow,
                CorrelationId = correlationId
            };
        }

        public static PaymentResult Failure(string reason, string correlationId)
        {
            return new PaymentResult
            {
                IsSuccess = false,
                TransactionId = null,
                Message = reason,
                AmountPaid = 0,
                ProcessedAt = DateTime.UtcNow,
                CorrelationId = correlationId
            };
        }
    }
}
