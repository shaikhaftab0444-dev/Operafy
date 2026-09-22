using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_SalesCoordinatorProfiles")]
    public class SalesCoordinatorProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CoordinatorCode { get; set; } = string.Empty; // e.g. "SC-301"

        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;

        public int? UserUserId { get; set; }

        [ForeignKey("UserUserId")]
        public virtual User? User { get; set; }

        [Required]
        [StringLength(150)]
        public string PrimaryTerritory { get; set; } = "North Zone"; // e.g. "North Zone", "West & South Zone"

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // "Active", "Inactive"

        public virtual ICollection<SalesExecutiveProfile> AssignedExecutives { get; set; } = new List<SalesExecutiveProfile>();
    }

    [Table("erp_SalesExecutiveProfiles")]
    public class SalesExecutiveProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ExecCode { get; set; } = string.Empty; // e.g. "EX-104"

        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;

        public int? UserUserId { get; set; }

        [ForeignKey("UserUserId")]
        public virtual User? User { get; set; }

        [Required]
        [StringLength(100)]
        public string Region { get; set; } = "North India"; // "North India", "West India", "South India", "East India"

        [StringLength(25)]
        public string MobileNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyTarget { get; set; } = 1500000m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionPercentage { get; set; } = 2.5m;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // "Active", "On Leave", "Inactive"

        public int? AssignedCoordinatorId { get; set; }

        [ForeignKey("AssignedCoordinatorId")]
        public virtual SalesCoordinatorProfile? AssignedCoordinator { get; set; }
    }

    [Table("erp_SalesTargetAllocations")]
    public class SalesTargetAllocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FiscalQuarter { get; set; } = "Q3 2026"; // "Q1 2026", "Q2 2026", "Q3 2026", "Q4 2026"

        [Required]
        [StringLength(100)]
        public string TargetCategory { get; set; } = "Enterprise Invoices"; // "Enterprise Invoices", "Retail Counter", "New Customer Acquisitions"

        [StringLength(100)]
        public string? SalesRepUserId { get; set; } // Null if team-wide aggregate target

        public int? SalesRepUserUserId { get; set; }

        [ForeignKey("SalesRepUserUserId")]
        public virtual User? SalesRep { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AchievedValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SalesPerformanceLeaderboardItem
    {
        public int Rank { get; set; }
        public string ExecId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Target { get; set; }
        public decimal TargetAchievement { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal GapToTarget { get; set; }
        public int WonDeals { get; set; }
        public int TotalDeals { get; set; }
        public string Status { get; set; } = "Active";
    }
}
