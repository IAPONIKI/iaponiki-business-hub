using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public class InvoiceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _wcfServiceUrl = "http://localhost:58362/Service1.svc/GetInvoicesByDateRange";

        public InvoiceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<InvoiceData>> GetInvoicesByDateRangeAsync(string startDate, string endDate)
        {
            var response = await _httpClient.GetAsync($"{_wcfServiceUrl}?startDate={startDate}&endDate={endDate}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve invoices: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<InvoiceData>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
