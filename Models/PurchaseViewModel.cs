using System.Collections.Generic;

namespace ERP_System.Models
{
    public class PurchaseViewModel
    {
        public List<Transaction> PurchaseOrders { get; set; } = new List<Transaction>();
        public List<Supplier> SuppliersList { get; set; } = new List<Supplier>();
        public List<Product> ProductsList { get; set; } = new List<Product>();

        // KPI metrics
        public decimal TotalPurchaseSpend { get; set; }
        public int TotalOrdersCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int ReceivedOrdersCount { get; set; }
        public int TotalSuppliersCount { get; set; }

        // Filter states
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedStatus { get; set; } = string.Empty;
        public string SelectedSupplier { get; set; } = string.Empty;
    }
}
