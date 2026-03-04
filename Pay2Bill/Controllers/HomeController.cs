using System.Web.Mvc;
using Pay2Bill.Services;

namespace Pay2Bill.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBillService _billService;

        public HomeController()
        {
            _billService = new BillService();
        }

        // GET: /
        public ActionResult Index()
        {
            var bills = _billService.GetAllBills();
            return View(bills);
        }
    }
}
