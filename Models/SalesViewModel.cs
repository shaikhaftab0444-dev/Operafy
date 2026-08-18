using System.Collections.Generic;

namespace ERP_System.Models
{
    public class SalesViewModel
    {
        public List<Transaction> SalesInvoices { get; set; } = new List<Transaction>();
        public List<Customer> CustomersList { get; set; } = new List<Customer>();
        public List<Product> ProductsList { get; set; } = new List<Product>();

        // KPI metrics
        public decimal TotalSalesRevenue { get; set; }
        public int TotalInvoicesCount { get; set; }
        public int PaidInvoicesCount { get; set; }
        public int PendingReceivablesCount { get; set; }
        public decimal TotalPendingAmount { get; set; }

        // Filter states
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedStatus { get; set; } = string.Empty;
        public string SelectedCustomer { get; set; } = string.Empty;
    }
}
