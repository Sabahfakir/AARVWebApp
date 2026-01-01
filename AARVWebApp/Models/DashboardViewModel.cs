using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AARVWebApp.Models
{
    public class DashboardViewModel
    {

        public DashboardViewModel()
        {
            RecentProjects = new List<RecentProjectItem>();
            Months = new List<string>();
            ProjectCount = new List<int>();
            StatusLabels = new List<string>();
            ProjectStatusCount = new List<int>();
        }

        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }

        public List<RecentProjectItem> RecentProjects { get; set; }

        public int TotalClients { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalCategories { get; set; }

        // === Chart 1: Projects Created Per Month ===
        public List<string> Months { get; set; }
        public List<int> ProjectCount { get; set; }

        // === Chart 2: Status Distribution ===
        public List<string> StatusLabels { get; set; }
        public List<int> ProjectStatusCount { get; set; }
    }

    public class RecentProjectItem
    {
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string ProjectManager { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? StartDate { get; internal set; }
        public string StatusClass { get; internal set; }
    }
}
