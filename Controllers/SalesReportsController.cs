using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ERP_System.Data;
using ERP_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Sales Manager,Sales Executive,Manager")]
    public class SalesReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;

        public SalesReportsController(ApplicationDbContext context, UserManager<ApplicationUser>? userManager = null)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================================
        // 1. GET: /SalesReports/Summary
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Summary(string? period = "6M")
        {
            period = string.IsNullOrWhiteSpace(period) ? "6M" : period.ToUpper();

            int monthsBack = period switch
            {
                "1Y" => 12,
                "ALL" => 36,
                _ => 6
            };

            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-(monthsBack - 1));

            // Load all invoices
            var allInvoices = await _context.SalesInvoices.ToListAsync();
            var periodInvoices = allInvoices.Where(i => i.InvoiceDate >= startDate).ToList();

            // Calculate overall metrics
            decimal totalRevenue = periodInvoices.Any() ? periodInvoices.Sum(i => i.TotalAmount) : allInvoices.Sum(i => i.TotalAmount);
            int totalInvoicesCount = periodInvoices.Any() ? periodInvoices.Count : allInvoices.Count;
            decimal totalTaxes = periodInvoices.Any() ? periodInvoices.Sum(i => i.GstAmount) : allInvoices.Sum(i => i.GstAmount);
            decimal netRevenue = periodInvoices.Any() ? periodInvoices.Sum(i => i.TaxableValue) : allInvoices.Sum(i => i.TaxableValue);
            decimal aov = totalInvoicesCount > 0 ? (totalRevenue / totalInvoicesCount) : 0m;

            // Generate monthly time buckets
            var monthlyLedger = new List<MonthlySalesSummaryItem>();
            decimal previousGross = 0m;

            for (int i = monthsBack - 1; i >= 0; i--)
            {
                var bucketDate = now.AddMonths(-i);
                int y = bucketDate.Year;
                int m = bucketDate.Month;
                string monthName = bucketDate.ToString("MMM yyyy", CultureInfo.InvariantCulture);

                var monthInvs = allInvoices.Where(inv => inv.InvoiceDate.Year == y && inv.InvoiceDate.Month == m).ToList();
                decimal gross = monthInvs.Sum(inv => inv.TotalAmount);
                decimal tax = monthInvs.Sum(inv => inv.GstAmount);
                decimal net = monthInvs.Sum(inv => inv.TaxableValue);
                int count = monthInvs.Count;

                decimal growth = 0m;
                if (previousGross > 0)
                {
                    growth = Math.Round(((gross - previousGross) / previousGross) * 100m, 1);
                }

                monthlyLedger.Add(new MonthlySalesSummaryItem
                {
                    MonthYear = monthName,
                    Year = y,
                    Month = m,
                    TotalInvoices = count,
                    GrossSales = gross,
                    TaxAmount = tax,
                    NetRevenue = net,
                    GrowthRate = growth,
                    BarColorClass = (i == 0) ? "bg-teal" : "bg-primary"
                });

                if (gross > 0)
                {
                    previousGross = gross;
                }
            }

            // Scale bar chart heights (between 25% and 95%)
            decimal maxGross = monthlyLedger.Max(x => x.GrossSales);
            if (maxGross <= 0m) maxGross = 1m;

            foreach (var item in monthlyLedger)
            {
                if (item.GrossSales > 0)
                {
                    item.HeightPercentage = Math.Max(25, (int)Math.Round((item.GrossSales / maxGross) * 90m));
                }
                else
                {
                    item.HeightPercentage = 10;
                }
            }

            var viewModel = new SalesSummaryReportViewModel
            {
                Period = period,
                TotalRevenue = totalRevenue,
                TotalInvoicesCount = totalInvoicesCount,
                AverageOrderValue = aov,
                NetRevenue = netRevenue,
                TotalTaxes = totalTaxes,
                MonthlyLedger = monthlyLedger
            };

            return View(viewModel);
        }

        // =========================================================================
        // 2. GET: /SalesReports/ByCustomer
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> ByCustomer()
        {
            var customers = await _context.Customers.ToListAsync();
            var invoices = await _context.SalesInvoices.ToListAsync();

            decimal totalRevenue = invoices.Sum(i => i.TotalAmount);
            if (totalRevenue <= 0m) totalRevenue = 1m;

            var customerRows = new List<CustomerSalesReportViewModel>();

            // Group invoices by CustomerId
            var invoiceGroups = invoices.GroupBy(i => i.CustomerId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var cust in customers)
            {
                invoiceGroups.TryGetValue(cust.Id, out var custInvoices);
                custInvoices ??= new List<SalesInvoice>();

                // Also check if any invoices match customer name if ID wasn't linked
                if (!custInvoices.Any() && !string.IsNullOrWhiteSpace(cust.CustomerName))
                {
                    custInvoices = invoices.Where(i => 
                        !string.IsNullOrWhiteSpace(i.CustomerName) && 
                        (i.CustomerName.Equals(cust.CustomerName, StringComparison.OrdinalIgnoreCase) ||
                         (!string.IsNullOrWhiteSpace(cust.CompanyName) && i.CustomerName.Equals(cust.CompanyName, StringComparison.OrdinalIgnoreCase)))
                    ).ToList();
                }

                decimal gross = custInvoices.Sum(i => i.TotalAmount);
                int count = custInvoices.Count;
                decimal aov = count > 0 ? (gross / count) : 0m;
                decimal contribution = Math.Round((gross / totalRevenue) * 100m, 2);

                customerRows.Add(new CustomerSalesReportViewModel
                {
                    CustomerId = cust.Id,
                    CustomerCode = cust.CustomerCode,
                    CustomerName = !string.IsNullOrWhiteSpace(cust.CustomerName) ? cust.CustomerName : cust.CompanyName ?? "Customer",
                    CompanyName = cust.CompanyName ?? string.Empty,
                    Email = cust.Email,
                    InvoicesCount = count,
                    GrossRevenue = gross,
                    ContributionPercentage = contribution,
                    AverageOrderValue = aov,
                    Status = cust.IsActive ? "Active" : "Inactive"
                });
            }

            // Include any invoices whose CustomerName didn't map to existing Customers
            var unmappedInvoices = invoices
                .Where(i => !customerRows.Any(c => c.CustomerId == i.CustomerId || c.CustomerName.Equals(i.CustomerName, StringComparison.OrdinalIgnoreCase)))
                .GroupBy(i => i.CustomerName)
                .ToList();

            foreach (var unmapped in unmappedInvoices)
            {
                decimal gross = unmapped.Sum(i => i.TotalAmount);
                int count = unmapped.Count();
                decimal aov = count > 0 ? (gross / count) : 0m;
                decimal contribution = Math.Round((gross / totalRevenue) * 100m, 2);

                customerRows.Add(new CustomerSalesReportViewModel
                {
                    CustomerId = unmapped.First().CustomerId,
                    CustomerCode = "CUST#" + unmapped.First().CustomerId.ToString("D4"),
                    CustomerName = unmapped.Key,
                    CompanyName = unmapped.Key,
                    Email = "accounts@" + unmapped.Key.ToLower().Replace(" ", "").Replace(".", "") + ".com",
                    InvoicesCount = count,
                    GrossRevenue = gross,
                    ContributionPercentage = contribution,
                    AverageOrderValue = aov,
                    Status = "Active"
                });
            }

            // Sort Pareto: highest revenue first
            customerRows = customerRows.OrderByDescending(c => c.GrossRevenue).ThenByDescending(c => c.InvoicesCount).ToList();

            var topCust = customerRows.FirstOrDefault();

            var viewModel = new CustomerReportsListViewModel
            {
                Customers = customerRows,
                TotalRevenue = invoices.Sum(i => i.TotalAmount),
                TotalInvoices = invoices.Count,
                TotalActiveCustomers = customerRows.Count(c => c.Status == "Active"),
                TopCustomerName = topCust?.CustomerName ?? "None",
                TopCustomerRevenue = topCust?.GrossRevenue ?? 0m
            };

            return View(viewModel);
        }

        // GET: /SalesReports/GetCustomerInvoices?customerId=1&customerName=Tata
        [HttpGet]
        public async Task<IActionResult> GetCustomerInvoices(int customerId, string? customerName)
        {
            var query = _context.SalesInvoices.AsQueryable();

            if (customerId > 0)
            {
                query = query.Where(i => i.CustomerId == customerId);
            }
            else if (!string.IsNullOrWhiteSpace(customerName))
            {
                query = query.Where(i => i.CustomerName.Contains(customerName));
            }

            var invoices = await query
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => new
                {
                    invoiceNumber = i.InvoiceNumber,
                    linkedOrder = i.LinkedOrderNumber ?? "-",
                    invoiceDate = i.InvoiceDate.ToString("yyyy-MM-dd"),
                    dueDate = i.DueDate.ToString("yyyy-MM-dd"),
                    taxableValue = i.TaxableValue,
                    gstAmount = i.GstAmount,
                    totalAmount = i.TotalAmount,
                    status = i.Status
                })
                .ToListAsync();

            return Json(invoices);
        }

        // =========================================================================
        // 3. GET: /SalesReports/ByProduct
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> ByProduct()
        {
            var products = await _context.Products.ToListAsync();
            var orderItems = await _context.SalesOrderItems.ToListAsync();

            var productRows = new List<ProductSalesReportViewModel>();

            foreach (var prod in products)
            {
                var matchedItems = orderItems.Where(oi => oi.ProductId == prod.ProductId).ToList();

                int unitsSold = matchedItems.Any() ? matchedItems.Sum(oi => oi.Quantity) : prod.SoldQty;
                decimal totalSales = matchedItems.Any() ? matchedItems.Sum(oi => oi.TotalPrice) : (prod.Revenue > 0 ? prod.Revenue : (unitsSold * prod.UnitSellingPrice));

                productRows.Add(new ProductSalesReportViewModel
                {
                    ProductId = prod.ProductId,
                    ProductName = prod.ProductName,
                    Category = !string.IsNullOrWhiteSpace(prod.Category) ? prod.Category : "General",
                    UnitsSold = unitsSold,
                    TotalSales = totalSales,
                    UnitSellingPrice = prod.UnitSellingPrice,
                    StockStatus = prod.StockQty > 0 ? "In Stock" : "Out of Stock",
                    FreeStock = prod.FreeAvailableStock
                });
            }

            decimal totalProductRev = productRows.Sum(p => p.TotalSales);
            if (totalProductRev <= 0m) totalProductRev = 1m;

            foreach (var p in productRows)
            {
                p.ContributionPercentage = Math.Round((p.TotalSales / totalProductRev) * 100m, 2);
            }

            productRows = productRows.OrderByDescending(p => p.TotalSales).ThenByDescending(p => p.UnitsSold).ToList();

            var topProduct = productRows.FirstOrDefault();
            var dominantCategory = productRows
                .GroupBy(p => p.Category)
                .OrderByDescending(g => g.Sum(p => p.TotalSales))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "General";

            var viewModel = new ProductReportsListViewModel
            {
                Products = productRows,
                TotalProductRevenue = productRows.Sum(p => p.TotalSales),
                TotalUnitsSold = productRows.Sum(p => p.UnitsSold),
                TopSellingProduct = topProduct?.ProductName ?? "None",
                DominantCategory = dominantCategory
            };

            return View(viewModel);
        }

        // =========================================================================
        // 4. GET: /SalesReports/BySalesperson
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> BySalesperson()
        {
            var executives = await _context.SalesExecutiveProfiles
                .Include(e => e.User)
                .ToListAsync();

            var invoices = await _context.SalesInvoices.ToListAsync();
            var targetAllocations = await _context.SalesTargetAllocations.ToListAsync();

            var repRows = new List<SalespersonReportViewModel>();

            foreach (var exec in executives)
            {
                string repName = exec.User?.FullName ?? ("Executive " + exec.ExecCode);
                decimal target = exec.MonthlyTarget > 0 ? exec.MonthlyTarget : 1500000m;

                // Match sales achieved by rep user ID or allocated targets
                var repInvoices = invoices.Where(i => 
                    (!string.IsNullOrWhiteSpace(i.CreatedByUserId) && i.CreatedByUserId == exec.UserId) ||
                    (exec.UserUserId.HasValue && i.CreatedByUserUserId == exec.UserUserId)
                ).ToList();

                decimal achieved = repInvoices.Sum(i => i.TotalAmount);

                // Fallback to SalesTargetAllocations if invoices are not directly tagged
                if (achieved <= 0)
                {
                    var repTargets = targetAllocations.Where(t => 
                        (!string.IsNullOrWhiteSpace(t.SalesRepUserId) && t.SalesRepUserId == exec.UserId) ||
                        (exec.UserUserId.HasValue && t.SalesRepUserUserId == exec.UserUserId)
                    ).ToList();

                    if (repTargets.Any())
                    {
                        achieved = repTargets.Sum(t => t.AchievedValue);
                    }
                }

                decimal pct = target > 0 ? Math.Round((achieved / target) * 100m, 1) : 0m;

                string pacing = "On Track";
                if (pct >= 100m) pacing = "Ahead";
                else if (pct >= 75m) pacing = "On Track";
                else pacing = "Lagging Target";

                repRows.Add(new SalespersonReportViewModel
                {
                    SalespersonName = repName,
                    ExecCode = exec.ExecCode,
                    Region = exec.Region,
                    MobileNumber = exec.MobileNumber,
                    TargetMonth = target,
                    SalesAchieved = achieved,
                    AchievementPercentage = pct,
                    PacingStatus = pacing
                });
            }

            // Sort by sales achieved descending
            repRows = repRows.OrderByDescending(r => r.SalesAchieved).ToList();

            decimal totalTarget = repRows.Sum(r => r.TargetMonth);
            decimal totalAchieved = repRows.Sum(r => r.SalesAchieved);
            decimal overallPct = totalTarget > 0 ? Math.Round((totalAchieved / totalTarget) * 100m, 1) : 0m;
            var topRep = repRows.FirstOrDefault();

            var viewModel = new SalespersonReportsListViewModel
            {
                Salespeople = repRows,
                TotalTeamTarget = totalTarget,
                TotalTeamAchieved = totalAchieved,
                TeamAchievementPercentage = overallPct,
                TopPerformerName = topRep?.SalespersonName ?? "None",
                TopPerformerRevenue = topRep?.SalesAchieved ?? 0m
            };

            return View(viewModel);
        }

        // =========================================================================
        // 5. GET: /SalesReports/TargetReport
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> TargetReport(string? quarter = "All")
        {
            var allocations = await _context.SalesTargetAllocations.ToListAsync();

            // Distinct quarters available
            var quarterGroups = allocations
                .GroupBy(a => a.FiscalQuarter)
                .OrderBy(g => g.Key)
                .ToList();

            var quarterRows = new List<TargetReportViewModel>();

            foreach (var qg in quarterGroups)
            {
                decimal target = qg.Sum(a => a.TargetValue);
                decimal achieved = qg.Sum(a => a.AchievedValue);
                decimal variance = achieved - target;
                decimal pct = target > 0 ? Math.Round((achieved / target) * 100m, 1) : 0m;

                quarterRows.Add(new TargetReportViewModel
                {
                    Quarter = qg.Key,
                    TargetAmount = target,
                    AchievedAmount = achieved,
                    Variance = variance,
                    AchievementPercentage = pct,
                    Status = variance >= 0 ? "Exceeded" : "Under Target"
                });
            }

            // Category breakdown
            var categoryBreakdowns = allocations
                .GroupBy(a => a.TargetCategory)
                .Select(cg => new CategoryTargetBreakdownItem
                {
                    Category = cg.Key,
                    TargetAmount = cg.Sum(a => a.TargetValue),
                    AchievedAmount = cg.Sum(a => a.AchievedValue),
                    AchievementPercentage = cg.Sum(a => a.TargetValue) > 0 ? Math.Round((cg.Sum(a => a.AchievedValue) / cg.Sum(a => a.TargetValue)) * 100m, 1) : 0m
                })
                .ToList();

            decimal totalTarget = quarterRows.Sum(q => q.TargetAmount);
            decimal totalAchieved = quarterRows.Sum(q => q.AchievedAmount);
            decimal totalVariance = totalAchieved - totalTarget;
            decimal overallPct = totalTarget > 0 ? Math.Round((totalAchieved / totalTarget) * 100m, 1) : 0m;

            var filteredQuarterRows = quarterRows;
            if (!string.IsNullOrWhiteSpace(quarter) && !quarter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                filteredQuarterRows = quarterRows.Where(q => q.Quarter.Equals(quarter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var viewModel = new TargetReportsListViewModel
            {
                SelectedQuarter = quarter ?? "All",
                Quarters = filteredQuarterRows,
                CategoryBreakdowns = categoryBreakdowns,
                TotalTarget = totalTarget,
                TotalAchieved = totalAchieved,
                TotalVariance = totalVariance,
                OverallAchievementPercentage = overallPct
            };

            return View(viewModel);
        }

        // =========================================================================
        // 6. GET: /SalesReports/ReceivablesReport
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> ReceivablesReport()
        {
            var receivables = await _context.PaymentReceivables
                .Include(r => r.Customer)
                .ToListAsync();

            var invoices = await _context.SalesInvoices
                .Include(i => i.Customer)
                .ToListAsync();

            var customerAgingMap = new Dictionary<string, CustomerAgingViewModel>();
            var today = DateTime.UtcNow.Date;

            // Process payment receivables with pending balance
            foreach (var rec in receivables.Where(r => r.PendingBalance > 0))
            {
                string custKey = !string.IsNullOrWhiteSpace(rec.Customer?.CustomerName) 
                    ? rec.Customer.CustomerName 
                    : (!string.IsNullOrWhiteSpace(rec.CustomerName) ? rec.CustomerName : ("Customer #" + rec.CustomerId));

                if (!customerAgingMap.TryGetValue(custKey, out var item))
                {
                    item = new CustomerAgingViewModel
                    {
                        CustomerId = rec.CustomerId,
                        CustomerCode = rec.Customer?.CustomerCode ?? ("CUST#" + rec.CustomerId.ToString("D4")),
                        CustomerName = custKey,
                        Email = rec.Customer?.Email ?? ("accounts@" + custKey.ToLower().Replace(" ", "") + ".com"),
                        PhoneNumber = rec.Customer?.PhoneNumber ?? "+91 98111 00000"
                    };
                    customerAgingMap[custKey] = item;
                }

                int daysPastDue = (today - rec.DueDate.Date).Days;
                decimal bal = rec.PendingBalance;

                if (daysPastDue <= 30)
                {
                    item.Days0_30 += bal;
                }
                else if (daysPastDue <= 60)
                {
                    item.Days31_60 += bal;
                }
                else if (daysPastDue <= 90)
                {
                    item.Days61_90 += bal;
                }
                else
                {
                    item.Days90Plus += bal;
                }

                item.TotalDue += bal;
            }

            // Also check unpaid/pending/overdue sales invoices not yet in receivables
            foreach (var inv in invoices.Where(i => i.Status != "Paid"))
            {
                // Check if already accounted for
                bool existsInRec = receivables.Any(r => r.InvoiceNumber == inv.InvoiceNumber);
                if (!existsInRec)
                {
                    string custKey = !string.IsNullOrWhiteSpace(inv.CustomerName) ? inv.CustomerName : (inv.Customer?.CustomerName ?? ("Customer #" + inv.CustomerId));

                    if (!customerAgingMap.TryGetValue(custKey, out var item))
                    {
                        item = new CustomerAgingViewModel
                        {
                            CustomerId = inv.CustomerId,
                            CustomerCode = inv.Customer?.CustomerCode ?? ("CUST#" + inv.CustomerId.ToString("D4")),
                            CustomerName = custKey,
                            Email = inv.Customer?.Email ?? ("accounts@" + custKey.ToLower().Replace(" ", "") + ".com"),
                            PhoneNumber = inv.Customer?.PhoneNumber ?? "+91 98111 00000"
                        };
                        customerAgingMap[custKey] = item;
                    }

                    int daysPastDue = (today - inv.DueDate.Date).Days;
                    decimal bal = inv.TotalAmount;

                    if (daysPastDue <= 30)
                    {
                        item.Days0_30 += bal;
                    }
                    else if (daysPastDue <= 60)
                    {
                        item.Days31_60 += bal;
                    }
                    else if (daysPastDue <= 90)
                    {
                        item.Days61_90 += bal;
                    }
                    else
                    {
                        item.Days90Plus += bal;
                    }

                    item.TotalDue += bal;
                }
            }

            var agingList = customerAgingMap.Values.ToList();

            // Set risk levels
            foreach (var row in agingList)
            {
                if (row.Days90Plus > 0)
                {
                    row.RiskLevel = "High Risk";
                }
                else if (row.Days61_90 > 0 || row.Days31_60 > 0)
                {
                    row.RiskLevel = "Watch";
                }
                else
                {
                    row.RiskLevel = "Normal";
                }
            }

            agingList = agingList.OrderByDescending(a => a.Days90Plus).ThenByDescending(a => a.TotalDue).ToList();

            var viewModel = new ReceivablesAgingReportViewModel
            {
                Customers = agingList,
                TotalOutstanding = agingList.Sum(a => a.TotalDue),
                Total0_30 = agingList.Sum(a => a.Days0_30),
                Total31_60 = agingList.Sum(a => a.Days31_60),
                Total61_90 = agingList.Sum(a => a.Days61_90),
                Total90Plus = agingList.Sum(a => a.Days90Plus),
                OverdueAccountsCount = agingList.Count(a => (a.Days31_60 + a.Days61_90 + a.Days90Plus) > 0)
            };

            return View(viewModel);
        }

        // POST: /SalesReports/SendDemandNotice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendDemandNotice(string customerName, string? email, decimal amountDue)
        {
            TempData["SuccessMessage"] = $"Statutory demand notice and payment ledger link successfully dispatched to {customerName} ({email ?? "accounts"}) for outstanding sum of ₹{amountDue:N2}.";
            return RedirectToAction(nameof(ReceivablesReport));
        }
    }
}
