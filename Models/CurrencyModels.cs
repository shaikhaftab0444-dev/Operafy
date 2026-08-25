using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Currencies")]
    public class Currency
    {
        [Key]
        public int CurrencyId { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrencyCode { get; set; } = string.Empty; // e.g. USD, INR

        [Required]
        [StringLength(100)]
        public string CurrencyName { get; set; } = string.Empty; // e.g. US Dollar

        [Required]
        [StringLength(10)]
        public string Symbol { get; set; } = string.Empty; // e.g. $, ₹

        [Column(TypeName = "decimal(18, 6)")]
        public decimal ExchangeRate { get; set; } = 1.000000m; // Relative to Base Currency

        public int DecimalPlaces { get; set; } = 2; // Precision

        public bool IsActive { get; set; } = true;

        public bool IsBaseCurrency { get; set; } = false;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    [Table("erp_CurrencyRateHistories")]
    public class CurrencyRateHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        [ForeignKey("CurrencyId")]
        public Currency? Currency { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        public decimal ExchangeRate { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }

    public class CurrencyViewModel
    {
        public List<Currency> Currencies { get; set; } = new List<Currency>();
        
        public string BaseCurrencyCode { get; set; } = "INR";

        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency Code must be exactly 3 characters.")]
        public string CurrencyCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Currency Name cannot exceed 100 characters.")]
        public string CurrencyName { get; set; } = string.Empty;

        [Required]
        [StringLength(10, ErrorMessage = "Symbol cannot exceed 10 characters.")]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [Range(0.000001, 1000000.0, ErrorMessage = "Exchange Rate must be greater than zero.")]
        public decimal ExchangeRate { get; set; } = 1.000000m;

        [Required]
        public int DecimalPlaces { get; set; } = 2;

        public bool IsActive { get; set; } = true;
    }
}
