using System.Collections.Generic;

namespace ERP_System.Models
{
    public class HolidayViewModel
    {
        public HRHoliday Holiday { get; set; } = new HRHoliday();
        public List<HRHoliday> Holidays { get; set; } = new List<HRHoliday>();
        public List<Branch> Branches { get; set; } = new List<Branch>();

        public int SelectedYear { get; set; }
        public int SelectedBranchId { get; set; }
        public string SelectedView { get; set; } = "list"; // "list" or "calendar"
        
        // KPI summary metrics
        public int TotalHolidaysCount { get; set; }
        public int MandatoryCount { get; set; }
        public int OptionalCount { get; set; }
        public HRHoliday? NextUpcomingHoliday { get; set; }
    }
}
