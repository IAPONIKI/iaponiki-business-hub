using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication3.Models
{
    public class HRModel
    {
        public class JobPosting
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
            public string Details { get; set; }
            public string ApplyLink { get; set; }
            public string Location { get; set; }
            public DateTime PostingDate { get; set; }
            public DateTime ExpiryDate { get; set; }
            public string Category { get; set; }
            public string SalaryRange { get; set; }
            public bool IsActive { get; set; }
            public int OrderId { get; set; }
        }

        public class JobPostingsModel
        {
            public List<JobPosting> CurrentPostings { get; set; } = new();
            public List<JobPosting> PreviousPostings { get; set; } = new();
        }

        public List<JobPosting> JobPostings { get; set; } = new();

        public class JobDashboardViewModel
        {
            // Metrics
            public int ActiveJobsCount { get; set; }
            public int ExpiringJobsCount { get; set; }
            public int TotalJobViews { get; set; }
            public int TotalApplications { get; set; }

            // Change metrics
            public int ActiveJobsChange { get; set; }
            public int ApplicationsIncrease { get; set; }

            // Charts data
            public List<string> ApplicationTrendLabels { get; set; }
            public List<int> ApplicationTrendData { get; set; }
            public List<string> CategoriesLabels { get; set; }
            public List<int> CategoriesData { get; set; }

            // Recent jobs
            public List<DashboardJobViewModel> RecentJobs { get; set; }
        }

        public class DashboardJobViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
            public string Category { get; set; }
            public string Location { get; set; }
            public DateTime PostingDate { get; set; }
            public DateTime ExpiryDate { get; set; }
            public bool IsActive { get; set; }
            public int Views { get; set; }
            public int Applications { get; set; }
        }
    }
}
