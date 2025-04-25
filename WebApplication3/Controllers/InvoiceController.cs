using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ServiceReference1;
using WebApplication3.Services;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin,Finance")]
    public class InvoiceController : Controller
    {
        private readonly string serviceUrl = "http://localhost:58362/Service1.svc";
        private readonly InvoiceService _invoiceService;
        private readonly PendingInvoiceService _pendingInvoiceService;

        public InvoiceController(
            InvoiceService invoiceService,
            PendingInvoiceService pendingInvoiceService)
        {
            _invoiceService = invoiceService;
            _pendingInvoiceService = pendingInvoiceService;
        }

        // Main view for fetching invoices
        [HttpGet]
        public IActionResult Index()
        {
            return View("FetchInvoices", new List<ServiceReference1.InvoiceData>());
        }

        // Fetch invoices by date range
        [HttpGet]
        public async Task<ActionResult> FetchInvoices(string startDate, string endDate)
        {
            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            {
                ViewBag.Error = "Please enter a valid Start Date and End Date.";
                return View(new List<ServiceReference1.InvoiceData>());
            }

            var invoices = await GetInvoicesFromWebService(startDate, endDate);
            Console.WriteLine("Parsed Invoices Count: " + invoices.Count);

            return View(invoices);
        }

        // Upload Excel file and compare with web service data
        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file, string startDate, string endDate)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Please upload a valid Excel file.";
                return View("FetchInvoices", new List<ServiceReference1.InvoiceData>());
            }

            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            {
                ViewBag.Error = "Please enter valid Start and End dates.";
                return View("FetchInvoices", new List<ServiceReference1.InvoiceData>());
            }

            try
            {
                Console.WriteLine("=== STARTING UPLOAD & FETCH PROCESS ===");

                // Step 1: Read Excel Data
                List<ServiceReference1.InvoiceData> excelInvoices = await ReadExcelFile(file);
                Console.WriteLine($"Excel Invoices Count: {excelInvoices.Count}");

                // Step 2: Fetch WCF Invoices Using User-Provided Dates
                var wcfInvoices = await GetInvoicesFromWebService(startDate, endDate);
                Console.WriteLine($"WCF Invoices Count: {wcfInvoices.Count}");

                // Step 3: Find Matching Invoices (Excel vs WCF)
                var matchedInvoices = wcfInvoices
                    .Where(w => excelInvoices.Any(e =>
                        e.ERPTID == w.ERPTID &&
                        e.ERPAmount == w.ERPAmount &&
                        e.CardPAN == w.CardPAN))
                    .ToList();

                // Step 4: Find New Pending Invoices (Not in Excel)
                var pendingInvoices = wcfInvoices
                    .Where(w => !excelInvoices.Any(e =>
                        e.ERPTID == w.ERPTID &&
                        e.ERPAmount == w.ERPAmount &&
                        e.CardPAN == w.CardPAN))
                    .ToList();

                Console.WriteLine($"New Pending Invoices Count: {pendingInvoices.Count}");

                // Save the pending invoices using the service
                _pendingInvoiceService.AddPendingInvoices(pendingInvoices);

                // Save both lists as Excel files to disk
                SaveExcelFileToDisk(matchedInvoices, "MatchedInvoices.xlsx", "MatchedInvoices");
                SaveExcelFileToDisk(pendingInvoices, "PendingInvoices.xlsx", "PendingInvoices");

                // Pass data to View
                ViewBag.Differences = pendingInvoices; // Show new pending invoices
                return View("FetchInvoices", matchedInvoices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ViewBag.Error = "Error processing file: " + ex.Message;
                return View("FetchInvoices", new List<ServiceReference1.InvoiceData>());
            }
        }

        // Compare uploaded Excel with pending payments
        [HttpPost]
        public async Task<IActionResult> CompareWithPendingPayments(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Please upload a valid Excel file.";
                return View("FetchInvoices", new List<ServiceReference1.InvoiceData>());
            }

            try
            {
                Console.WriteLine("=== STARTING PENDING PAYMENTS COMPARISON ===");

                // Step 1: Read Excel Data
                List<ServiceReference1.InvoiceData> excelInvoices = await ReadExcelFile(file);
                Console.WriteLine($"Excel Invoices Count: {excelInvoices.Count}");

                // Step 2: Match with pending payments and update status
                var matchedInvoices = _pendingInvoiceService.MatchWithExcelEntries(excelInvoices);

                if (matchedInvoices.Any())
                {
                    Console.WriteLine($"✅ Found {matchedInvoices.Count} invoices in Pending Payments. Matched...");
                }

                // Pass matched pending invoices to the view
                ViewBag.MatchedPendingInvoices = matchedInvoices.Select(p => p.ToInvoiceData()).ToList();
                return View("FetchInvoices", excelInvoices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ViewBag.Error = "Error processing file: " + ex.Message;
                return View("FetchInvoices", new List<ServiceReference1.InvoiceData>());
            }
        }

        // View pending payments
        [HttpGet]
        public IActionResult PendingPayments(string statusFilter = "Pending")
        {
            var pendingInvoices = _pendingInvoiceService.GetInvoicesByStatus(statusFilter);
            ViewBag.StatusFilter = statusFilter;
            return View(pendingInvoices);
        }

        // Delete a pending invoice
        [HttpPost]
        public IActionResult DeletePendingInvoice(string id)
        {
            _pendingInvoiceService.DeleteInvoice(id);
            return RedirectToAction("PendingPayments");
        }

        // Update invoice status
        [HttpPost]
        public IActionResult UpdateInvoiceStatus(string id, string status, string notes)
        {
            _pendingInvoiceService.UpdateInvoiceStatus(
                id,
                status,
                notes,
                User.Identity?.Name ?? "System");

            return RedirectToAction("PendingPayments");
        }

        // Download matched invoices Excel file
        [HttpGet]
        public IActionResult DownloadMatchedInvoices()
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previous files");
            string filePath = Path.Combine(folderPath, "MatchedInvoices.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("The matched invoices Excel file does not exist.");
            }

            return PhysicalFile(filePath,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "MatchedInvoices.xlsx");
        }

        // Download pending invoices as Excel file
        [HttpGet]
        public IActionResult DownloadPendingInvoices(string statusFilter = "Pending")
        {
            var pendingInvoices = _pendingInvoiceService
                .GetInvoicesByStatus(statusFilter)
                .Select(p => p.ToInvoiceData())
                .ToList();

            byte[] fileContent = GenerateExcelFile(pendingInvoices, "PendingInvoices");
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"PendingInvoices_{statusFilter}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // Show invoice dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            var allInvoices = _pendingInvoiceService.GetAllPendingInvoices();

            ViewBag.PendingCount = allInvoices.Count(i => i.Status == "Pending");
            ViewBag.MatchedCount = allInvoices.Count(i => i.Status == "Matched");
            ViewBag.ResolvedCount = allInvoices.Count(i => i.Status == "Resolved");
            ViewBag.DisputedCount = allInvoices.Count(i => i.Status == "Disputed");

            ViewBag.PendingAmount = allInvoices
                .Where(i => i.Status == "Pending")
                .Sum(i => i.ERPAmount);

            ViewBag.TodayCount = allInvoices
                .Count(i => i.AddedDate.Date == DateTime.Today);

            return View();
        }

        // Get invoice details for modal
        [HttpGet]
        public IActionResult GetInvoiceDetails(string id)
        {
            var invoice = _pendingInvoiceService.GetInvoiceById(id);
            if (invoice == null)
                return NotFound();

            return PartialView("_InvoiceDetails", invoice);
        }

        #region Helper Methods

        // Get invoices from web service
        private async Task<List<ServiceReference1.InvoiceData>> GetInvoicesFromWebService(string startDate, string endDate)
        {
            List<ServiceReference1.InvoiceData> invoices = new List<ServiceReference1.InvoiceData>();

            try
            {
                Console.WriteLine("Attempting to call the WCF service...");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("SOAPAction", "http://tempuri.org/IService1/GetInvoicesByDateRange");

                    string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                              xmlns:web=""http://tempuri.org/"">
                <soapenv:Header/>
                <soapenv:Body>
                    <web:GetInvoicesByDateRange>
                        <web:startDate>{startDate}</web:startDate>
                        <web:endDate>{endDate}</web:endDate>
                    </web:GetInvoicesByDateRange>
                </soapenv:Body>
            </soapenv:Envelope>";

                    var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

                    HttpResponseMessage response = await client.PostAsync(serviceUrl, content);
                    string result = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Response from WCF: " + result);

                    if (response.IsSuccessStatusCode)
                    {
                        invoices = ParseInvoicesFromXml(result);
                    }
                    else
                    {
                        Console.WriteLine($"Error calling WCF: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            return invoices;
        }

        // Parse invoices from XML response
        private List<ServiceReference1.InvoiceData> ParseInvoicesFromXml(string xmlResponse)
        {
            List<ServiceReference1.InvoiceData> invoices = new List<ServiceReference1.InvoiceData>();

            XDocument doc = XDocument.Parse(xmlResponse);

            XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace tempuriNs = "http://tempuri.org/";
            XNamespace dataContractNs = "http://schemas.datacontract.org/2004/07/WebService_POS_Match";

            var invoiceElements = doc.Descendants(soapNs + "Body")
                                     .Descendants(tempuriNs + "GetInvoicesByDateRangeResponse")
                                     .Descendants(tempuriNs + "GetInvoicesByDateRangeResult")
                                     .Descendants(dataContractNs + "InvoiceData");

            foreach (var item in invoiceElements)
            {
                string amountStr = item.Element(dataContractNs + "ERPAmount")?.Value;
                decimal parsedAmount = decimal.TryParse(amountStr, out var amount) ? amount / 100 : 0;

                invoices.Add(new ServiceReference1.InvoiceData
                {
                    InvoiceInfo = item.Element(dataContractNs + "InvoiceInfo")?.Value,
                    RepeatedReason = item.Element(dataContractNs + "RepeatedReason")?.Value,
                    InvoiceDocumentId = item.Element(dataContractNs + "InvoiceDocumentId")?.Value,
                    DateOnly = item.Element(dataContractNs + "DateOnly")?.Value,
                    ERPAmount = parsedAmount,
                    CardPAN = item.Element(dataContractNs + "CardPAN")?.Value,
                    ERPTID = item.Element(dataContractNs + "ERPTID")?.Value
                });
            }

            return invoices;
        }

        // Read Excel file uploaded by user
        private async Task<List<ServiceReference1.InvoiceData>> ReadExcelFile(IFormFile file)
        {
            List<ServiceReference1.InvoiceData> invoices = new List<ServiceReference1.InvoiceData>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        invoices.Add(new ServiceReference1.InvoiceData
                        {
                            RepeatedReason = worksheet.Cells[row, 1].Text,
                            DateOnly = worksheet.Cells[row, 4].Text,
                            ERPAmount = decimal.TryParse(worksheet.Cells[row, 7].Text, out var amount) ? Math.Abs(amount) : 0,
                            CardPAN = worksheet.Cells[row, 6].Text,
                            ERPTID = worksheet.Cells[row, 14].Text
                        });
                    }
                }
            }

            return invoices;
        }

        // Save differences to JSON file
        private void SaveDifferencesToFile(List<ServiceReference1.InvoiceData> newDifferences)
        {
            try
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previous files");
                string filePath = Path.Combine(folderPath, "pending_payments.json");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                List<ServiceReference1.InvoiceData> existingDifferences = new List<ServiceReference1.InvoiceData>();

                if (System.IO.File.Exists(filePath))
                {
                    string existingJson = System.IO.File.ReadAllText(filePath);
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        existingDifferences = JsonSerializer.Deserialize<List<ServiceReference1.InvoiceData>>(existingJson)
                                              ?? new List<ServiceReference1.InvoiceData>();
                    }
                }

                existingDifferences.AddRange(newDifferences);

                existingDifferences = existingDifferences
                    .GroupBy(i => new { i.ERPTID, i.ERPAmount, i.CardPAN, i.InvoiceDocumentId, i.InvoiceInfo, i.RepeatedReason })
                    .Select(g => g.First())
                    .ToList();

                string json = JsonSerializer.Serialize(existingDifferences, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, json);

                Console.WriteLine($"✅ JSON file updated with {existingDifferences.Count} unique entries.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving JSON file: {ex.Message}");
            }
        }

        // Generate Excel file from data
        private byte[] GenerateExcelFile<T>(List<T> data, string sheetName = "Sheet1")
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);
                var properties = typeof(T).GetProperties();

                // Write headers
                for (int col = 1; col <= properties.Length; col++)
                {
                    worksheet.Cells[1, col].Value = properties[col - 1].Name;
                }

                // Write data rows
                for (int row = 0; row < data.Count; row++)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        var value = properties[col].GetValue(data[row]);
                        worksheet.Cells[row + 2, col + 1].Value = value;
                    }
                }

                return package.GetAsByteArray();
            }
        }

        // Save Excel file to disk
        private void SaveExcelFileToDisk<T>(List<T> data, string fileName, string sheetName = "Sheet1")
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previous files");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);

            byte[] fileContent = GenerateExcelFile(data, sheetName);
            System.IO.File.WriteAllBytes(filePath, fileContent);
        }

        #endregion
    }
}