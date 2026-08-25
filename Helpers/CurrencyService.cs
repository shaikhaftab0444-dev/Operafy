using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;

namespace ERP_System.Helpers
{
    public interface ICurrencyService
    {
        RegionalConfiguration GetSettings();
        void RefreshCache();
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly ApplicationDbContext _context;
        private static RegionalConfiguration? _cachedSettings;
        private static readonly object _lock = new();

        public CurrencyService(ApplicationDbContext context)
        {
            _context = context;
            InitializeStaticReference();
        }

        private void InitializeStaticReference()
        {
            if (_cachedSettings == null)
            {
                lock (_lock)
                {
                    if (_cachedSettings == null)
                    {
                        try
                        {
                            _cachedSettings = _context.RegionalConfigurations.FirstOrDefault();
                            if (_cachedSettings == null)
                            {
                                _cachedSettings = new RegionalConfiguration
                                {
                                    Country = "India",
                                    CurrencyCode = "INR",
                                    CurrencySymbol = "₹",
                                    NumberSystem = "Lakhs/Crores",
                                    DateFormat = "DD/MM/YYYY",
                                    Timezone = "India Standard Time",
                                    TaxSystem = "GST",
                                    FinancialYearCycle = "April 1 - March 31"
                                };
                            }
                        }
                        catch
                        {
                            _cachedSettings = new RegionalConfiguration
                            {
                                Country = "India",
                                CurrencyCode = "INR",
                                CurrencySymbol = "₹",
                                NumberSystem = "Lakhs/Crores",
                                DateFormat = "DD/MM/YYYY",
                                Timezone = "India Standard Time",
                                TaxSystem = "GST",
                                FinancialYearCycle = "April 1 - March 31"
                            };
                        }
                    }
                }
            }
        }

        public RegionalConfiguration GetSettings()
        {
            InitializeStaticReference();
            return _cachedSettings!;
        }

        public void RefreshCache()
        {
            lock (_lock)
            {
                try
                {
                    _cachedSettings = _context.RegionalConfigurations.AsNoTracking().FirstOrDefault();
                }
                catch
                {
                    _cachedSettings = null;
                }
            }
        }

        public static string Format(decimal amount)
        {
            var settings = _cachedSettings ?? new RegionalConfiguration();
            bool isNegative = amount < 0;
            decimal absVal = Math.Abs(amount);

            string formatted;
            if (settings.NumberSystem == "Lakhs/Crores")
            {
                if (absVal >= 10000000m) // 1 Crore
                    formatted = $"{(absVal / 10000000m):F2} Cr";
                else if (absVal >= 100000m) // 1 Lakh
                    formatted = $"{(absVal / 100000m):F2} L";
                else if (absVal >= 10000m) // 10K
                {
                    decimal kVal = absVal / 1000m;
                    formatted = kVal % 1 == 0 ? $"{kVal:F0} K" : $"{kVal:F1} K";
                }
                else
                    formatted = absVal.ToString("N2", new System.Globalization.CultureInfo("en-IN"));
            }
            else
            {
                if (absVal >= 1000000000m) // 1 Billion
                    formatted = $"{(absVal / 1000000000m):F2} B";
                else if (absVal >= 1000000m) // 1 Million
                    formatted = $"{(absVal / 1000000m):F2} M";
                else if (absVal >= 10000m) // 10K
                {
                    decimal kVal = absVal / 1000m;
                    formatted = kVal % 1 == 0 ? $"{kVal:F0} K" : $"{kVal:F1} K";
                }
                else
                    formatted = absVal.ToString("N2", new System.Globalization.CultureInfo("en-US"));
            }

            string displaySymbol = settings.CurrencySymbol;
            return isNegative ? $"-{displaySymbol} {formatted}" : $"{displaySymbol} {formatted}";
        }

        public static string FormatDate(DateTime date)
        {
            var settings = _cachedSettings ?? new RegionalConfiguration();
            if (settings.DateFormat == "MM/DD/YYYY")
                return date.ToString("MM/dd/yyyy");
            if (settings.DateFormat == "YYYY-MM-DD")
                return date.ToString("yyyy-MM-dd");
            return date.ToString("dd/MM/yyyy");
        }
    }

    public static class CurrencyExtensions
    {
        public static string ToOrgCurrency(this decimal amount)
        {
            return CurrencyService.Format(amount);
        }

        public static string ToOrgCurrency(this double amount)
        {
            return CurrencyService.Format((decimal)amount);
        }

        public static string ToOrgDate(this DateTime date)
        {
            return CurrencyService.FormatDate(date);
        }
    }
}
