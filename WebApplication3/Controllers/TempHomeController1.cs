using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using static WebApplication3.Models.HRModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using OfficeOpenXml;
using WebApplication3.Services;
using ServiceReference1;
using System.ServiceModel;
using System.Xml.Linq;

namespace WebApplication3.Controllers
{
    public class tempHomeController1 : Controller
    {

        //[Authorize]
        public IActionResult Index()
        {
            var username = User.Identity.Name;  // Returns DOMAIN\username
            ViewBag.Username = username;
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


        // MARKETING SECTION

        [HttpGet]
        public IActionResult IndexCarousel()
        {
            return View(new CarouselModel());
        }

        // Step 1: User submits ewhich carousels thy need
        [HttpPost]
        public IActionResult ChooseCarousels(CarouselModel model)
        {
            return View("ChooseNumberOfImages", model);
        }

        [HttpPost]
        public IActionResult EnterImageUrls(CarouselModel model)
        {
            // Load the old model *once*
            var oldModel = LoadOldModel();
            if (oldModel != null)
            {
                model.OldSeminarioImages = oldModel.SeminarioImages;
                model.OldEnergeiesImages = oldModel.EnergeiesImages;
                model.OldProsforesImages = oldModel.ProsforesImages;
            }

            // Then allocate new images
            if (model.NeedsSeminario)
                model.SeminarioImages = new List<ImageInfo>(model.SeminarioImageCount);

            if (model.NeedsEnergeiesTouMina)
                model.EnergeiesImages = new List<ImageInfo>(model.EnergeiesTouMinaImageCount);

            if (model.NeedsProsforesExoplismou)
                model.ProsforesImages = new List<ImageInfo>(model.ProsforesExoplismouImageCount);

            return View(model);
        }


        // Step 2: User inputs the URLs, we generate the final HTML
        [HttpPost]
        public IActionResult GenerateHtml(CarouselModel model)
        {

            // Handle keep default logic
            HandleKeepDefault(model.SeminarioImages, model.OldSeminarioImages);
            HandleKeepDefault(model.EnergeiesImages, model.OldEnergeiesImages);
            HandleKeepDefault(model.ProsforesImages, model.OldProsforesImages);

            string finalHtml = GenerateFinalHtml(model);

            if (model.NeedsSeminario)
            {
                // For each image the user typed
                for (int i = 0; i < model.SeminarioImages.Count; i++)
                {
                    var newImg = model.SeminarioImages[i];

                    if (newImg.KeepDefault && !string.IsNullOrEmpty(newImg.OldImageSelection))
                    {
                        // Find that old image in model.OldEnergeiesImages
                        var oldImg = model.OldSeminarioImages.FirstOrDefault(o => o.ImageId == newImg.OldImageSelection);
                        if (oldImg != null)
                        {
                            // Override the current newImg with the old one
                            newImg.Url = oldImg.Url;
                            newImg.ImageId = oldImg.ImageId;
                        }
                    }
                }
            }

            if (model.NeedsEnergeiesTouMina)
            {
                // For each image the user typed
                for (int i = 0; i < model.EnergeiesImages.Count; i++)
                {
                    var newImg = model.EnergeiesImages[i];

                    if (newImg.KeepDefault && !string.IsNullOrEmpty(newImg.OldImageSelection))
                    {
                        // Find that old image in model.OldEnergeiesImages
                        var oldImg = model.OldEnergeiesImages.FirstOrDefault(o => o.ImageId == newImg.OldImageSelection);
                        if (oldImg != null)
                        {
                            // Override the current newImg with the old one
                            newImg.Url = oldImg.Url;
                            newImg.ImageId = oldImg.ImageId;
                        }
                    }
                }
            }

            if (model.NeedsProsforesExoplismou)
            {
                // For each image the user typed
                for (int i = 0; i < model.ProsforesImages.Count; i++)
                {
                    var newImg = model.ProsforesImages[i];

                    if (newImg.KeepDefault && !string.IsNullOrEmpty(newImg.OldImageSelection))
                    {
                        // Find that old image in model.OldEnergeiesImages
                        var oldImg = model.OldProsforesImages.FirstOrDefault(o => o.ImageId == newImg.OldImageSelection);
                        if (oldImg != null)
                        {
                            // Override the current newImg with the old one
                            newImg.Url = oldImg.Url;
                            newImg.ImageId = oldImg.ImageId;
                        }
                    }
                }
            }
            SaveOldModel(model, "lastModel.json");

            return View("ShowGeneratedHtml", finalHtml);
        }

        private string GenerateFinalHtml(CarouselModel model)
        {
            //Read the base template file
            string templatePath = Path.Combine("wwwroot", "templates", "baseTemplate.html");
            string baseHtml = System.IO.File.ReadAllText(templatePath);

            //If user wants to include the carousel, build the items:
            if (model.NeedsSeminario && model.SeminarioImages != null && model.SeminarioImages.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < model.SeminarioImages.Count; i++)
                {
                    var image = model.SeminarioImages[i];

                    // "active" class for the first item
                    string activeClass = i == 0 ? " active" : "";


                    sb.AppendLine($@"
                        <div class='carousel-item{activeClass}'>
                            <section class='section'>
                                <div class='grid-container-small'>
                                    <img id='{image.ImageId}' src='{image.Url}' />
                                </div>
                            </section>
                        </div>");
                    if (image.IncludePostMessage)
                    {
                        sb.AppendLine($@"
                    <script>
                        (function() {{
                            var element = document.getElementById('{image.ImageId}');
                            if(element) {{
                                element.addEventListener('click', function() {{
                                    // postMessage with '{image.ImageId}'
                                    window.top.postMessage({{
                                        openArticleList: {{
                                            uniSearch: {{
                                                query: '{image.ImageId}'
                                            }}
                                        }}
                                    }}, '*');
                                }});
                            }}
                        }})();
                    </script>");
                    }
                    sb.AppendLine($@"
                    <script>
                        (function() {{
                            var element = document.getElementById('{image.ImageId}');
                            if(element) {{
                               // GA event
                                    if (typeof gtag === 'function') {{
                                        gtag('event', 'click', {{
                                            'event_category': 'landing page',
                                            'event_label': 'Click on {image.ImageId} banner',
                                            'customer_id': customerId
                                        }});
                                    }}
                                }});
                            }}
                        }})();
                    </script>");
                }
                //Replace the marker with the newly generated blocks
                baseHtml = baseHtml.Replace("<!--CAROUSEL_SEMINARIO_ITEMS-->", sb.ToString());

            }
            else
            {
                // If user doesn't need it, remove the entire block
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_SEMINARIO-->", "<!--END_SEMINARIO-->");
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_SEMINARIO_TITLE-->", "<!--END_SEMINARIO_TITLE-->");

            }


            if (model.NeedsEnergeiesTouMina && model.EnergeiesImages != null && model.EnergeiesImages.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < model.EnergeiesImages.Count; i++)
                {
                    var image = model.EnergeiesImages[i];
                    // "active" class for the first item
                    string activeClass = i == 0 ? " active" : "";

                    sb.AppendLine($@"
                        <div class='carousel-item{activeClass}'>
                            <section class='section'>
                                <div class='grid-container-small'>
                                    <img id='{image.ImageId}' src='{image.Url}' />
                                </div>
                            </section>
                        </div>");
                    if (image.IncludePostMessage)
                    {
                        sb.AppendLine($@"
                    <script>
                        (function() {{
                            var element = document.getElementById('{image.ImageId}');
                            if(element) {{
                                element.addEventListener('click', function() {{
                                    // postMessage with '{image.ImageId}'
                                    window.top.postMessage({{
                                        openArticleList: {{
                                            uniSearch: {{
                                                query: '{image.ImageId}'
                                            }}
                                        }}
                                    }}, '*');
                                }});
                            }}
                        }})();
                    </script>");
                    }
                    sb.AppendLine($@"
                    <script>
                        (function() {{
                            var element = document.getElementById('{image.ImageId}');
                            if(element) {{
                               // GA event
                                    if (typeof gtag === 'function') {{
                                        gtag('event', 'click', {{
                                            'event_category': 'landing page',
                                            'event_label': 'Click on {image.ImageId} banner',
                                            'customer_id': customerId
                                        }});
                                    }}
                                }});
                            }}
                        }})();
                    </script>");
                }
                //Replace the marker with the newly generated blocks
                baseHtml = baseHtml.Replace("<!--CAROUSEL_ENERGEIES_ITEMS-->", sb.ToString());

            }
            else
            {
                // If user doesn't need it, remove the entire block
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_ENERGEIES-->", "<!--END_ENERGEIES-->");
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_ENERGEIES_TITLE-->", "<!--END_ENERGEIES_TITLE-->");

            }

            if (model.NeedsProsforesExoplismou && model.ProsforesImages != null && model.ProsforesImages.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < model.ProsforesImages.Count; i++)
                {
                    var image = model.ProsforesImages[i];
                    // "active" class for the first item
                    string activeClass = i == 0 ? " active" : "";

                    sb.AppendLine($@"
                        <div class='carousel-item{activeClass}'>
                            <section class='section'>
                                <div class='grid-container-small'>
                                    <img id='{image.ImageId}' src='{image.Url}' />
                                </div>
                            </section>
                        </div>");
                    if (image.IncludePostMessage)
                    {
                        sb.AppendLine($@"
                    <script>
                        (function() {{
                            var element = document.getElementById('{image.ImageId}');
                            if(element) {{
                                element.addEventListener('click', function() {{
                                    // postMessage with '{image.ImageId}'
                                    window.top.postMessage({{
                                        openArticleList: {{
                                            uniSearch: {{
                                                query: '{image.ImageId}'
                                            }}
                                        }}
                                    }}, '*');
                                }});
                            }}
                        }})();
                    </script>");
                    }
                    sb.AppendLine($@"
                    <script>
                        (function() {{
                            var element = document.getElementById('{image.ImageId}');
                            if(element) {{
                               // GA event
                                    if (typeof gtag === 'function') {{
                                        gtag('event', 'click', {{
                                            'event_category': 'landing page',
                                            'event_label': 'Click on {image.ImageId} banner',
                                            'customer_id': customerId
                                        }});
                                    }}
                                }});
                            }}
                        }})();
                    </script>");
                }
                //Replace the marker with the newly generated blocks
                baseHtml = baseHtml.Replace("<!--CAROUSEL_PROSFORES_ITEMS-->", sb.ToString());
            }
            else
            {
                //If user doesn't need it, remove the entire block
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_PROSFORES-->", "<!--END_PROSFORES-->");
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_PROSFORES_TITLE-->", "<!--END_PROSFORES_TITLE-->");

            }

            // Return final HTML
            return baseHtml;
        }

        private string RemoveSection(string html, string startMarker, string endMarker)
        {
            int startIndex = html.IndexOf(startMarker);
            if (startIndex == -1) return html; // If marker not found, return unchanged

            int endIndex = html.IndexOf(endMarker, startIndex);
            if (endIndex == -1) return html; // If end marker not found, return unchanged

            return html.Remove(startIndex, (endIndex + endMarker.Length) - startIndex);
        }


        [HttpPost]
        public FileResult DownloadHtml(string finalHtml)
        {
            //Convert the HTML string to bytes (UTF8)
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(finalHtml);

            //Return as a file with a .html download
            return File(fileBytes, "text/html", "landingpage.html");
        }

        private void SaveOldModel(CarouselModel model, string fileName)
        {
            //Convert the model to JSON
            string json = JsonSerializer.Serialize(model);

            //Decide on a path to store
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previous files");
            Directory.CreateDirectory(folderPath);

            //Final path of the JSON file
            string fullPath = Path.Combine(folderPath, fileName);

            //Write the JSON to disk
            System.IO.File.WriteAllText(fullPath, json);
        }

        private CarouselModel LoadOldModel()
        {
            //Read a file named "lastModel.json"
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previous files");
            string filePath = Path.Combine(folderPath, "lastModel.json");

            //Check if file exists
            if (!System.IO.File.Exists(filePath))
                return null;

            //Read the JSON
            string json = System.IO.File.ReadAllText(filePath);

            //Deserialize into a CarouselModel
            CarouselModel oldModel = JsonSerializer.Deserialize<CarouselModel>(json);

            return oldModel;
        }

        private void HandleKeepDefault(List<ImageInfo> newImages, List<ImageInfo> oldImages)
        {
            if (newImages == null || oldImages == null) return;

            foreach (var newImg in newImages)
            {
                if (newImg.KeepDefault && !string.IsNullOrEmpty(newImg.OldImageSelection))
                {
                    var oldImg = oldImages.FirstOrDefault(o => o.ImageId == newImg.OldImageSelection);
                    if (oldImg != null)
                    {
                        newImg.Url = oldImg.Url;
                        newImg.ImageId = oldImg.ImageId;
                    }
                }
            }
        }

        // HR SECTION
        private static List<HRModel.JobPosting> _jobPostings = new List<HRModel.JobPosting>();
        private readonly string savedJobsPath = Path.Combine("wwwroot", "previous files", "SavedJobPostings.json");

        public tempHomeController1(InvoiceService invoiceService)
        {
            _jobPostings = LoadJobPostingsFromFile();
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

            var model = new HRModel.JobPostingsModel
            {
                CurrentPostings = currentPostings,
                PreviousPostings = previousPostings
            };

            return View(model);
        }

        // Add Job
        public IActionResult AddJob()
        {
            return View(new HRModel.JobPosting { PostingDate = DateTime.Now, ExpiryDate = DateTime.Now });
        }

        [HttpPost]
        public IActionResult AddJob(HRModel.JobPosting job)
        {
            job.Id = _jobPostings.Count == 0 ? 1 : _jobPostings.Max(j => j.Id) + 1;
            job.IsActive = job.IsActive;
            job.OrderId = job.OrderId;
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
        public IActionResult EditJob(HRModel.JobPosting updatedJob)
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
            var model = new HRModel.JobPostingsModel
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


        //this method will be used to save the job postings to a json file !!!!!!!!!!!!!!!!!!!
        private void SaveOldModelHR(List<JobPosting> jobPosting, string fileName)
        {
            //Convert the model to JSON
            string json = JsonSerializer.Serialize(jobPosting);

            //Decide on a path to store
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previous files");
            Directory.CreateDirectory(folderPath);

            //Final path of the JSON file
            string fullPath = Path.Combine(folderPath, fileName);

            //Write the JSON to disk
            System.IO.File.WriteAllText(fullPath, json);
        }

        // Utility Methods
        private void SaveJobPostingsToFile()
        {
            var json = JsonSerializer.Serialize(_jobPostings, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(savedJobsPath, json);
        }

        private List<HRModel.JobPosting> LoadJobPostingsFromFile()
        {
            if (System.IO.File.Exists(savedJobsPath))
            {
                var json = System.IO.File.ReadAllText(savedJobsPath);
                return JsonSerializer.Deserialize<List<HRModel.JobPosting>>(json) ?? new List<HRModel.JobPosting>();
            }
            return new List<HRModel.JobPosting>();
        }

        private string GenerateBaseHRHtml(List<HRModel.JobPosting> jobPostings, string sortOrder)
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

        private readonly string serviceUrl = "http://localhost:58362/Service1.svc";

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


        private List<ServiceReference1.InvoiceData> ParseInvoicesFromXml(string xmlResponse)
        {
            List<ServiceReference1.InvoiceData> invoices = new List<ServiceReference1.InvoiceData>();

            //Load XML response
            XDocument doc = XDocument.Parse(xmlResponse);

            //Define namespaces
            XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace tempuriNs = "http://tempuri.org/";
            XNamespace dataContractNs = "http://schemas.datacontract.org/2004/07/WebService_POS_Match";

            //Find all <a:InvoiceData> elements inside the response
            var invoiceElements = doc.Descendants(soapNs + "Body")
                                     .Descendants(tempuriNs + "GetInvoicesByDateRangeResponse")
                                     .Descendants(tempuriNs + "GetInvoicesByDateRangeResult")
                                     .Descendants(dataContractNs + "InvoiceData");

            //Extract invoice details
            foreach (var item in invoiceElements)
            {
                string amountStr = item.Element(dataContractNs + "ERPAmount")?.Value; // Read amount as string
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

                //Step 1: Read Excel Data
                List<ServiceReference1.InvoiceData> excelInvoices = await ReadExcelFile(file);
                Console.WriteLine($"Excel Invoices Count: {excelInvoices.Count}");

                //Step 2: Fetch WCF Invoices Using User-Provided Dates
                var wcfInvoices = await GetInvoicesFromWebService(startDate, endDate);
                Console.WriteLine($"WCF Invoices Count: {wcfInvoices.Count}");

                //Step 3: Compare Excel and WCF Invoices
                var matchingInvoices = wcfInvoices
                    .Where(w => excelInvoices.Any(e => e.ERPTID == w.ERPTID && e.ERPAmount == w.ERPAmount && e.CardPAN == w.CardPAN))
                    .ToList();

                var newPendingInvoices = wcfInvoices
                    .Where(w => !excelInvoices.Any(e => e.ERPTID == w.ERPTID && e.ERPAmount == w.ERPAmount && e.CardPAN == w.CardPAN))
                    .ToList();

                Console.WriteLine($"Matching Invoices Count: {matchingInvoices.Count}");
                Console.WriteLine($"New Pending Invoices Count: {newPendingInvoices.Count}");

                //Step 4: Store Pending Invoices in JSON File
                SaveDifferencesToFile(newPendingInvoices);

                ViewBag.Differences = newPendingInvoices; // Show new pending invoices
                return View("FetchInvoices", matchingInvoices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ViewBag.Error = "Error processing file: " + ex.Message;
                return View("FetchInvoices", new List<ServiceReference1.InvoiceData>());
            }
        }



        private async Task<List<ServiceReference1.InvoiceData>> ReadExcelFile(IFormFile file)
        {
            List<ServiceReference1.InvoiceData> invoices = new List<ServiceReference1.InvoiceData>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream); //Convert IFormFile to Stream
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; //Required for EPPlus
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Start from row 2 (Skip Headers)
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

        private void SaveDifferencesToFile(List<ServiceReference1.InvoiceData> newDifferences)
        {
            try
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previous files");
                string filePath = Path.Combine(folderPath, "pending_payments.json");

                //Ensure directory exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                List<ServiceReference1.InvoiceData> existingDifferences = new List<ServiceReference1.InvoiceData>();

                //Load existing pending payments
                if (System.IO.File.Exists(filePath))
                {
                    string existingJson = System.IO.File.ReadAllText(filePath);
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        existingDifferences = JsonSerializer.Deserialize<List<ServiceReference1.InvoiceData>>(existingJson)
                                              ?? new List<ServiceReference1.InvoiceData>();
                    }
                }

                //Combine new and existing invoices
                existingDifferences.AddRange(newDifferences);

                //Remove exact duplicates
                existingDifferences = existingDifferences
                    .GroupBy(i => new { i.ERPTID, i.ERPAmount, i.CardPAN, i.InvoiceDocumentId, i.InvoiceInfo, i.RepeatedReason })
                    .Select(g => g.First()) // Keep only one of each duplicate
                    .ToList();

                //Save back the cleaned pending payments
                string json = JsonSerializer.Serialize(existingDifferences, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, json);

                Console.WriteLine($"✅ JSON file updated at: {filePath} with {existingDifferences.Count} unique entries.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving JSON file: {ex.Message}");
            }
        }
    }
}