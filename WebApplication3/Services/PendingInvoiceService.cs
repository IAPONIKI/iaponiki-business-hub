using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public class PendingInvoiceService
    {
        private readonly string _folderPath;
        private readonly string _filePath;
        private static readonly object _fileLock = new object();
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public PendingInvoiceService(string webRootPath)
        {
            _folderPath = Path.Combine(webRootPath, "previous files");
            _filePath = Path.Combine(_folderPath, "pending_payments.json");

            // Ensure directory exists
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }

            // Ensure file exists
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        // Get all pending invoices
        public List<PendingInvoiceData> GetAllPendingInvoices()
        {
            lock (_fileLock)
            {
                try
                {
                    string json = File.ReadAllText(_filePath);
                    return !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<List<PendingInvoiceData>>(json)
                        : new List<PendingInvoiceData>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading pending invoices: {ex.Message}");
                    return new List<PendingInvoiceData>();
                }
            }
        }

        // Get invoices by status
        public List<PendingInvoiceData> GetInvoicesByStatus(string status)
        {
            var allInvoices = GetAllPendingInvoices();
            return status.ToLower() == "all"
                ? allInvoices
                : allInvoices.Where(i => i.Status == status).ToList();
        }

        // Get a specific invoice by ID
        public PendingInvoiceData GetInvoiceById(string id)
        {
            return GetAllPendingInvoices().FirstOrDefault(i => i.Id == id);
        }

        // Add new pending invoices
        public bool AddPendingInvoices(List<ServiceReference1.InvoiceData> newInvoices)
        {
            if (newInvoices == null || !newInvoices.Any())
                return false;

            lock (_fileLock)
            {
                try
                {
                    // Get existing invoices
                    var existingInvoices = GetAllPendingInvoices();

                    // Convert and add new invoices
                    var newPendingInvoices = newInvoices
                        .Select(PendingInvoiceData.FromInvoiceData)
                        .ToList();

                    // Merge with existing, avoiding duplicates
                    foreach (var newInvoice in newPendingInvoices)
                    {
                        // Check if this invoice already exists
                        bool isDuplicate = existingInvoices.Any(e =>
                            e.ERPTID == newInvoice.ERPTID &&
                            e.CardPAN == newInvoice.CardPAN &&
                            e.ERPAmount == newInvoice.ERPAmount);

                        if (!isDuplicate)
                        {
                            existingInvoices.Add(newInvoice);
                        }
                    }

                    // Save back to file
                    string json = JsonSerializer.Serialize(existingInvoices, _jsonOptions);
                    File.WriteAllText(_filePath, json);

                    // Create backup
                    CreateBackup();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding pending invoices: {ex.Message}");
                    return false;
                }
            }
        }

        // Update invoice status
        public bool UpdateInvoiceStatus(string id, string status, string notes, string username)
        {
            lock (_fileLock)
            {
                try
                {
                    var invoices = GetAllPendingInvoices();
                    var invoice = invoices.FirstOrDefault(i => i.Id == id);

                    if (invoice == null)
                        return false;

                    invoice.Status = status;
                    invoice.Notes = notes;
                    invoice.LastModifiedBy = username;
                    invoice.LastModifiedDate = DateTime.Now;

                    string json = JsonSerializer.Serialize(invoices, _jsonOptions);
                    File.WriteAllText(_filePath, json);

                    // Create backup
                    CreateBackup();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating invoice status: {ex.Message}");
                    return false;
                }
            }
        }

        // Delete an invoice
        public bool DeleteInvoice(string id)
        {
            lock (_fileLock)
            {
                try
                {
                    var invoices = GetAllPendingInvoices();
                    var invoice = invoices.FirstOrDefault(i => i.Id == id);

                    if (invoice == null)
                        return false;

                    invoices.Remove(invoice);

                    string json = JsonSerializer.Serialize(invoices, _jsonOptions);
                    File.WriteAllText(_filePath, json);

                    // Create backup
                    CreateBackup();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting invoice: {ex.Message}");
                    return false;
                }
            }
        }

        // Match with Excel entries
        public List<PendingInvoiceData> MatchWithExcelEntries(List<ServiceReference1.InvoiceData> excelInvoices)
        {
            lock (_fileLock)
            {
                try
                {
                    var pendingInvoices = GetAllPendingInvoices();
                    var matchedInvoices = new List<PendingInvoiceData>();

                    foreach (var pending in pendingInvoices)
                    {
                        bool isMatched = excelInvoices.Any(excel =>
                            excel.ERPTID == pending.ERPTID &&
                            excel.ERPAmount == pending.ERPAmount &&
                            excel.CardPAN == pending.CardPAN);

                        if (isMatched && pending.Status == "Pending")
                        {
                            pending.Status = "Matched";
                            pending.LastModifiedDate = DateTime.Now;
                            pending.Notes = "Matched via Excel upload";
                            matchedInvoices.Add(pending);
                        }
                    }

                    // If any matches were found, update the file
                    if (matchedInvoices.Any())
                    {
                        string json = JsonSerializer.Serialize(pendingInvoices, _jsonOptions);
                        File.WriteAllText(_filePath, json);

                        // Create backup
                        CreateBackup();
                    }

                    return matchedInvoices;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error matching with Excel: {ex.Message}");
                    return new List<PendingInvoiceData>();
                }
            }
        }

        // Create a backup of the pending payments file
        private void CreateBackup()
        {
            try
            {
                string backupFolder = Path.Combine(_folderPath, "backups");

                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = Path.Combine(backupFolder, $"pending_payments_{timestamp}.json");

                File.Copy(_filePath, backupFile);

                // Keep only the latest 10 backups
                var backupFiles = new DirectoryInfo(backupFolder)
                    .GetFiles("pending_payments_*.json")
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(10);

                foreach (var file in backupFiles)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating backup: {ex.Message}");
            }
        }
    }
}