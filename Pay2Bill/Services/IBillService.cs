using System.Collections.Generic;
using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    public interface IBillService
    {
        IEnumerable<Bill> GetAllBills();
        Bill GetBillById(int id);
        void MarkAsPaid(int id);
    }
}
