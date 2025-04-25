using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using WebApplication3.Models;
using static WebApplication3.Models.HRModel;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class HRController : Controller
    {
        private static List<JobPosting> _jobPostings = new List<JobPosting>();
        private readonly string savedJobsPath = Path.Combine("wwwroot", "previous files", "SavedJobPostings.json");

        public HRController()
        {
            // Load job postings when controller is instantiated
            _jobPostings = LoadJobPostingsFromFile();
        }

        // Main index page redirects to ManageJobs
        public IActionResult Index()
        {
            return RedirectToAction("ManageJobs");
        }

        // Manage Jobs
        public IActionResult ManageJobs(string sortOrder = "Id")
        {
            var currentPostings = _jobPostings.Where(j => j.IsActive && j.ExpiryDate > DateTime.Now).ToList();
            var previousPostings = _jobPostings.Where(j => !j.IsActive || j.ExpiryDate <= DateTime.Now).ToList();

            switch (sortOrder)
            {
                case "Title":
                    currentPostings = currentPostings.OrderBy(j => j.Title).ToList();
                    previousPostings = previousPostings.OrderBy(j => j.Title).ToList();
                    break;
                case "PostingDate":
                    currentPostings = currentPostings.OrderBy(j => j.PostingDate).ToList();
                    previousPostings = previousPostings.OrderBy(j => j.PostingDate).ToList();
                    break;
                case "ExpiryDate":
                    currentPostings = currentPostings.OrderBy(j => j.ExpiryDate).ToList();
                    previousPostings = previousPostings.OrderBy(j => j.ExpiryDate).ToList();
                    break;
                case "OrderId":
                    currentPostings = currentPostings.OrderBy(j => j.OrderId).ToList();
                    previousPostings = previousPostings.OrderBy(j => j.OrderId).ToList();
                    break;
                default:
                    currentPostings = currentPostings.OrderBy(j => j.Id).ToList();
                    previousPostings = previousPostings.OrderBy(j => j.Id).ToList();
                    break;
            }

            // Set the sort order in ViewBag
            ViewBag.SortOrder = sortOrder;

            var model = new JobPostingsModel
            {
                CurrentPostings = currentPostings,
                PreviousPostings = previousPostings
            };

            return View(model);
        }

        // Add Job
        public IActionResult AddJob()
        {
            return View(new JobPosting { PostingDate = DateTime.Now, ExpiryDate = DateTime.Now.AddDays(30) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddJob(JobPosting job)
        {
            job.Id = _jobPostings.Count == 0 ? 1 : _jobPostings.Max(j => j.Id) + 1;
            _jobPostings.Add(job);
            SaveJobPostingsToFile();
            return RedirectToAction("ManageJobs");
        }

        // Edit Job
        public IActionResult EditJob(int id)
        {
            var job = _jobPostings.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();
            return View(job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditJob(JobPosting updatedJob)
        {
            var job = _jobPostings.FirstOrDefault(j => j.Id == updatedJob.Id);
            if (job == null) return NotFound();

            job.Title = updatedJob.Title;
            job.Summary = updatedJob.Summary;
            job.Details = updatedJob.Details;
            job.ApplyLink = updatedJob.ApplyLink;
            job.Location = updatedJob.Location;
            job.Category = updatedJob.Category;
            job.SalaryRange = updatedJob.SalaryRange;
            job.PostingDate = updatedJob.PostingDate;
            job.ExpiryDate = updatedJob.ExpiryDate;
            job.IsActive = updatedJob.IsActive;
            job.OrderId = updatedJob.OrderId;

            SaveJobPostingsToFile();
            return RedirectToAction("ManageJobs");
        }

        // Delete Job
        public IActionResult DeleteJob(int id)
        {
            var job = _jobPostings.FirstOrDefault(j => j.Id == id);
            if (job != null)
            {
                _jobPostings.Remove(job);
                SaveJobPostingsToFile();
            }
            return RedirectToAction("ManageJobs");
        }

        [HttpGet]
        public IActionResult ManageBaseHR()
        {
            // Fetch only active job postings
            var model = new JobPostingsModel
            {
                CurrentPostings = _jobPostings.Where(j => j.IsActive).ToList() // Only active jobs
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult GenerateBaseHR(List<int> selectedJobIds, string sortOrder = "Id")
        {
            // Filter selected active jobs
            var selectedJobs = _jobPostings
                .Where(j => j.IsActive && selectedJobIds.Contains(j.Id)) // Ensure only active jobs are processed
                .ToList();

            // Generate HTML for selected jobs only
            string htmlContent = GenerateBaseHRHtml(selectedJobs, sortOrder);

            // Return the generated HTML as a downloadable file
            return File(Encoding.UTF8.GetBytes(htmlContent), "text/html", "orderhr.html");
        }

        private string GenerateBaseHRHtml(List<JobPosting> jobPostings, string sortOrder)
        {
            // Apply sorting
            switch (sortOrder)
            {
                case "Title":
                    jobPostings = jobPostings.OrderBy(j => j.Title).ToList();
                    break;
                case "PostingDate":
                    jobPostings = jobPostings.OrderBy(j => j.PostingDate).ToList();
                    break;
                case "ExpiryDate":
                    jobPostings = jobPostings.OrderBy(j => j.ExpiryDate).ToList();
                    break;
                case "OrderId":
                    jobPostings = jobPostings.OrderBy(j => j.OrderId).ToList();
                    break;
                default:
                    jobPostings = jobPostings.OrderBy(j => j.Id).ToList();
                    break;
            }

            // Read the base template
            string templatePath = Path.Combine("wwwroot", "templates", "basehr.html");
            string baseHtml = System.IO.File.ReadAllText(templatePath);

            // Build the job postings
            if (jobPostings != null && jobPostings.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var job in jobPostings)
                {
                    sb.AppendLine($@"
                <div class='job-summary' onclick='toggleDetails(""job{job.Id}"")' 
                     style='background-color: #cbcbcb; color: #000000; padding: 20px; margin-bottom: 10px; border-radius: 8px; cursor: pointer; position: relative; transition: background-color 0.3s ease; box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.2);'>
                    <h2 style='margin: 0; font-size: 20px;'>{job.Title}</h2>
                    <p style='margin: 10px 0 0; font-size: 16px;'>{job.Summary}</p>
                    <div class='expand-icon' style='position: absolute; right: 20px; top: 20px; font-size: 18px; border: 1px solid #000000; border-radius: 50%; padding: 5px; width: 24px; height: 24px; display: flex; justify-content: center; align-items: center;'>+</div>
                </div>
                <div class='job-details' id='job{job.Id}' 
                     style='display: none; background-color: #cbcbcb; color: #000000; padding: 20px; margin-bottom: 10px; border-radius: 8px;'>
                    <h3 style='font-size: 18px; margin-top: 0;'>Details:</h3>
                    <p>{job.Details}</p>
                    <a href='{job.ApplyLink}' class='submitLink' 
                       style='display: inline-block; background-color: #ff0000; color: #ffffff; padding: 10px 20px; border: 2px solid #FF0000; font-size: 16px; font-weight: bold; border-radius: 4px; text-align: center; text-decoration: none; cursor: pointer; transition: background-color 0.3s ease, color 0.3s ease;'>Apply Now</a>
                </div>");
                }

                // Replace placeholder with job postings
                baseHtml = baseHtml.Replace("<!--JOB_POSTINGS_PLACEHOLDER-->", sb.ToString());
            }
            else
            {
                // Remove the section if no job postings exist
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_JOB_POSTINGS-->", "<!--END_JOB_POSTINGS-->");
            }

            return baseHtml;
        }

        // Utility method to save job postings to file
        private void SaveJobPostingsToFile()
        {
            var json = JsonSerializer.Serialize(_jobPostings, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(savedJobsPath, json);
        }

        // Utility method to load job postings from file
        private List<JobPosting> LoadJobPostingsFromFile()
        {
            if (System.IO.File.Exists(savedJobsPath))
            {
                var json = System.IO.File.ReadAllText(savedJobsPath);
                return JsonSerializer.Deserialize<List<JobPosting>>(json) ?? new List<JobPosting>();
            }
            return new List<JobPosting>();
        }

        // Utility method to remove sections from HTML
        private string RemoveSection(string html, string startMarker, string endMarker)
        {
            int startIndex = html.IndexOf(startMarker);
            if (startIndex == -1) return html; // If marker not found, return unchanged

            int endIndex = html.IndexOf(endMarker, startIndex);
            if (endIndex == -1) return html; // If end marker not found, return unchanged

            return html.Remove(startIndex, (endIndex + endMarker.Length) - startIndex);
        }

        [HttpPost]
        public IActionResult UpdateJobOrder([FromBody] Dictionary<string, int> orderData)
        {
            try
            {
                foreach (var item in orderData)
                {
                    int jobId = int.Parse(item.Key);
                    int newOrder = item.Value;

                    var job = _jobPostings.FirstOrDefault(j => j.Id == jobId);
                    if (job != null)
                    {
                        job.OrderId = newOrder;
                    }
                }

                SaveJobPostingsToFile();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PreviewBaseHR(List<int> selectedJobIds, string sortOrder = "Id")
        {
            // Filter selected active jobs
            var selectedJobs = _jobPostings
                .Where(j => j.IsActive && selectedJobIds.Contains(j.Id))
                .ToList();

            // Apply custom sorting if needed (based on OrderId or the provided sortOrder)
            switch (sortOrder)
            {
                case "Title":
                    selectedJobs = selectedJobs.OrderBy(j => j.Title).ToList();
                    break;
                case "PostingDate":
                    selectedJobs = selectedJobs.OrderBy(j => j.PostingDate).ToList();
                    break;
                case "ExpiryDate":
                    selectedJobs = selectedJobs.OrderBy(j => j.ExpiryDate).ToList();
                    break;
                case "OrderId":
                    selectedJobs = selectedJobs.OrderBy(j => j.OrderId).ToList();
                    break;
                default:
                    selectedJobs = selectedJobs.OrderBy(j => j.OrderId).ToList();
                    break;
            }

            // Generate HTML for selected jobs only
            string htmlContent = GenerateBaseHRHtml(selectedJobs, sortOrder);

            // Return just the HTML content
            return Content(htmlContent, "text/html");
        }

        public IActionResult JobDashboard()
        {
            // Sample data - in a real application, you would calculate these from your database
            var model = new HRModel.JobDashboardViewModel
            {
                ActiveJobsCount = _jobPostings.Count(j => j.IsActive && j.ExpiryDate > DateTime.Now),
                ExpiringJobsCount = _jobPostings.Count(j => j.IsActive && j.ExpiryDate > DateTime.Now && j.ExpiryDate < DateTime.Now.AddDays(7)),
                TotalJobViews = 1254,
                TotalApplications = 85,

                ActiveJobsChange = 2,
                ApplicationsIncrease = 15,

                ApplicationTrendLabels = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" },
                ApplicationTrendData = new List<int> { 5, 8, 12, 9, 6, 3, 4 },

                CategoriesLabels = new List<string> { "Development", "Marketing", "Management", "Sales", "Support", "Other" },
                CategoriesData = new List<int> { 8, 5, 3, 4, 2, 1 },

                RecentJobs = _jobPostings
                    .Where(j => j.IsActive || j.ExpiryDate > DateTime.Now.AddDays(-30))
                    .OrderByDescending(j => j.PostingDate)
                    .Take(5)
                    .Select(j => new HRModel.DashboardJobViewModel
                    {
                        Id = j.Id,
                        Title = j.Title,
                        Summary = j.Summary,
                        Category = j.Category ?? "General",
                        Location = j.Location ?? "Not specified",
                        PostingDate = j.PostingDate,
                        ExpiryDate = j.ExpiryDate,
                        IsActive = j.IsActive,
                        Views = new Random().Next(50, 500),  // Random sample data
                        Applications = new Random().Next(5, 30)  // Random sample data
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateJobStatus([FromBody] JobStatusUpdateModel model)
        {
            try
            {
                var job = _jobPostings.FirstOrDefault(j => j.Id == model.JobId);
                if (job == null)
                {
                    return Json(new { success = false, message = "Job not found" });
                }

                job.IsActive = model.IsActive;
                SaveJobPostingsToFile();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class JobStatusUpdateModel
        {
            public int JobId { get; set; }
            public bool IsActive { get; set; }
        }

        [HttpGet]
        public IActionResult UploadGuide()
        {
            return View();
        }
    }
}