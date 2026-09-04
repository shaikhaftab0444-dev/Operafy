using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ERP_System.Models;

namespace ERP_System.Data
{
    public class PurchaseStoreData
    {
        public List<RequisitionItem> Requisitions { get; set; } = new();
        public List<PurchaseOrderItem> PurchaseOrders { get; set; } = new();
        public List<VendorScorecardItem> Vendors { get; set; } = new();
        public List<GRNItem> GoodsReceipts { get; set; } = new();
    }

    public static class PurchaseDataStore
    {
        private static readonly object _lock = new object();
        private static PurchaseStoreData? _data;
        private static readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "purchase_datastore.json");

        public static PurchaseStoreData Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_data == null)
                    {
                        LoadData();
                    }
                    return _data!;
                }
            }
        }

        private static void LoadData()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    _data = JsonSerializer.Deserialize<PurchaseStoreData>(json);
                    if (_data != null && _data.Requisitions != null && _data.Requisitions.Any())
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading purchase store: {ex.Message}");
            }

            // Populate defaults
            _data = CreateDefaultData();
            SaveDataInternal();
        }

        public static void Save()
        {
            lock (_lock)
            {
                SaveDataInternal();
            }
        }

        private static void SaveDataInternal()
        {
            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_data, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving purchase store: {ex.Message}");
            }
        }

        public static List<RequisitionItem> GetRequisitions()
        {
            lock (_lock)
            {
                return Instance.Requisitions.OrderBy(r => r.Id).ToList();
            }
        }

        public static RequisitionItem? ApproveRequisition(int id, string approverName = "Chief Procurement Officer")
        {
            lock (_lock)
            {
                var item = Instance.Requisitions.FirstOrDefault(r => r.Id == id);
                if (item != null)
                {
                    item.Status = "Approved";
                    item.ApprovedOn = DateTime.Now.ToString("dd MMM yyyy");
                    item.ApprovedBy = approverName;

                    // Automatically generate an associated Purchase Order in Orders registry
                    var poNumber = "PO-2026-0" + new Random().Next(900, 999);
                    var po = new PurchaseOrderItem
                    {
                        PONumber = poNumber,
                        VendorName = GetDefaultVendorForDepartment(item.Department),
                        ItemsCount = 1,
                        TotalAmount = item.EstimatedCost,
                        OrderDate = DateTime.Now.ToString("dd MMM yyyy"),
                        Status = "Confirmed",
                        PaymentTerms = "Net 30"
                    };
                    Instance.PurchaseOrders.Insert(0, po);

                    SaveDataInternal();
                }
                return item;
            }
        }

        public static RequisitionItem? RejectRequisition(int id, string approverName = "Chief Procurement Officer")
        {
            lock (_lock)
            {
                var item = Instance.Requisitions.FirstOrDefault(r => r.Id == id);
                if (item != null)
                {
                    item.Status = "Rejected";
                    item.ApprovedOn = DateTime.Now.ToString("dd MMM yyyy");
                    item.ApprovedBy = approverName;
                    SaveDataInternal();
                }
                return item;
            }
        }

        public static RequisitionItem AddRequisition(RequisitionItem item)
        {
            lock (_lock)
            {
                int maxId = Instance.Requisitions.Any() ? Instance.Requisitions.Max(r => r.Id) : 500;
                item.Id = maxId + 1;
                item.RequestedOn = DateTime.Now.ToString("dd MMM yyyy");
                item.Status = "Pending";
                Instance.Requisitions.Add(item);
                SaveDataInternal();
                return item;
            }
        }

        public static List<PurchaseOrderItem> GetOrders()
        {
            lock (_lock)
            {
                return Instance.PurchaseOrders.ToList();
            }
        }

        public static PurchaseOrderItem AddOrder(CreatePurchaseOrderInput input)
        {
            lock (_lock)
            {
                var poNumber = "PO-2026-" + new Random().Next(1000, 9999);
                var po = new PurchaseOrderItem
                {
                    PONumber = poNumber,
                    VendorName = !string.IsNullOrWhiteSpace(input.VendorName) ? input.VendorName : "TechInfra Solutions Ltd",
                    ItemsCount = 1,
                    TotalAmount = !string.IsNullOrWhiteSpace(input.EstimatedCost) ? input.EstimatedCost : "₹ 1,50,000.00",
                    OrderDate = DateTime.Now.ToString("dd MMM yyyy"),
                    Status = "Confirmed",
                    PaymentTerms = !string.IsNullOrWhiteSpace(input.PaymentTerms) ? input.PaymentTerms : "Net 30"
                };
                Instance.PurchaseOrders.Insert(0, po);
                SaveDataInternal();
                return po;
            }
        }

        public static List<VendorScorecardItem> GetVendors()
        {
            lock (_lock)
            {
                return Instance.Vendors.ToList();
            }
        }

        public static VendorScorecardItem AddVendor(VendorScorecardItem item)
        {
            lock (_lock)
            {
                Instance.Vendors.Insert(0, item);
                SaveDataInternal();
                return item;
            }
        }

        public static List<GRNItem> GetReceipts()
        {
            lock (_lock)
            {
                return Instance.GoodsReceipts.ToList();
            }
        }

        public static GRNItem AddReceipt(GRNItem item)
        {
            lock (_lock)
            {
                Instance.GoodsReceipts.Insert(0, item);
                SaveDataInternal();
                return item;
            }
        }

        public static void ResetToDefaults()
        {
            lock (_lock)
            {
                _data = CreateDefaultData();
                SaveDataInternal();
            }
        }

        private static string GetDefaultVendorForDepartment(string dept)
        {
            if (string.IsNullOrWhiteSpace(dept)) return "TechInfra Solutions Ltd";
            if (dept.Contains("Tech", StringComparison.OrdinalIgnoreCase) || dept.Contains("IT", StringComparison.OrdinalIgnoreCase))
                return "TechInfra Solutions Ltd";
            if (dept.Contains("Operations", StringComparison.OrdinalIgnoreCase))
                return "Apex Industrial Supplies";
            if (dept.Contains("Logistics", StringComparison.OrdinalIgnoreCase) || dept.Contains("Fleet", StringComparison.OrdinalIgnoreCase))
                return "Delta Logistics & Freight";
            if (dept.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                return "National Paper Mills";
            if (dept.Contains("Quality", StringComparison.OrdinalIgnoreCase))
                return "SteelCraft Heavy Metals";
            return "TechInfra Solutions Ltd";
        }

        private static PurchaseStoreData CreateDefaultData()
        {
            return new PurchaseStoreData
            {
                Requisitions = new List<RequisitionItem>
                {
                    new RequisitionItem { Id = 501, Department = "Information Technology", RequestedBy = "Aftab Shaik", ItemSummary = "5x Dell UltraSharp Monitors", EstimatedCost = "₹ 1,45,000.00", Urgency = "High", RequestedOn = "01 Sep 2026", Status = "Pending" },
                    new RequisitionItem { Id = 502, Department = "Operations", RequestedBy = "Sneha Patil", ItemSummary = "Warehouse Packaging Cartons (10,000 units)", EstimatedCost = "₹ 85,000.00", Urgency = "Urgent", RequestedOn = "02 Sep 2026", Status = "Pending" },
                    new RequisitionItem { Id = 503, Department = "Administration", RequestedBy = "Sameer Verma", ItemSummary = "Quarterly Stationary & Printing Supplies", EstimatedCost = "₹ 32,500.00", Urgency = "Medium", RequestedOn = "03 Sep 2026", Status = "Pending" },
                    new RequisitionItem { Id = 504, Department = "Logistics & Fleet", RequestedBy = "Farhan Akhtar", ItemSummary = "Fleet Telematics GPS Sensors (8 Units)", EstimatedCost = "₹ 64,000.00", Urgency = "Medium", RequestedOn = "02 Sep 2026", Status = "Pending" },
                    new RequisitionItem { Id = 505, Department = "Human Resources", RequestedBy = "Pooja Hegde", ItemSummary = "Ergonomic Mesh Chairs for Executive Floor (15x)", EstimatedCost = "₹ 1,12,000.00", Urgency = "High", RequestedOn = "03 Sep 2026", Status = "Pending" },
                    new RequisitionItem { Id = 506, Department = "Quality Assurance", RequestedBy = "Rohan Das", ItemSummary = "Digital Vernier Calipers & Micrometer Set", EstimatedCost = "₹ 48,000.00", Urgency = "Urgent", RequestedOn = "04 Sep 2026", Status = "Pending" }
                },
                PurchaseOrders = new List<PurchaseOrderItem>
                {
                    new PurchaseOrderItem { PONumber = "PO-2026-0891", VendorName = "TechInfra Solutions Ltd", ItemsCount = 12, TotalAmount = "₹ 4,20,000.00", OrderDate = "29 Aug 2026", Status = "In Transit", PaymentTerms = "Net 30" },
                    new PurchaseOrderItem { PONumber = "PO-2026-0892", VendorName = "Apex Industrial Supplies", ItemsCount = 4, TotalAmount = "₹ 1,80,000.00", OrderDate = "30 Aug 2026", Status = "Confirmed", PaymentTerms = "Advance 50%" },
                    new PurchaseOrderItem { PONumber = "PO-2026-0893", VendorName = "National Paper Mills", ItemsCount = 50, TotalAmount = "₹ 65,000.00", OrderDate = "01 Sep 2026", Status = "Delivered", PaymentTerms = "Immediate" },
                    new PurchaseOrderItem { PONumber = "PO-2026-0894", VendorName = "SteelCraft Heavy Metals", ItemsCount = 18, TotalAmount = "₹ 7,50,000.00", OrderDate = "02 Sep 2026", Status = "In Transit", PaymentTerms = "Net 45" },
                    new PurchaseOrderItem { PONumber = "PO-2026-0895", VendorName = "Global Cloud Corp", ItemsCount = 1, TotalAmount = "₹ 3,60,000.00", OrderDate = "03 Sep 2026", Status = "Confirmed", PaymentTerms = "Advance 100%" },
                    new PurchaseOrderItem { PONumber = "PO-2026-0896", VendorName = "Delta Logistics & Freight", ItemsCount = 8, TotalAmount = "₹ 92,000.00", OrderDate = "04 Sep 2026", Status = "Delivered", PaymentTerms = "Net 15" }
                },
                Vendors = new List<VendorScorecardItem>
                {
                    new VendorScorecardItem { Name = "TechInfra Solutions Ltd", Rating = "4.8 / 5.0", Category = "IT & Hardware", OnTimeDeliveryRate = "98%", QualityScore = "99%", SpendYTD = "₹ 24,50,000.00", Status = "Preferred Partner" },
                    new VendorScorecardItem { Name = "Apex Industrial Supplies", Rating = "4.5 / 5.0", Category = "Operations & Tools", OnTimeDeliveryRate = "94%", QualityScore = "96%", SpendYTD = "₹ 16,80,000.00", Status = "Approved Tier 1" },
                    new VendorScorecardItem { Name = "National Paper Mills", Rating = "4.2 / 5.0", Category = "Admin & Supplies", OnTimeDeliveryRate = "91%", QualityScore = "93%", SpendYTD = "₹ 5,40,000.00", Status = "Approved Tier 2" },
                    new VendorScorecardItem { Name = "SteelCraft Heavy Metals", Rating = "4.6 / 5.0", Category = "Raw Materials", OnTimeDeliveryRate = "96%", QualityScore = "97%", SpendYTD = "₹ 32,10,000.00", Status = "Preferred Partner" },
                    new VendorScorecardItem { Name = "Delta Logistics & Freight", Rating = "3.9 / 5.0", Category = "Supply Chain", OnTimeDeliveryRate = "86%", QualityScore = "89%", SpendYTD = "₹ 8,90,000.00", Status = "Under Review" },
                    new VendorScorecardItem { Name = "Global Cloud Corp", Rating = "4.9 / 5.0", Category = "Software & Cloud", OnTimeDeliveryRate = "100%", QualityScore = "100%", SpendYTD = "₹ 12,00,000.00", Status = "Strategic Vendor" }
                },
                GoodsReceipts = new List<GRNItem>
                {
                    new GRNItem { GRNNumber = "GRN-2026-0411", Status = "Inspected & Accepted", PONumber = "PO-2026-0893", VendorName = "National Paper Mills", ReceivedDate = "02 Sep 2026", Warehouse = "WH-Main Bay A", ItemsReceived = 50, InspectedBy = "Rajesh K." },
                    new GRNItem { GRNNumber = "GRN-2026-0410", Status = "3-Way Matched", PONumber = "PO-2026-0890", VendorName = "SteelCraft Heavy Metals", ReceivedDate = "01 Sep 2026", Warehouse = "WH-Raw Storage B3", ItemsReceived = 120, InspectedBy = "Sneha P." },
                    new GRNItem { GRNNumber = "GRN-2026-0409", Status = "Quality Quarantine", PONumber = "PO-2026-0888", VendorName = "Apex Industrial Supplies", ReceivedDate = "30 Aug 2026", Warehouse = "WH-QC Holding Cell", ItemsReceived = 4, InspectedBy = "Vikram M." },
                    new GRNItem { GRNNumber = "GRN-2026-0408", Status = "Completed & Stocked", PONumber = "PO-2026-0885", VendorName = "TechInfra Solutions Ltd", ReceivedDate = "28 Aug 2026", Warehouse = "WH-IT Staging Rm", ItemsReceived = 25, InspectedBy = "Aftab S." }
                }
            };
        }
    }
}
