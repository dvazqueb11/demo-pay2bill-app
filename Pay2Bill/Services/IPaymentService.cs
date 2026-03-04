using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    public interface IPaymentService
    {
        PaymentResult ProcessPayment(PaymentRequest request, string correlationId);
    }
}
