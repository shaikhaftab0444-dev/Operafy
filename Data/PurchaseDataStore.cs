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
                        PaymentTerms = "Net 30",
                        Department = item.Department,
                        RequestedBy = item.RequestedBy,
                        ItemSummary = item.ItemSummary,
                        DeliveryDate = DateTime.Now.AddDays(7).ToString("dd MMM yyyy")
                    };
                    EnsurePOEnriched(po);
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
                bool modified = false;
                foreach (var po in Instance.PurchaseOrders)
                {
                    if (EnsurePOEnriched(po)) modified = true;
                }
                if (modified) SaveDataInternal();
                return Instance.PurchaseOrders.ToList();
            }
        }

        public static PurchaseOrderItem? GetOrderByNumber(string poNumber)
        {
            lock (_lock)
            {
                var po = Instance.PurchaseOrders.FirstOrDefault(p => p.PONumber.Equals(poNumber, StringComparison.OrdinalIgnoreCase));
                if (po != null)
                {
                    if (EnsurePOEnriched(po)) SaveDataInternal();
                }
                return po;
            }
        }

        public static PurchaseOrderItem? UpdateOrderStatus(string poNumber, string status)
        {
            lock (_lock)
            {
                var po = Instance.PurchaseOrders.FirstOrDefault(p => p.PONumber.Equals(poNumber, StringComparison.OrdinalIgnoreCase));
                if (po != null)
                {
                    po.Status = status;
                    EnsurePOEnriched(po);
                    SaveDataInternal();
                }
                return po;
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
                    PaymentTerms = !string.IsNullOrWhiteSpace(input.PaymentTerms) ? input.PaymentTerms : "Net 30",
                    Department = input.Department ?? "Corporate Procurement",
                    ItemSummary = input.ItemSummary ?? "Procured Goods & Materials",
                    DeliveryDate = !string.IsNullOrWhiteSpace(input.DeliveryDate) ? input.DeliveryDate : DateTime.Now.AddDays(7).ToString("dd MMM yyyy")
                };
                EnsurePOEnriched(po);
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

        public static bool UpdateVendorStatus(string vendorName, string newStatus)
        {
            lock (_lock)
            {
                var v = Instance.Vendors.FirstOrDefault(x => x.Name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
                if (v != null)
                {
                    v.Status = newStatus;
                    SaveDataInternal();
                    return true;
                }
                return false;
            }
        }

        public static bool SaveVendorAuditFinding(string vendorName, string remarks, string nextAuditDate, string riskGrade)
        {
            lock (_lock)
            {
                var v = Instance.Vendors.FirstOrDefault(x => x.Name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
                if (v != null)
                {
                    if (!string.IsNullOrWhiteSpace(remarks)) v.AuditorRemarks = remarks;
                    if (!string.IsNullOrWhiteSpace(nextAuditDate)) v.NextAuditDate = nextAuditDate;
                    if (!string.IsNullOrWhiteSpace(riskGrade)) v.RiskGrade = riskGrade;
                    SaveDataInternal();
                    return true;
                }
                return false;
            }
        }

        public static VendorAuditDossier GetVendorAuditDossier(string vendorName)
        {
            lock (_lock)
            {
                var v = Instance.Vendors.FirstOrDefault(x => x.Name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
                if (v == null)
                {
                    v = new VendorScorecardItem
                    {
                        Name = vendorName,
                        Category = "Procurement Partner",
                        Rating = "4.8 / 5.0",
                        Status = "Preferred Partner",
                        SpendYTD = "₹ 24,50,000.00",
                        OnTimeDeliveryRate = "98%",
                        QualityScore = "99%"
                    };
                }

                var dossier = new VendorAuditDossier
                {
                    VendorName = v.Name,
                    Category = v.Category,
                    Rating = !string.IsNullOrWhiteSpace(v.Rating) ? v.Rating : "4.8 / 5.0",
                    Status = !string.IsNullOrWhiteSpace(v.Status) ? v.Status : "Preferred Partner",
                    SpendYTD = !string.IsNullOrWhiteSpace(v.SpendYTD) ? v.SpendYTD : "₹ 0.00",
                    OnTimeDeliveryRate = !string.IsNullOrWhiteSpace(v.OnTimeDeliveryRate) ? v.OnTimeDeliveryRate : "95%",
                    QualityScore = !string.IsNullOrWhiteSpace(v.QualityScore) ? v.QualityScore : "98%",
                    AuditDate = DateTime.Now.ToString("dd MMM yyyy"),
                    AuditValidUntil = DateTime.Now.AddYears(1).ToString("dd MMM yyyy"),
                    NextAuditDate = !string.IsNullOrWhiteSpace(v.NextAuditDate) ? v.NextAuditDate : DateTime.Now.AddMonths(6).ToString("dd MMM yyyy"),
                    LeadAuditor = "Rajesh Khanna, Lead SRM Quality Auditor",
                    AuditorRemarks = !string.IsNullOrWhiteSpace(v.AuditorRemarks) ? v.AuditorRemarks : "Supplier demonstrates exceptional compliance with SLA requirements, robust manufacturing quality gates, and automated GST/e-Invoicing integration."
                };

                // Populate vendor-specific profiles
                string vn = v.Name.ToLower();
                if (vn.Contains("techinfra"))
                {
                    dossier.Gstin = !string.IsNullOrWhiteSpace(v.Gstin) ? v.Gstin : "27AAACN8921K1Z2";
                    dossier.Pan = !string.IsNullOrWhiteSpace(v.Pan) ? v.Pan : "AAACN8921K";
                    dossier.UdyamRegNo = "UDYAM-MH-03-0019842";
                    dossier.CinNumber = "U72200MH2015PLC261890";
                    dossier.ContactPerson = !string.IsNullOrWhiteSpace(v.ContactPerson) ? v.ContactPerson : "Rohan Malhotra (Director - Enterprise Accounts)";
                    dossier.ContactEmail = !string.IsNullOrWhiteSpace(v.ContactEmail) ? v.ContactEmail : "procurement-support@techinfra.com";
                    dossier.ContactPhone = !string.IsNullOrWhiteSpace(v.ContactPhone) ? v.ContactPhone : "+91 98450 82910";
                    dossier.RegisteredAddress = "Unit 402-404, Tech Park Central, Andheri East, Mumbai, Maharashtra - 400069";
                    dossier.PlantLocation = "Electronic Hardware Zone, Hinjewadi Phase 2, Pune, Maharashtra - 411057";
                    dossier.BankName = "HDFC Bank Ltd (Commercial Branch, Nariman Point)";
                    dossier.BankAccountMasked = "502000492819XX";
                    dossier.IfscCode = "HDFC0000128";
                    dossier.PaymentTerms = !string.IsNullOrWhiteSpace(v.PaymentTerms) ? v.PaymentTerms : "Net 30 Days";
                    dossier.LeadTimeDays = "3-5 Business Days";
                    dossier.DefectRatio = "0.12%";
                    dossier.PriceCompetitiveness = "97%";
                    dossier.RiskGrade = !string.IsNullOrWhiteSpace(v.RiskGrade) ? v.RiskGrade : "Low Risk (Grade AAA)";
                    dossier.Certifications = new List<string>
                    {
                        "ISO 9001:2015 Quality Management System",
                        "ISO/IEC 27001:2022 Information Security Management",
                        "RoHS & REACH Directives Compliant Hardware",
                        "CMMI Maturity Level 3 Validated Facility",
                        "Dun & Bradstreet Comprehensive 5A1 Rating"
                    };
                }
                else if (vn.Contains("apex"))
                {
                    dossier.Gstin = !string.IsNullOrWhiteSpace(v.Gstin) ? v.Gstin : "29AABCA4512L1Z8";
                    dossier.Pan = !string.IsNullOrWhiteSpace(v.Pan) ? v.Pan : "AABCA4512L";
                    dossier.UdyamRegNo = "UDYAM-KR-03-0045129";
                    dossier.CinNumber = "U29299KA2012PTC068112";
                    dossier.ContactPerson = !string.IsNullOrWhiteSpace(v.ContactPerson) ? v.ContactPerson : "Kavita Rao (Senior Procurement Specialist)";
                    dossier.ContactEmail = !string.IsNullOrWhiteSpace(v.ContactEmail) ? v.ContactEmail : "orders@apexindustrial.in";
                    dossier.ContactPhone = !string.IsNullOrWhiteSpace(v.ContactPhone) ? v.ContactPhone : "+91 98860 41209";
                    dossier.RegisteredAddress = "Plot 88, Peenya Industrial Area Phase 1, Bangalore, Karnataka - 560058";
                    dossier.PlantLocation = "Apex Heavy Fabrication Yard, Dobbaspet Industrial Area, Tumkur - 562111";
                    dossier.BankName = "State Bank of India (Industrial Finance Branch, Bangalore)";
                    dossier.BankAccountMasked = "31948821034XX";
                    dossier.IfscCode = "SBIN0004134";
                    dossier.PaymentTerms = !string.IsNullOrWhiteSpace(v.PaymentTerms) ? v.PaymentTerms : "Advance 50% / Net 30";
                    dossier.LeadTimeDays = "4-6 Business Days";
                    dossier.DefectRatio = "0.38%";
                    dossier.PriceCompetitiveness = "94%";
                    dossier.RiskGrade = !string.IsNullOrWhiteSpace(v.RiskGrade) ? v.RiskGrade : "Low Risk (Grade A)";
                    dossier.Certifications = new List<string>
                    {
                        "ISO 9001:2015 Quality Management System",
                        "CE Machinery Directive 2006/42/EC Certified",
                        "EN ISO 12100 Safety of Industrial Machinery",
                        "Bureau of Indian Standards (BIS) Grade-1 License",
                        "OHSAS 18001 Occupational Health & Safety"
                    };
                }
                else if (vn.Contains("national paper"))
                {
                    dossier.Gstin = !string.IsNullOrWhiteSpace(v.Gstin) ? v.Gstin : "33AABCN1234P1Z3";
                    dossier.Pan = !string.IsNullOrWhiteSpace(v.Pan) ? v.Pan : "AABCN1234P";
                    dossier.UdyamRegNo = "UDYAM-TN-02-0091243";
                    dossier.CinNumber = "L21010TN1989PLC017845";
                    dossier.ContactPerson = !string.IsNullOrWhiteSpace(v.ContactPerson) ? v.ContactPerson : "S. Swaminathan (Commercial Sales Head)";
                    dossier.ContactEmail = !string.IsNullOrWhiteSpace(v.ContactEmail) ? v.ContactEmail : "sales@nationalpapermills.com";
                    dossier.ContactPhone = !string.IsNullOrWhiteSpace(v.ContactPhone) ? v.ContactPhone : "+91 94440 23891";
                    dossier.RegisteredAddress = "Paper Mills Road, Perambur, Chennai, Tamil Nadu - 600011";
                    dossier.PlantLocation = "Pulp & Kraft Board Plant, Erode-Karur Highway, Tamil Nadu - 638002";
                    dossier.BankName = "ICICI Bank Ltd (T. Nagar Commercial Branch, Chennai)";
                    dossier.BankAccountMasked = "00180501839XX";
                    dossier.IfscCode = "ICIC0000018";
                    dossier.PaymentTerms = !string.IsNullOrWhiteSpace(v.PaymentTerms) ? v.PaymentTerms : "Immediate / 15 Days";
                    dossier.LeadTimeDays = "2-4 Business Days";
                    dossier.DefectRatio = "0.45%";
                    dossier.PriceCompetitiveness = "98%";
                    dossier.RiskGrade = !string.IsNullOrWhiteSpace(v.RiskGrade) ? v.RiskGrade : "Low Risk (Grade A)";
                    dossier.Certifications = new List<string>
                    {
                        "FSC Forest Stewardship Council Chain of Custody",
                        "ISO 14001:2015 Environmental Management System",
                        "ISO 9001:2015 Quality Management Standards",
                        "Green Pro Eco-Label Certified Sustainable Facility",
                        "Zero Liquid Discharge (ZLD) Environmental Audit Pass"
                    };
                }
                else if (vn.Contains("steelcraft"))
                {
                    dossier.Gstin = !string.IsNullOrWhiteSpace(v.Gstin) ? v.Gstin : "24AAACS7890M1Z5";
                    dossier.Pan = !string.IsNullOrWhiteSpace(v.Pan) ? v.Pan : "AAACS7890M";
                    dossier.UdyamRegNo = "UDYAM-GJ-01-0067341";
                    dossier.CinNumber = "L27100GJ2004PLC044321";
                    dossier.ContactPerson = !string.IsNullOrWhiteSpace(v.ContactPerson) ? v.ContactPerson : "Dharmesh Patel (VP - Structural Steel)";
                    dossier.ContactEmail = !string.IsNullOrWhiteSpace(v.ContactEmail) ? v.ContactEmail : "dispatch@steelcraftmetals.in";
                    dossier.ContactPhone = !string.IsNullOrWhiteSpace(v.ContactPhone) ? v.ContactPhone : "+91 98250 67182";
                    dossier.RegisteredAddress = "GIDC Industrial Estate, Odhav, Ahmedabad, Gujarat - 382415";
                    dossier.PlantLocation = "Steel Rolling Mill #3, Sanand Industrial Area, Gujarat - 382110";
                    dossier.BankName = "Axis Bank Ltd (Corporate Banking Centre, Ashram Road)";
                    dossier.BankAccountMasked = "91802004819XX";
                    dossier.IfscCode = "UTIB0000084";
                    dossier.PaymentTerms = !string.IsNullOrWhiteSpace(v.PaymentTerms) ? v.PaymentTerms : "Net 45 Days";
                    dossier.LeadTimeDays = "5-8 Business Days";
                    dossier.DefectRatio = "0.18%";
                    dossier.PriceCompetitiveness = "95%";
                    dossier.RiskGrade = !string.IsNullOrWhiteSpace(v.RiskGrade) ? v.RiskGrade : "Low Risk (Grade AAA)";
                    dossier.Certifications = new List<string>
                    {
                        "BIS IS 1786:2008 High Strength Deformed Steel Bars",
                        "ISO 9001:2015 Certified Manufacturing Facility",
                        "ISO 45001:2018 Occupational Health & Safety",
                        "Lloyd's Register Marine Grade Structural Steel Certification",
                        "ASTM A615 International Structural Conformity"
                    };
                }
                else if (vn.Contains("delta logistics"))
                {
                    dossier.Gstin = !string.IsNullOrWhiteSpace(v.Gstin) ? v.Gstin : "07AABCD5678Q1Z1";
                    dossier.Pan = !string.IsNullOrWhiteSpace(v.Pan) ? v.Pan : "AABCD5678Q";
                    dossier.UdyamRegNo = "UDYAM-DL-08-0012984";
                    dossier.CinNumber = "U63090DL2018PTC334512";
                    dossier.ContactPerson = !string.IsNullOrWhiteSpace(v.ContactPerson) ? v.ContactPerson : "Rajeev Sethi (Fleet & Logistics Operations)";
                    dossier.ContactEmail = !string.IsNullOrWhiteSpace(v.ContactEmail) ? v.ContactEmail : "operations@deltalogistics.co.in";
                    dossier.ContactPhone = !string.IsNullOrWhiteSpace(v.ContactPhone) ? v.ContactPhone : "+91 98110 56230";
                    dossier.RegisteredAddress = "Cargo Terminal Block B, IGI Airport Logistics Centre, New Delhi - 110037";
                    dossier.PlantLocation = "Central Fleet Depot & Telematics Hub, Transport Nagar, Delhi - 110042";
                    dossier.BankName = "Kotak Mahindra Bank (Connaught Place, New Delhi)";
                    dossier.BankAccountMasked = "28119043218XX";
                    dossier.IfscCode = "KKBK0000182";
                    dossier.PaymentTerms = !string.IsNullOrWhiteSpace(v.PaymentTerms) ? v.PaymentTerms : "Net 15 Days";
                    dossier.LeadTimeDays = "1-3 Business Days";
                    dossier.DefectRatio = "1.42%";
                    dossier.PriceCompetitiveness = "91%";
                    dossier.RiskGrade = !string.IsNullOrWhiteSpace(v.RiskGrade) ? v.RiskGrade : "Moderate Risk (Grade B - Under Review)";
                    dossier.Certifications = new List<string>
                    {
                        "IATA Certified Air Cargo Handling Agent",
                        "ISO 28000:2007 Security Management Systems for Supply Chain",
                        "Customs Broker Authorized License No. DEL/041/2019",
                        "C-TPAT Supply Chain Security Compliant Facility"
                    };
                }
                else if (vn.Contains("global cloud"))
                {
                    dossier.Gstin = !string.IsNullOrWhiteSpace(v.Gstin) ? v.Gstin : "29AAACG9012R1Z4";
                    dossier.Pan = !string.IsNullOrWhiteSpace(v.Pan) ? v.Pan : "AAACG9012R";
                    dossier.UdyamRegNo = "UDYAM-KR-03-0089123";
                    dossier.CinNumber = "U72900KA2016FTC092841";
                    dossier.ContactPerson = !string.IsNullOrWhiteSpace(v.ContactPerson) ? v.ContactPerson : "Priya Sundaram (Head of Strategic Enterprise Accounts)";
                    dossier.ContactEmail = !string.IsNullOrWhiteSpace(v.ContactEmail) ? v.ContactEmail : "billing-apac@globalcloudcorp.com";
                    dossier.ContactPhone = !string.IsNullOrWhiteSpace(v.ContactPhone) ? v.ContactPhone : "+91 80 4910 8800";
                    dossier.RegisteredAddress = "Level 12, World Trade Center, Rajajinagar, Bangalore, Karnataka - 560055";
                    dossier.PlantLocation = "Cloud Infrastructure DC-1 (Equinix Tier-IV), Whitefield, Bangalore - 560066";
                    dossier.BankName = "Citibank N.A. (MG Road Corporate Branch, Bangalore)";
                    dossier.BankAccountMasked = "00548190241XX";
                    dossier.IfscCode = "CITI0000004";
                    dossier.PaymentTerms = !string.IsNullOrWhiteSpace(v.PaymentTerms) ? v.PaymentTerms : "Advance 100% / Annual License";
                    dossier.LeadTimeDays = "Instant Digital Provisioning";
                    dossier.DefectRatio = "0.01%";
                    dossier.PriceCompetitiveness = "96%";
                    dossier.RiskGrade = !string.IsNullOrWhiteSpace(v.RiskGrade) ? v.RiskGrade : "Low Risk (Grade AAA - Strategic Partner)";
                    dossier.Certifications = new List<string>
                    {
                        "SOC 1 / SOC 2 / SOC 3 Type II Certified",
                        "ISO/IEC 27001:2022 & ISO/IEC 27017 Cloud Security",
                        "CSA STAR Level 2 Continuous Certification",
                        "PCI-DSS Level 1 Service Provider",
                        "HIPAA & GDPR Ready Enterprise Cloud Enclave"
                    };
                }
                else
                {
                    // Fallback for custom or newly onboarded vendor
                    dossier.Gstin = !string.IsNullOrWhiteSpace(v.Gstin) ? v.Gstin : "29AAACE" + Math.Abs(v.Name.GetHashCode() % 9000 + 1000) + "F1Z5";
                    dossier.Pan = !string.IsNullOrWhiteSpace(v.Pan) ? v.Pan : "AAACE" + Math.Abs(v.Name.GetHashCode() % 9000 + 1000) + "F";
                    dossier.UdyamRegNo = "UDYAM-KR-03-00" + Math.Abs(v.Name.GetHashCode() % 90000 + 10000);
                    dossier.CinNumber = "U" + Math.Abs(v.Name.GetHashCode() % 90000 + 10000) + "KA2020PTC099182";
                    dossier.ContactPerson = !string.IsNullOrWhiteSpace(v.ContactPerson) ? v.ContactPerson : "Anil Sharma (Commercial Director)";
                    dossier.ContactEmail = !string.IsNullOrWhiteSpace(v.ContactEmail) ? v.ContactEmail : ("procurement@" + v.Name.ToLower().Replace(" ", "").Replace(".", "") + ".com");
                    dossier.ContactPhone = !string.IsNullOrWhiteSpace(v.ContactPhone) ? v.ContactPhone : "+91 98450 71092";
                    dossier.RegisteredAddress = "Corporate Industrial Park, Sector 4, Bangalore, Karnataka - 560068";
                    dossier.PlantLocation = "Logistics Bay & Fabrication Center, Nelamangala Industrial Corridor - 562123";
                    dossier.BankName = "HDFC Bank Ltd (Commercial Banking Division)";
                    dossier.BankAccountMasked = "50100084719XX";
                    dossier.IfscCode = "HDFC0000412";
                    dossier.PaymentTerms = !string.IsNullOrWhiteSpace(v.PaymentTerms) ? v.PaymentTerms : "Net 30 Days";
                    dossier.LeadTimeDays = "4-7 Business Days";
                    dossier.DefectRatio = "0.25%";
                    dossier.PriceCompetitiveness = "94%";
                    dossier.RiskGrade = !string.IsNullOrWhiteSpace(v.RiskGrade) ? v.RiskGrade : "Low Risk (Grade A)";
                    dossier.Certifications = new List<string>
                    {
                        "ISO 9001:2015 Quality Management System",
                        "RoHS Compliant Materials & Assembly",
                        "GSTN Authorized E-Invoice Direct Integrator"
                    };
                }

                // Attach REAL Purchase Orders placed with this supplier from the active registry
                var matchingOrders = Instance.PurchaseOrders
                    .Where(p => p.VendorName.Equals(v.Name, StringComparison.OrdinalIgnoreCase) ||
                                (!string.IsNullOrWhiteSpace(p.VendorName) && v.Name.Contains(p.VendorName, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(p.VendorName) && p.VendorName.Contains(v.Name, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(p => p.OrderDate)
                    .ToList();

                if (matchingOrders.Any())
                {
                    dossier.RecentOrders = matchingOrders.Select(o => new VendorAuditOrderItem
                    {
                        PONumber = o.PONumber,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        ItemsCount = o.ItemsCount,
                        PaymentTerms = o.PaymentTerms
                    }).ToList();
                }
                else
                {
                    dossier.RecentOrders = new List<VendorAuditOrderItem>
                    {
                        new VendorAuditOrderItem { PONumber = "PO-2026-0891", OrderDate = "29 Aug 2026", TotalAmount = "₹ 4,20,000.00", Status = "In Transit", ItemsCount = 12, PaymentTerms = "Net 30" },
                        new VendorAuditOrderItem { PONumber = "PO-2026-0850", OrderDate = "14 Jul 2026", TotalAmount = "₹ 2,80,000.00", Status = "Delivered", ItemsCount = 5, PaymentTerms = "Net 30" }
                    };
                }

                // Realistic SRM audit history events
                dossier.AuditHistory = new List<VendorAuditFindingItem>
                {
                    new VendorAuditFindingItem
                    {
                        Date = "14 Aug 2026",
                        AuditType = "Comprehensive Annual SRM Surveillance",
                        Auditor = "Rajesh Khanna (Lead Auditor)",
                        Score = "98.4%",
                        Result = "Passed - Full Certification Retained",
                        Notes = "Zero critical defects. SLA delivery turnaround variance within 2.1% allowable tolerance."
                    },
                    new VendorAuditFindingItem
                    {
                        Date = "18 Feb 2026",
                        AuditType = "Statutory GSTN & E-Way Bill Reconciliation",
                        Auditor = "Pooja Hegde (Compliance Officer)",
                        Score = "100%",
                        Result = "Verified & Reconciled",
                        Notes = "100% GSTR-2B compliance; zero tax credit disputes across all fiscal invoices."
                    },
                    new VendorAuditFindingItem
                    {
                        Date = "05 Nov 2025",
                        AuditType = "On-site Facility & ESG Inspection",
                        Auditor = "Vikram Malhotra (Quality Lead)",
                        Score = "96.5%",
                        Result = "Grade A Validated",
                        Notes = "Clean dispatch bays, robust raw material quarantine workflow, complete safety gear adherence."
                    }
                };

                return dossier;
            }
        }

        public static List<GRNItem> GetReceipts()
        {
            lock (_lock)
            {
                return Instance.GoodsReceipts.ToList();
            }
        }

        public static GRNItem? GetReceiptByNumber(string grnNumber)
        {
            lock (_lock)
            {
                return Instance.GoodsReceipts.FirstOrDefault(g => g.GRNNumber.Equals(grnNumber, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static bool UpdateGRNStatus(string grnNumber, string status, string? remarks = null)
        {
            lock (_lock)
            {
                var grn = Instance.GoodsReceipts.FirstOrDefault(g => g.GRNNumber.Equals(grnNumber, StringComparison.OrdinalIgnoreCase));
                if (grn != null)
                {
                    grn.Status = status;
                    if (!string.IsNullOrWhiteSpace(remarks))
                    {
                        grn.InspectorRemarks = remarks;
                    }

                    // If released from quarantine, update line items
                    if (grn.Items != null && grn.Items.Any())
                    {
                        foreach (var item in grn.Items)
                        {
                            if (status == "Quality Quarantine")
                            {
                                item.AcceptedQty = 0;
                                item.QuarantineQty = item.ReceivedQty;
                                item.Verdict = "QUARANTINE HOLD";
                            }
                            else
                            {
                                item.AcceptedQty = item.ReceivedQty;
                                item.QuarantineQty = 0;
                                item.Verdict = status == "Completed & Stocked" ? "STOCKED (101)" : "PASSED QC";
                            }
                        }
                    }

                    SaveDataInternal();
                    return true;
                }
                return false;
            }
        }

        public static GRNSlipDetailViewModel GetGRNSlipDetails(string grnNumber)
        {
            lock (_lock)
            {
                var grn = Instance.GoodsReceipts.FirstOrDefault(g => g.GRNNumber.Equals(grnNumber, StringComparison.OrdinalIgnoreCase))
                          ?? new GRNItem
                          {
                              GRNNumber = grnNumber ?? "GRN-2026-0411",
                              Status = "Inspected & Accepted",
                              PONumber = "PO-2026-0893",
                              VendorName = "National Paper Mills",
                              ReceivedDate = DateTime.Now.ToString("dd MMM yyyy"),
                              Warehouse = "WH-Main Bay A",
                              ItemsReceived = 50,
                              InspectedBy = "Rajesh K."
                          };

                var po = Instance.PurchaseOrders.FirstOrDefault(p => p.PONumber.Equals(grn.PONumber, StringComparison.OrdinalIgnoreCase));
                if (po != null)
                {
                    EnsurePOEnriched(po);
                }

                var dossier = GetVendorAuditDossier(grn.VendorName);

                var slip = new GRNSlipDetailViewModel
                {
                    GRNNumber = grn.GRNNumber,
                    Status = grn.Status,
                    PONumber = grn.PONumber,
                    VendorName = grn.VendorName,
                    VendorGstin = !string.IsNullOrWhiteSpace(dossier?.Gstin) ? dossier.Gstin : (!string.IsNullOrWhiteSpace(po?.VendorGstin) ? po.VendorGstin : "29AAACE1234F1Z8"),
                    VendorAddress = !string.IsNullOrWhiteSpace(dossier?.RegisteredAddress) ? dossier.RegisteredAddress : (!string.IsNullOrWhiteSpace(po?.VendorAddress) ? po.VendorAddress : "Industrial Logistics Corridor, Bangalore - 560100"),
                    ReceivedDate = grn.ReceivedDate,
                    Warehouse = grn.Warehouse,
                    StorageBin = !string.IsNullOrWhiteSpace(grn.StorageBin) ? grn.StorageBin : GetDefaultStorageBin(grn.Warehouse),
                    ReceivingGate = !string.IsNullOrWhiteSpace(grn.ReceivingGate) ? grn.ReceivingGate : (!string.IsNullOrWhiteSpace(po?.ReceivingGate) ? po.ReceivingGate : "North Terminal Intake Gate #2"),
                    ItemsReceived = grn.ItemsReceived,
                    InspectedBy = grn.InspectedBy,
                    DeliveryChallanNo = !string.IsNullOrWhiteSpace(grn.DeliveryChallanNo) ? grn.DeliveryChallanNo : ("DC-2026-" + Math.Abs((grn.GRNNumber + grn.PONumber).GetHashCode() % 89999 + 10000)),
                    ChallanDate = !string.IsNullOrWhiteSpace(grn.ReceivedDate) ? grn.ReceivedDate : DateTime.Now.ToString("dd MMM yyyy"),
                    Carrier = !string.IsNullOrWhiteSpace(grn.Carrier) ? grn.Carrier : (!string.IsNullOrWhiteSpace(po?.Carrier) ? po.Carrier : "BlueDart Express Freight (Vehicle: KA-01-EA-9821)"),
                    VehicleNumber = !string.IsNullOrWhiteSpace(grn.VehicleNumber) ? grn.VehicleNumber : GetDefaultVehicleNumber(grn.GRNNumber),
                    SapMovementType = "101 (Goods Receipt for Purchase Order into Storage)",
                    BillOfLading = "BL-" + Math.Abs(grn.GRNNumber.GetHashCode() % 89999 + 10000) + "-IN"
                };

                // Determine 3-Way match status & remarks based on Quality Status
                if (grn.Status.Equals("Quality Quarantine", StringComparison.OrdinalIgnoreCase))
                {
                    slip.ThreeWayMatchStatus = "Quality Hold: 3-Way Match Blocked (Payment Hold until QA Clearance)";
                    slip.Variance = "Pending QA Lab Test";
                    slip.InspectorRemarks = !string.IsNullOrWhiteSpace(grn.InspectorRemarks)
                        ? grn.InspectorRemarks
                        : "Consignment quarantined under QA quarantine protocol. Initial tolerance check identified variance requiring secondary metallurgical/mechanical calibration sign-off.";
                }
                else if (grn.Status.Equals("3-Way Matched", StringComparison.OrdinalIgnoreCase))
                {
                    slip.ThreeWayMatchStatus = "100% Matched: PO, Delivery Challan & Vendor Bill Fully Aligned";
                    slip.Variance = "0.00%";
                    slip.InspectorRemarks = !string.IsNullOrWhiteSpace(grn.InspectorRemarks)
                        ? grn.InspectorRemarks
                        : "Consignment fully matched across PO, physical receipt, and vendor commercial e-invoice. Zero quantity or price variance. Authorized for automated AP disbursement.";
                }
                else if (grn.Status.Equals("Completed & Stocked", StringComparison.OrdinalIgnoreCase))
                {
                    slip.ThreeWayMatchStatus = "100% Matched: Stock Movement 101 Posted & Binned";
                    slip.Variance = "0.00%";
                    slip.InspectorRemarks = !string.IsNullOrWhiteSpace(grn.InspectorRemarks)
                        ? grn.InspectorRemarks
                        : "Consignment thoroughly verified, serial numbers catalogued, and items binned in designated warehouse racks for immediate enterprise allocation.";
                }
                else
                {
                    slip.ThreeWayMatchStatus = "100% Matched (PO, GRN & Invoice Aligned)";
                    slip.Variance = "0.00%";
                    slip.InspectorRemarks = !string.IsNullOrWhiteSpace(grn.InspectorRemarks)
                        ? grn.InspectorRemarks
                        : "Physical count and quality verification completed with zero defect tolerance. Package seals intact upon arrival.";
                }

                // Generate or use authentic line items
                slip.Items = GenerateGRNLineItems(grn, po);

                return slip;
            }
        }

        private static List<GRNLineItem> GenerateGRNLineItems(GRNItem grn, PurchaseOrderItem? po)
        {
            if (grn.Items != null && grn.Items.Any())
            {
                return grn.Items;
            }

            var list = new List<GRNLineItem>();
            string vn = (grn.VendorName ?? "").ToLower();
            string grnNo = grn.GRNNumber ?? "";
            bool isQuarantine = grn.Status.Equals("Quality Quarantine", StringComparison.OrdinalIgnoreCase);
            bool isStocked = grn.Status.Equals("Completed & Stocked", StringComparison.OrdinalIgnoreCase);

            if (grnNo.Contains("411") || vn.Contains("national paper"))
            {
                list.Add(new GRNLineItem
                {
                    Item = "JK Copier A4 Paper 75 GSM High-Speed Printing Cartons",
                    Specification = "Super White 98% Brightness, Moisture-Proof Wrapped (50 Reams per Ctn)",
                    Hsn = "4802",
                    PoQty = 30,
                    ReceivedQty = 30,
                    AcceptedQty = isQuarantine ? 0 : 30,
                    QuarantineQty = isQuarantine ? 30 : 0,
                    RejectedQty = 0,
                    Unit = "Cartons",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (BIN A-12)" : "PASSED QC"),
                    Remarks = isQuarantine ? "Moisture test pending" : "Grammage 75.2 GSM verified"
                });
                list.Add(new GRNLineItem
                {
                    Item = "Heavy-Duty Double Wall Corrugated Packaging Shipping Boxes",
                    Specification = "3-Ply Kraft 200 GSM Bursting Strength, 450 x 300 x 300 mm (1,000 Units)",
                    Hsn = "4819",
                    PoQty = 20,
                    ReceivedQty = 20,
                    AcceptedQty = isQuarantine ? 0 : 20,
                    QuarantineQty = isQuarantine ? 20 : 0,
                    RejectedQty = 0,
                    Unit = "Bundles",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (BIN A-15)" : "PASSED QC"),
                    Remarks = isQuarantine ? "Bursting test pending" : "Bursting strength 14.2 kg/cm² verified"
                });
            }
            else if (grnNo.Contains("410") || vn.Contains("steelcraft"))
            {
                list.Add(new GRNLineItem
                {
                    Item = "Fe-550D TMT High-Tensile Structural Steel Rebar Rods (16mm)",
                    Specification = "Primary Billet Produced conforming to IS 1786:2008 Grade Fe-550D (10 Ton)",
                    Hsn = "7214",
                    PoQty = 70,
                    ReceivedQty = 70,
                    AcceptedQty = isQuarantine ? 0 : 70,
                    QuarantineQty = isQuarantine ? 70 : 0,
                    RejectedQty = 0,
                    Unit = "Bundles",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (YARD B3)" : "PASSED QC"),
                    Remarks = isQuarantine ? "Yield strength test pending" : "Heat H8821 tensile strength > 585 N/mm² verified"
                });
                list.Add(new GRNLineItem
                {
                    Item = "Hot-Rolled Structural Heavy Steel Equal Angles & Channels",
                    Specification = "Grade IS 2062 E250A Quality, Anti-Corrosion Primer Coated (Bundle packs)",
                    Hsn = "7216",
                    PoQty = 50,
                    ReceivedQty = 50,
                    AcceptedQty = isQuarantine ? 0 : 50,
                    QuarantineQty = isQuarantine ? 50 : 0,
                    RejectedQty = 0,
                    Unit = "Metric Tons",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (YARD B4)" : "PASSED QC"),
                    Remarks = isQuarantine ? "MTC test pending" : "IS 2062 Grade E250 conformity certificate attached"
                });
            }
            else if (grnNo.Contains("409") || vn.Contains("apex"))
            {
                list.Add(new GRNLineItem
                {
                    Item = "Bosch Professional Rotary Hammer & Demolition Drill Kits",
                    Specification = "GBH 2-28 F Heavy Duty SDS-Plus with Vibration Control 880W",
                    Hsn = "8205",
                    PoQty = 2,
                    ReceivedQty = 2,
                    AcceptedQty = isQuarantine ? 0 : 2,
                    QuarantineQty = isQuarantine ? 2 : 0,
                    RejectedQty = 0,
                    Unit = "Sets",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (TOOL VAULT)" : "PASSED QC"),
                    Remarks = isQuarantine ? "Electrical insulation & grounding test pending" : "Passed electrical safety audit"
                });
                list.Add(new GRNLineItem
                {
                    Item = "Hydraulic Heavy Hand Pallet Truck (2.5 Ton Capacity)",
                    Specification = "Tandem Polyurethane Roller Wheels, Ergonomic 3-Position Lever",
                    Hsn = "8427",
                    PoQty = 1,
                    ReceivedQty = 1,
                    AcceptedQty = isQuarantine ? 0 : 1,
                    QuarantineQty = isQuarantine ? 1 : 0,
                    RejectedQty = 0,
                    Unit = "Units",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (DOCK 3)" : "PASSED QC"),
                    Remarks = isQuarantine ? "Load pressure test in progress (Holding Cell 3)" : "Hydraulic seal and pressure tested"
                });
                list.Add(new GRNLineItem
                {
                    Item = "Industrial Digital Torque Wrench Set & Calibration Rig",
                    Specification = "Range 40-200 Nm, Microprocessor Controlled with Buzzer & LED",
                    Hsn = "9031",
                    PoQty = 1,
                    ReceivedQty = 1,
                    AcceptedQty = isQuarantine ? 0 : 1,
                    QuarantineQty = isQuarantine ? 1 : 0,
                    RejectedQty = 0,
                    Unit = "Kits",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (CAL LAB)" : "PASSED QC"),
                    Remarks = isQuarantine ? "Torque calibration certificate re-verification requested" : "Recalibration certificate verified"
                });
            }
            else if (grnNo.Contains("408") || vn.Contains("techinfra"))
            {
                list.Add(new GRNLineItem
                {
                    Item = "Dell UltraSharp 27\" 4K UHD Monitors (U2723QE)",
                    Specification = "Model U2723QE, USB-C Hub 90W PD, IPS Black Technology 2000:1 Contrast",
                    Hsn = "8471",
                    PoQty = 10,
                    ReceivedQty = 10,
                    AcceptedQty = isQuarantine ? 0 : 10,
                    QuarantineQty = isQuarantine ? 10 : 0,
                    RejectedQty = 0,
                    Unit = "Units",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : "STOCKED (BIN IT-04)",
                    Remarks = "Zero dead pixel test passed, serial numbers asset tagged"
                });
                list.Add(new GRNLineItem
                {
                    Item = "Cisco Catalyst 1000 Series 24-Port Managed Switch",
                    Specification = "C1000-24T-4G-L Gigabit Ethernet with 4x 1G SFP Uplinks Layer 2+",
                    Hsn = "8517",
                    PoQty = 10,
                    ReceivedQty = 10,
                    AcceptedQty = isQuarantine ? 0 : 10,
                    QuarantineQty = isQuarantine ? 10 : 0,
                    RejectedQty = 0,
                    Unit = "Units",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : "STOCKED (BIN IT-02)",
                    Remarks = "Firmware v15.2 validated & MAC addresses recorded"
                });
                list.Add(new GRNLineItem
                {
                    Item = "Dell PowerEdge R450 Server Expansion RAM (64GB DDR4)",
                    Specification = "3200MHz Registered ECC RDIMM Low Latency Server Memory",
                    Hsn = "8471",
                    PoQty = 5,
                    ReceivedQty = 5,
                    AcceptedQty = isQuarantine ? 0 : 5,
                    QuarantineQty = isQuarantine ? 5 : 0,
                    RejectedQty = 0,
                    Unit = "Kits",
                    Verdict = isQuarantine ? "QUARANTINE HOLD" : "STOCKED (VAULT-A)",
                    Remarks = "Anti-static seal intact; ECC diagnostic check passed"
                });
            }
            else
            {
                // Fallback: use PO line items if available
                if (po != null && po.Items != null && po.Items.Any())
                {
                    foreach (var pi in po.Items)
                    {
                        list.Add(new GRNLineItem
                        {
                            Item = pi.Item,
                            Specification = pi.Specification,
                            Hsn = pi.Hsn,
                            PoQty = pi.Qty,
                            ReceivedQty = pi.Qty,
                            AcceptedQty = isQuarantine ? 0 : pi.Qty,
                            QuarantineQty = isQuarantine ? pi.Qty : 0,
                            RejectedQty = 0,
                            Unit = "Units",
                            Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (101)" : "PASSED QC"),
                            Remarks = isQuarantine ? "Holding for QA clearance" : "Inspected and verified against PO"
                        });
                    }
                }
                else
                {
                    int qty = grn.ItemsReceived > 0 ? grn.ItemsReceived : 25;
                    list.Add(new GRNLineItem
                    {
                        Item = !string.IsNullOrWhiteSpace(grn.VendorName) ? ($"{grn.VendorName} Procured Consignment Goods") : "Procurement Materials & Line Supplies",
                        Specification = "Grade-A Technical Conformance verified against PO standards",
                        Hsn = "8471",
                        PoQty = qty,
                        ReceivedQty = qty,
                        AcceptedQty = isQuarantine ? 0 : qty,
                        QuarantineQty = isQuarantine ? qty : 0,
                        RejectedQty = 0,
                        Unit = "Units",
                        Verdict = isQuarantine ? "QUARANTINE HOLD" : (isStocked ? "STOCKED (101)" : "PASSED QC"),
                        Remarks = isQuarantine ? "Awaiting QA clearance" : "Passed physical inspection and quantity count"
                    });
                }
            }

            return list;
        }

        private static string GetDefaultStorageBin(string? warehouse)
        {
            if (string.IsNullOrWhiteSpace(warehouse)) return "BAY-A-RACK-04";
            if (warehouse.Contains("Raw", StringComparison.OrdinalIgnoreCase)) return "YARD-B-ZONE-3";
            if (warehouse.Contains("QC", StringComparison.OrdinalIgnoreCase) || warehouse.Contains("Quarantine", StringComparison.OrdinalIgnoreCase)) return "HOLDING-CELL-Q1";
            if (warehouse.Contains("IT", StringComparison.OrdinalIgnoreCase)) return "IT-VAULT-SEC-02";
            return "BAY-A-RACK-04";
        }

        private static string GetDefaultVehicleNumber(string grnNo)
        {
            if (grnNo.Contains("411")) return "MH-04-AZ-4112";
            if (grnNo.Contains("410")) return "GJ-01-BX-7729";
            if (grnNo.Contains("409")) return "KA-05-MM-1892";
            if (grnNo.Contains("408")) return "DL-01-TF-3341";
            return "KA-01-EA-9821";
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

        public static decimal ParseMoney(string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return 0m;
            var digitsOnly = new string(val.Where(c => char.IsDigit(c) || c == '.').ToArray());
            if (decimal.TryParse(digitsOnly, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal res))
            {
                return res;
            }
            return 0m;
        }

        public static string FormatRupees(decimal val)
        {
            return "₹ " + val.ToString("N2", new System.Globalization.CultureInfo("en-IN"));
        }

        public static bool EnsurePOEnriched(PurchaseOrderItem po)
        {
            if (po == null) return false;
            bool changed = false;

            if (string.IsNullOrWhiteSpace(po.VendorName)) { po.VendorName = "TechInfra Solutions Ltd"; changed = true; }

            if (string.IsNullOrWhiteSpace(po.VendorGstin) || string.IsNullOrWhiteSpace(po.VendorPan))
            {
                var v = po.VendorName;
                if (v.Contains("TechInfra", StringComparison.OrdinalIgnoreCase))
                {
                    po.VendorGstin = "29AAACT9821K1Z5";
                    po.VendorPan = "AAACT9821K";
                    po.VendorEmail = "procurement@techinfra.com";
                    po.VendorPhone = "+91 80 4122 8900";
                    po.VendorAddress = "TechInfra Innovation Hub, Electronic City Phase 1, Bangalore - 560100";
                }
                else if (v.Contains("Apex", StringComparison.OrdinalIgnoreCase))
                {
                    po.VendorGstin = "27AAACA1245P1Z8";
                    po.VendorPan = "AAACA1245P";
                    po.VendorEmail = "orders@apexsupplies.in";
                    po.VendorPhone = "+91 22 6789 4432";
                    po.VendorAddress = "Plot 42, MIDC Industrial Area, Andheri East, Mumbai - 400093";
                }
                else if (v.Contains("National Paper", StringComparison.OrdinalIgnoreCase))
                {
                    po.VendorGstin = "07AAACN5678Q1Z2";
                    po.VendorPan = "AAACN5678Q";
                    po.VendorEmail = "corporate@nationalpapermills.com";
                    po.VendorPhone = "+91 11 2341 9870";
                    po.VendorAddress = "Paper Mill Complex, Okhla Industrial Area Phase II, New Delhi - 110020";
                }
                else if (v.Contains("SteelCraft", StringComparison.OrdinalIgnoreCase))
                {
                    po.VendorGstin = "24AAACS3412M1Z0";
                    po.VendorPan = "AAACS3412M";
                    po.VendorEmail = "dispatch@steelcraftmetals.co.in";
                    po.VendorPhone = "+91 79 2658 1120";
                    po.VendorAddress = "Heavy Fabrication Zone, GIDC Estate, Vatva, Ahmedabad - 382445";
                }
                else if (v.Contains("Global Cloud", StringComparison.OrdinalIgnoreCase))
                {
                    po.VendorGstin = "33AAACG7890N1Z9";
                    po.VendorPan = "AAACG7890N";
                    po.VendorEmail = "enterprise-licensing@globalcloudcorp.com";
                    po.VendorPhone = "+91 44 4901 2200";
                    po.VendorAddress = "Tidel Park, 7th Floor, Rajiv Gandhi Salai, Taramani, Chennai - 600113";
                }
                else if (v.Contains("Delta Logistics", StringComparison.OrdinalIgnoreCase))
                {
                    po.VendorGstin = "29AAACD4567L1Z4";
                    po.VendorPan = "AAACD4567L";
                    po.VendorEmail = "supplychain@deltalogistics.in";
                    po.VendorPhone = "+91 80 2839 5566";
                    po.VendorAddress = "Logistics Hub 5, Peenya Industrial Area 3rd Phase, Bangalore - 560058";
                }
                else
                {
                    po.VendorGstin = "29AAACE" + Math.Abs(po.VendorName.GetHashCode() % 8999 + 1000) + "K1Z9";
                    po.VendorPan = "AAACE" + Math.Abs(po.VendorName.GetHashCode() % 8999 + 1000) + "K";
                    po.VendorEmail = "sales@" + po.VendorName.ToLower().Replace(" ", "").Replace(".", "") + ".com";
                    po.VendorPhone = "+91 98450 78219";
                    po.VendorAddress = "Silicon Valley Tech Quarter, Sector 4, Bangalore - 560100";
                }
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(po.ShippingAddress))
            {
                po.ShippingAddress = "Operafy Central Logistics Depot, Warehouse Bay 3, Nelamangala Logistics Park, Bangalore - 562123";
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(po.Warehouse)) { po.Warehouse = "WH-Main Logistics Bay 3"; changed = true; }
            if (string.IsNullOrWhiteSpace(po.ReceivingGate)) { po.ReceivingGate = "Commercial Inward Intake Gate #2"; changed = true; }
            if (string.IsNullOrWhiteSpace(po.DeliveryDate))
            {
                po.DeliveryDate = !string.IsNullOrWhiteSpace(po.OrderDate) && DateTime.TryParse(po.OrderDate, out var od)
                    ? od.AddDays(7).ToString("dd MMM yyyy")
                    : DateTime.Now.AddDays(7).ToString("dd MMM yyyy");
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(po.Carrier))
            {
                po.Carrier = po.Status == "In Transit" ? "BlueDart Express Freight (Vehicle: KA-01-EA-9821)" : "V-Trans Multi-Modal Logistics (Vehicle: MH-04-AZ-4112)";
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(po.TrackingNumber))
            {
                po.TrackingNumber = "TRK-" + (po.PONumber ?? "").Replace("PO-", "").Replace("PO", "") + "-IN";
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(po.AuthorizedSignatory))
            {
                po.AuthorizedSignatory = "Aftab Shaik (Chief Procurement Officer)";
                changed = true;
            }

            decimal totalVal = ParseMoney(po.TotalAmount);
            if (totalVal <= 0) { totalVal = 150000m; po.TotalAmount = FormatRupees(totalVal); changed = true; }
            decimal subtotalVal = Math.Round(totalVal / 1.18m, 2);
            decimal taxVal = totalVal - subtotalVal;

            if (string.IsNullOrWhiteSpace(po.Subtotal))
            {
                po.Subtotal = FormatRupees(subtotalVal);
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(po.TaxAmount))
            {
                po.TaxAmount = FormatRupees(taxVal);
                changed = true;
            }

            if (po.Items == null || !po.Items.Any())
            {
                po.Items = GenerateLineItemsForPO(po, subtotalVal, taxVal, totalVal);
                changed = true;
            }

            return changed;
        }

        private static List<POLineItem> GenerateLineItemsForPO(PurchaseOrderItem po, decimal subtotalVal, decimal taxVal, decimal totalVal)
        {
            var list = new List<POLineItem>();
            var v = po.VendorName ?? "";

            if (po.PONumber.Equals("PO-2026-0937", StringComparison.OrdinalIgnoreCase) || (v.Contains("TechInfra", StringComparison.OrdinalIgnoreCase) && po.TotalAmount.Contains("1,45,000")))
            {
                list.Add(new POLineItem
                {
                    Item = "Dell UltraSharp 27\" 4K UHD Monitors (U2723QE)",
                    Specification = "Model U2723QE, USB-C Hub 90W PD, IPS Black Technology 2000:1 Contrast",
                    Hsn = "8471",
                    Qty = 5,
                    UnitPrice = FormatRupees(Math.Round(subtotalVal / 5, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(subtotalVal)
                });
            }
            else if (v.Contains("TechInfra", StringComparison.OrdinalIgnoreCase))
            {
                decimal item1Base = Math.Round(subtotalVal * 0.476m, 2);
                decimal item2Base = Math.Round(subtotalVal * 0.324m, 2);
                decimal item3Base = subtotalVal - item1Base - item2Base;

                list.Add(new POLineItem
                {
                    Item = "Cisco Catalyst 1000 Series 24-Port Managed Switch",
                    Specification = "C1000-24T-4G-L Gigabit Ethernet with 4x 1G SFP Uplinks Layer 2+",
                    Hsn = "8517",
                    Qty = 4,
                    UnitPrice = FormatRupees(Math.Round(item1Base / 4, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item1Base)
                });
                list.Add(new POLineItem
                {
                    Item = "Dell PowerEdge R450 Server Expansion RAM (64GB DDR4)",
                    Specification = "3200MHz Registered ECC RDIMM Low Latency Server Memory",
                    Hsn = "8471",
                    Qty = 4,
                    UnitPrice = FormatRupees(Math.Round(item2Base / 4, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item2Base)
                });
                list.Add(new POLineItem
                {
                    Item = "Category-6e Shielded Gigabit Copper Network Spools (305m)",
                    Specification = "Pure Bare Solid Copper 600MHz High Bandwidth LSZH Sheathed",
                    Hsn = "8544",
                    Qty = 4,
                    UnitPrice = FormatRupees(Math.Round(item3Base / 4, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item3Base)
                });
            }
            else if (v.Contains("Apex", StringComparison.OrdinalIgnoreCase))
            {
                decimal item1Base = Math.Round(subtotalVal * 0.422m, 2);
                decimal item2Base = Math.Round(subtotalVal * 0.383m, 2);
                decimal item3Base = subtotalVal - item1Base - item2Base;

                list.Add(new POLineItem
                {
                    Item = "Bosch Professional Rotary Hammer & Demolition Drill Kits",
                    Specification = "GBH 2-28 F Heavy Duty SDS-Plus with Vibration Control 880W",
                    Hsn = "8205",
                    Qty = 2,
                    UnitPrice = FormatRupees(Math.Round(item1Base / 2, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item1Base)
                });
                list.Add(new POLineItem
                {
                    Item = "Hydraulic Heavy Hand Pallet Truck (2.5 Ton Capacity)",
                    Specification = "Tandem Polyurethane Roller Wheels, Ergonomic 3-Position Lever",
                    Hsn = "8427",
                    Qty = 1,
                    UnitPrice = FormatRupees(item2Base),
                    Tax = "18% IGST",
                    Total = FormatRupees(item2Base)
                });
                list.Add(new POLineItem
                {
                    Item = "Industrial Digital Torque Wrench Set & Calibration Rig",
                    Specification = "Range 40-200 Nm, Microprocessor Controlled with Buzzer & LED",
                    Hsn = "9031",
                    Qty = 1,
                    UnitPrice = FormatRupees(item3Base),
                    Tax = "18% IGST",
                    Total = FormatRupees(item3Base)
                });
            }
            else if (v.Contains("National Paper", StringComparison.OrdinalIgnoreCase))
            {
                decimal item1Base = Math.Round(subtotalVal * 0.646m, 2);
                decimal item2Base = subtotalVal - item1Base;

                list.Add(new POLineItem
                {
                    Item = "JK Copier A4 Paper 75 GSM High-Speed Printing Cartons",
                    Specification = "Super White 98% Brightness, Moisture-Proof Wrapped (50 Reams per Ctn)",
                    Hsn = "4802",
                    Qty = 30,
                    UnitPrice = FormatRupees(Math.Round(item1Base / 30, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item1Base)
                });
                list.Add(new POLineItem
                {
                    Item = "Heavy-Duty Double Wall Corrugated Packaging Shipping Boxes",
                    Specification = "3-Ply Kraft 200 GSM Bursting Strength, 450 x 300 x 300 mm (1,000 Units)",
                    Hsn = "4819",
                    Qty = 20,
                    UnitPrice = FormatRupees(Math.Round(item2Base / 20, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item2Base)
                });
            }
            else if (v.Contains("SteelCraft", StringComparison.OrdinalIgnoreCase))
            {
                decimal item1Base = Math.Round(subtotalVal * 0.720m, 2);
                decimal item2Base = subtotalVal - item1Base;

                list.Add(new POLineItem
                {
                    Item = "Fe-550D TMT High-Tensile Structural Steel Rebar Rods",
                    Specification = "16mm Diameter, Primary Billet Produced conforming to IS 1786:2008 (10 Ton)",
                    Hsn = "7214",
                    Qty = 10,
                    UnitPrice = FormatRupees(Math.Round(item1Base / 10, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item1Base)
                });
                list.Add(new POLineItem
                {
                    Item = "Hot-Rolled Structural Heavy Steel Equal Angles & Channels",
                    Specification = "Grade IS 2062 E250A Quality, Rust-Inhibited Primer Coated",
                    Hsn = "7216",
                    Qty = 8,
                    UnitPrice = FormatRupees(Math.Round(item2Base / 8, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item2Base)
                });
            }
            else if (v.Contains("Global Cloud", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new POLineItem
                {
                    Item = "Enterprise Multi-Region Cloud Compute Cluster & Database",
                    Specification = "Annual Reserved Tier: 64 vCPU, 256GB RAM, High-Throughput NVMe IOPS SLA 99.99%",
                    Hsn = "998313",
                    Qty = 1,
                    UnitPrice = FormatRupees(subtotalVal),
                    Tax = "18% IGST",
                    Total = FormatRupees(subtotalVal)
                });
            }
            else if (v.Contains("Delta Logistics", StringComparison.OrdinalIgnoreCase))
            {
                decimal item1Base = Math.Round(subtotalVal * 0.717m, 2);
                decimal item2Base = subtotalVal - item1Base;

                list.Add(new POLineItem
                {
                    Item = "Real-Time GPS Fleet Vehicle OBD-II Telematics Tracker Nodes",
                    Specification = "4G LTE Telematics unit with CAN-bus sensor telemetry and emergency alerts",
                    Hsn = "9032",
                    Qty = 6,
                    UnitPrice = FormatRupees(Math.Round(item1Base / 6, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item1Base)
                });
                list.Add(new POLineItem
                {
                    Item = "Heavy-Duty Cargo Ratchet Tie-Down Straps (50mm x 10m)",
                    Specification = "5000 kg Break Strength, High-Tenacity Polyester Webbing with Double J Hooks",
                    Hsn = "5607",
                    Qty = 2,
                    UnitPrice = FormatRupees(Math.Round(item2Base / 2, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(item2Base)
                });
            }
            else
            {
                string desc = !string.IsNullOrWhiteSpace(po.ItemSummary) ? po.ItemSummary : "Procurement Materials & Line Supplies";
                int qty = po.ItemsCount > 0 ? po.ItemsCount : 1;
                list.Add(new POLineItem
                {
                    Item = desc,
                    Specification = "Standard Technical Specification Grade A, Conforming to Operafy Procurement Guidelines",
                    Hsn = "8471",
                    Qty = qty,
                    UnitPrice = FormatRupees(Math.Round(subtotalVal / qty, 2)),
                    Tax = "18% IGST",
                    Total = FormatRupees(subtotalVal)
                });
            }

            return list;
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
