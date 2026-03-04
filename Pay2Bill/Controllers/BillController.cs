using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.ApplicationInsights;
using Pay2Bill.Models;
using Pay2Bill.Services;

namespace Pay2Bill.Controllers
{
    public class BillController : Controller
    {
        private readonly IBillService _billService;
        private readonly IPaymentService _paymentService;
        private readonly TelemetryClient _telemetry;

        // Constructor uses poor-man's DI (manual wiring) for .NET Framework 4.8 compatibility
        // without adding a DI container. For production, consider Unity, Autofac, or Ninject.
        public BillController()
        {
            _telemetry = new TelemetryClient();
            _billService = new BillService();
            _paymentService = new PaymentService(_telemetry, _billService);
        }

        // GET: /Bill
        public ActionResult Index()
        {
            var bills = _billService.GetAllBills();
            return View(bills);
        }

        // GET: /Bill/Pay/{id}
        public ActionResult Pay(int id)
        {
            var bill = _billService.GetBillById(id);
            if (bill == null)
                return HttpNotFound();

            if (bill.IsPaid)
            {
                TempData["Info"] = "This bill has already been paid.";
                return RedirectToAction("Index");
            }

            var request = new PaymentRequest
            {
                BillId = bill.Id,
                Amount = bill.AmountDue
            };

            ViewBag.Bill = bill;
            return View(request);
        }

        // POST: /Bill/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(PaymentRequest request)
        {
            var bill = _billService.GetBillById(request.BillId);
            if (bill == null)
                return HttpNotFound();

            ViewBag.Bill = bill;

            if (!ModelState.IsValid)
                return View(request);

            // Set payment amount from the actual bill
            request.Amount = bill.AmountDue;

            // Retrieve or create a correlation ID for tracing this request
            var correlationId = GetOrCreateCorrelationId();

            // Set the correlation ID on the telemetry context for all events in this request
            _telemetry.Context.Operation.Id = correlationId;
            _telemetry.Context.Operation.Name = "PayBill";

            var result = _paymentService.ProcessPayment(request, correlationId);

            if (result.IsSuccess)
            {
                return RedirectToAction("Confirmation", new { transactionId = result.TransactionId, billId = bill.Id });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(request);
        }

        // GET: /Bill/Confirmation
        public ActionResult Confirmation(string transactionId, int billId)
        {
            var bill = _billService.GetBillById(billId);
            ViewBag.TransactionId = transactionId;
            ViewBag.Bill = bill;
            return View();
        }

        private string GetOrCreateCorrelationId()
        {
            // Respect incoming correlation header (for distributed tracing)
            var headerName = System.Configuration.ConfigurationManager.AppSettings["Correlation:HeaderName"]
                             ?? "X-Correlation-Id";
            var correlationId = Request.Headers[headerName];
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            Response.Headers[headerName] = correlationId;
            return correlationId;
        }
    }
}
