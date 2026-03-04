using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    /// <summary>
    /// Mock payment service that simulates payment processing.
    /// Logs payment attempts and results to Application Insights.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly TelemetryClient _telemetry;
        private readonly IBillService _billService;

        public PaymentService(TelemetryClient telemetryClient, IBillService billService)
        {
            _telemetry = telemetryClient;
            _billService = billService;
        }

        public PaymentResult ProcessPayment(PaymentRequest request, string correlationId)
        {
            // Track payment attempt as a custom event
            _telemetry.TrackEvent("PaymentAttempt", new System.Collections.Generic.Dictionary<string, string>
            {
                ["BillId"] = request.BillId.ToString(),
                ["Amount"] = request.Amount.ToString("F2"),
                ["CorrelationId"] = correlationId
            });

            var bill = _billService.GetBillById(request.BillId);
            if (bill == null)
            {
                var failResult = PaymentResult.Failure("Bill not found.", correlationId);
                TrackPaymentResult(failResult, request);
                return failResult;
            }

            if (bill.IsPaid)
            {
                var failResult = PaymentResult.Failure("Bill has already been paid.", correlationId);
                TrackPaymentResult(failResult, request);
                return failResult;
            }

            // Simulate payment processing (always succeeds in this demo)
            // In production: call payment gateway API and handle responses
            var result = PaymentResult.Success(request.Amount, correlationId);

            // Mark bill as paid in mock store
            _billService.MarkAsPaid(request.BillId);

            // Track successful payment
            _telemetry.TrackEvent("PaymentSuccess", new System.Collections.Generic.Dictionary<string, string>
            {
                ["BillId"] = request.BillId.ToString(),
                ["TransactionId"] = result.TransactionId,
                ["Amount"] = result.AmountPaid.ToString("F2"),
                ["CorrelationId"] = correlationId
            });

            // Track metric for monitoring dashboards
            _telemetry.TrackMetric("PaymentAmount", (double)result.AmountPaid);

            return result;
        }

        private void TrackPaymentResult(PaymentResult result, PaymentRequest request)
        {
            if (!result.IsSuccess)
            {
                _telemetry.TrackEvent("PaymentFailure", new System.Collections.Generic.Dictionary<string, string>
                {
                    ["BillId"] = request.BillId.ToString(),
                    ["Reason"] = result.Message,
                    ["CorrelationId"] = result.CorrelationId
                });
            }
        }
    }
}
