using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class UpcomingDeadlineVM
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string EmployeeName { get; set; }
        public string Priority { get; set; }
        public int DaysLeft { get; set; }
    }
}