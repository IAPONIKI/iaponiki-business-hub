using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin,Marketing")]
    public class CarouselController : Controller
    {
        private const string OldModelPath = "wwwroot/previous files/lastModel.json";
        private const string TemplateHtmlPath = "wwwroot/templates/baseTemplate.html";

        // Main landing page manager view - simplified version
        [HttpGet]
        public IActionResult ImprovedManager()
        {
            var model = LoadOldModel();
            if (model == null)
            {
                model = new CarouselModel
                {
                    NeedsSeminario = false,
                    NeedsEnergeiesTouMina = false,
                    NeedsProsforesExoplismou = false,
                    SeminarioImages = new List<ImageInfo>(),
                    EnergeiesImages = new List<ImageInfo>(),
                    ProsforesImages = new List<ImageInfo>()
                };
            }

            return View(model);
        }

        // Process form submission from improved manager
        [HttpPost]
        public IActionResult ProcessImproved(CarouselModel model)
        {
            // Process model
            ProcessModelForImproved(model);

            // Save model
            SaveOldModel(model, "lastModel.json");

            // Generate HTML
            string html = GenerateFinalHtml(model);

            // Return to the ShowGenerated view as before
            return View("ShowGeneratedHtml", html);
        }

        // Download HTML as a file
        [HttpPost]
        public FileResult DownloadHtml(string finalHtml)
        {
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(finalHtml);
            return File(fileBytes, "text/html", $"landingpage_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        }

        // Helper method for the improved manager
        private void ProcessModelForImproved(CarouselModel model)
        {
            // Handle active sections
            if (!model.NeedsSeminario)
                model.SeminarioImages = new List<ImageInfo>();

            if (!model.NeedsEnergeiesTouMina)
                model.EnergeiesImages = new List<ImageInfo>();

            if (!model.NeedsProsforesExoplismou)
                model.ProsforesImages = new List<ImageInfo>();

            // Remove null items
            model.SeminarioImages = model.SeminarioImages?.Where(i => i != null && !string.IsNullOrEmpty(i.Url))?.ToList() ?? new List<ImageInfo>();
            model.EnergeiesImages = model.EnergeiesImages?.Where(i => i != null && !string.IsNullOrEmpty(i.Url))?.ToList() ?? new List<ImageInfo>();
            model.ProsforesImages = model.ProsforesImages?.Where(i => i != null && !string.IsNullOrEmpty(i.Url))?.ToList() ?? new List<ImageInfo>();

            // Set counts
            model.SeminarioImageCount = model.SeminarioImages.Count;
            model.EnergeiesTouMinaImageCount = model.EnergeiesImages.Count;
            model.ProsforesExoplismouImageCount = model.ProsforesImages.Count;

            // Generate IDs for images that don't have one
            foreach (var image in model.SeminarioImages.Where(i => string.IsNullOrEmpty(i.ImageId)))
            {
                image.ImageId = GenerateImageId(image.Url);
            }

            foreach (var image in model.EnergeiesImages.Where(i => string.IsNullOrEmpty(i.ImageId)))
            {
                image.ImageId = GenerateImageId(image.Url);
            }

            foreach (var image in model.ProsforesImages.Where(i => string.IsNullOrEmpty(i.ImageId)))
            {
                image.ImageId = GenerateImageId(image.Url);
            }
        }

        // Generate an ID from a URL
        private string GenerateImageId(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "image" + DateTime.Now.Ticks;

            try
            {
                var uri = new Uri(url);
                string fileName = Path.GetFileNameWithoutExtension(uri.AbsolutePath);

                // Clean up the filename
                fileName = fileName.Replace("_", "").Replace("-", "");

                // If filename is empty or numeric, add a prefix
                if (string.IsNullOrEmpty(fileName) || int.TryParse(fileName, out _))
                {
                    fileName = "image" + DateTime.Now.Ticks.ToString().Substring(10);
                }

                return fileName.ToLower();
            }
            catch
            {
                return "image" + DateTime.Now.Ticks.ToString().Substring(10);
            }
        }

        // Generate HTML - using your existing method
        private string GenerateFinalHtml(CarouselModel model)
        {
            // Read the base template file
            string baseHtml = System.IO.File.ReadAllText(TemplateHtmlPath);

            // Handle Seminario carousel
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

                                    // GA event
                                    if (typeof gtag === 'function') {{
                                        gtag('event', 'click', {{
                                            'event_category': 'landing page',
                                            'event_label': 'Click on {image.ImageId} banner',
                                            'customer_id': '{model.CustomerId}'
                                        }});
                                    }}
                                }});
                            }}
                        }})();
                    </script>");
                    }
                }
                // Replace the marker with the newly generated blocks
                baseHtml = baseHtml.Replace("<!--CAROUSEL_SEMINARIO_ITEMS-->", sb.ToString());
            }
            else
            {
                // If user doesn't need it, remove the entire block
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_SEMINARIO-->", "<!--END_SEMINARIO-->");
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_SEMINARIO_TITLE-->", "<!--END_SEMINARIO_TITLE-->");
            }

            // Handle Energeies carousel
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

                                    // GA event
                                    if (typeof gtag === 'function') {{
                                        gtag('event', 'click', {{
                                            'event_category': 'landing page',
                                            'event_label': 'Click on {image.ImageId} banner',
                                            'customer_id': '{model.CustomerId}'
                                        }});
                                    }}
                                }});
                            }}
                        }})();
                    </script>");
                    }
                }
                // Replace the marker with the newly generated blocks
                baseHtml = baseHtml.Replace("<!--CAROUSEL_ENERGEIES_ITEMS-->", sb.ToString());
            }
            else
            {
                // If user doesn't need it, remove the entire block
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_ENERGEIES-->", "<!--END_ENERGEIES-->");
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_ENERGEIES_TITLE-->", "<!--END_ENERGEIES_TITLE-->");
            }

            // Handle Prosfores carousel
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

                                    // GA event
                                    if (typeof gtag === 'function') {{
                                        gtag('event', 'click', {{
                                            'event_category': 'landing page',
                                            'event_label': 'Click on {image.ImageId} banner',
                                            'customer_id': '{model.CustomerId}'
                                        }});
                                    }}
                                }});
                            }}
                        }})();
                    </script>");
                    }
                }
                // Replace the marker with the newly generated blocks
                baseHtml = baseHtml.Replace("<!--CAROUSEL_PROSFORES_ITEMS-->", sb.ToString());
            }
            else
            {
                // If user doesn't need it, remove the entire block
                baseHtml = RemoveSection(baseHtml, "<!--BEGIN_PROSFORES-->", "<!--END_PROSFORESS-->");
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

        private void SaveOldModel(CarouselModel model, string fileName)
        {
            try
            {
                //Convert the model to JSON
                string json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });

                //Decide on a path to store
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previous files");
                Directory.CreateDirectory(folderPath);

                //Final path of the JSON file
                string fullPath = Path.Combine(folderPath, fileName);

                //Write the JSON to disk
                System.IO.File.WriteAllText(fullPath, json);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error saving model: {ex.Message}");
            }
        }

        private CarouselModel LoadOldModel()
        {
            try
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
                CarouselModel oldModel = JsonSerializer.Deserialize<CarouselModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return oldModel;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading model: {ex.Message}");
                return null;
            }
        }


        // Original methods for backward compatibility
        [HttpGet]
        public IActionResult Index()
        {
            // Redirect to improved manager
            return RedirectToAction("ImprovedManager");
        }

        [HttpPost]
        public IActionResult ChooseCarousels(CarouselModel model)
        {
            return RedirectToAction("ImprovedManager", model);
        }

        [HttpPost]
        public IActionResult EnterImageUrls(CarouselModel model)
        {
            return RedirectToAction("ImprovedManager", model);
        }

        [HttpPost]
        public IActionResult GenerateHtml(CarouselModel model)
        {
            return RedirectToAction("ProcessImproved", model);
        }
    }
}