using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using static WebApplication3.Models.HRModel;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        //[Authorize]
        public IActionResult Index()
        {
            // Get current user information
            var username = User.Identity?.Name;  // Returns DOMAIN\username
            ViewBag.Username = username;

            // Load active job count for display
            var jobsFilePath = Path.Combine("wwwroot", "previous files", "SavedJobPostings.json");
            if (System.IO.File.Exists(jobsFilePath))
            {
                var jobsJson = System.IO.File.ReadAllText(jobsFilePath);
                if (!string.IsNullOrWhiteSpace(jobsJson))
                {
                    var allJobs = JsonSerializer.Deserialize<System.Collections.Generic.List<JobPosting>>(jobsJson);
                    if (allJobs != null)
                    {
                        ViewBag.ActiveJobs = allJobs.Count(j => j.IsActive && j.ExpiryDate > System.DateTime.Now);
                    }
                }
            }

            // Load pending invoices count
            var pendingFilePath = Path.Combine("wwwroot", "previous files", "pending_payments.json");
            if (System.IO.File.Exists(pendingFilePath))
            {
                var pendingJson = System.IO.File.ReadAllText(pendingFilePath);
                if (!string.IsNullOrWhiteSpace(pendingJson))
                {
                    var pendingInvoices = JsonSerializer.Deserialize<System.Collections.Generic.List<ServiceReference1.InvoiceData>>(pendingJson);
                    ViewBag.PendingInvoices = pendingInvoices?.Count ?? 0;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Redirect old path to the new controller
        [HttpGet]
        public IActionResult IndexCarousel()
        {
            return RedirectToAction("Index", "Carousels");
        }

        // HR SECTION
        // Redirect HR paths to the HR controller
        public IActionResult ManageJobs(string sortOrder = "Id")
        {
            return RedirectToAction("ManageJobs", "HR", new { sortOrder });
        }

        public IActionResult AddJob()
        {
            return RedirectToAction("AddJob", "HR");
        }

        public IActionResult EditJob(int id)
        {
            return RedirectToAction("EditJob", "HR", new { id });
        }

        public IActionResult DeleteJob(int id)
        {
            return RedirectToAction("DeleteJob", "HR", new { id });
        }

        public IActionResult ManageBaseHR()
        {
            return RedirectToAction("ManageBaseHR", "HR");
        }

        public IActionResult GenerateBaseHR(System.Collections.Generic.List<int> selectedJobIds, string sortOrder = "Id")
        {
            return RedirectToAction("GenerateBaseHR", "HR", new { selectedJobIds, sortOrder });
        }

        // Redirects for Invoice functionality
        public IActionResult FetchInvoices(string startDate, string endDate)
        {
            return RedirectToAction("FetchInvoices", "Invoice", new { startDate, endDate });
        }

        public IActionResult PendingPayments()
        {
            return RedirectToAction("PendingPayments", "Invoice");
        }

        public IActionResult DownloadMatchedInvoices()
        {
            return RedirectToAction("DownloadMatchedInvoices", "Invoice");
        }

        public IActionResult DownloadPendingInvoices()
        {
            return RedirectToAction("DownloadPendingInvoices", "Invoice");
        }
    }
}