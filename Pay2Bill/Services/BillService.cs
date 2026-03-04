using System;
using System.Collections.Generic;
using Pay2Bill.Models;

namespace Pay2Bill.Services
{
    /// <summary>
    /// Mock in-memory bill service.
    /// In production this would query an Azure SQL Database or Cosmos DB.
    /// </summary>
    public class BillService : IBillService
    {
        // Simulated in-memory data store
        private static readonly List<Bill> _bills = new List<Bill>
        {
            new Bill
            {
                Id = 1,
                Type = BillType.Internet,
                AccountNumber = "INET-00123",
                CustomerName = "John Doe",
                AmountDue = 79.99m,
                DueDate = DateTime.Today.AddDays(7),
                IsPaid = false,
                Description = "Fiber 500 Mbps - Monthly Service"
            },
            new Bill
            {
                Id = 2,
                Type = BillType.Phone,
                AccountNumber = "CELL-00456",
                CustomerName = "John Doe",
                AmountDue = 45.00m,
                DueDate = DateTime.Today.AddDays(3),
                IsPaid = false,
                Description = "Unlimited Talk & Text Plan"
            },
            new Bill
            {
                Id = 3,
                Type = BillType.Internet,
                AccountNumber = "INET-00789",
                CustomerName = "Jane Smith",
                AmountDue = 49.99m,
                DueDate = DateTime.Today.AddDays(14),
                IsPaid = false,
                Description = "DSL 100 Mbps - Monthly Service"
            },
            new Bill
            {
                Id = 4,
                Type = BillType.Phone,
                AccountNumber = "CELL-01012",
                CustomerName = "Jane Smith",
                AmountDue = 30.00m,
                DueDate = DateTime.Today.AddDays(-2),
                IsPaid = true,
                Description = "Basic Talk & Text Plan"
            }
        };

        public IEnumerable<Bill> GetAllBills()
        {
            return _bills;
        }

        public Bill GetBillById(int id)
        {
            return _bills.Find(b => b.Id == id);
        }

        public void MarkAsPaid(int id)
        {
            var bill = _bills.Find(b => b.Id == id);
            if (bill != null)
            {
                bill.IsPaid = true;
            }
        }
    }
}
