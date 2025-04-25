using System;
using System.Text.Json.Serialization;

namespace WebApplication3.Models
{
    public class PendingInvoiceData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("invoiceInfo")]
        public string InvoiceInfo { get; set; }

        [JsonPropertyName("repeatedReason")]
        public string RepeatedReason { get; set; }

        [JsonPropertyName("invoiceDocumentId")]
        public string InvoiceDocumentId { get; set; }

        [JsonPropertyName("dateOnly")]
        public string DateOnly { get; set; }

        [JsonPropertyName("erpAmount")]
        public decimal ERPAmount { get; set; }

        [JsonPropertyName("cardPAN")]
        public string CardPAN { get; set; }

        [JsonPropertyName("erptid")]
        public string ERPTID { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending";

        [JsonPropertyName("addedDate")]
        public DateTime AddedDate { get; set; } = DateTime.Now;

        [JsonPropertyName("lastModifiedDate")]
        public DateTime? LastModifiedDate { get; set; }

        [JsonPropertyName("lastModifiedBy")]
        public string LastModifiedBy { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        // Create from ServiceReference1.InvoiceData
        public static PendingInvoiceData FromInvoiceData(ServiceReference1.InvoiceData source)
        {
            return new PendingInvoiceData
            {
                InvoiceInfo = source.InvoiceInfo,
                RepeatedReason = source.RepeatedReason,
                InvoiceDocumentId = source.InvoiceDocumentId,
                DateOnly = source.DateOnly,
                ERPAmount = source.ERPAmount,
                CardPAN = source.CardPAN,
                ERPTID = source.ERPTID
            };
        }

        // Convert to ServiceReference1.InvoiceData
        public ServiceReference1.InvoiceData ToInvoiceData()
        {
            return new ServiceReference1.InvoiceData
            {
                InvoiceInfo = this.InvoiceInfo,
                RepeatedReason = this.RepeatedReason,
                InvoiceDocumentId = this.InvoiceDocumentId,
                DateOnly = this.DateOnly,
                ERPAmount = this.ERPAmount,
                CardPAN = this.CardPAN,
                ERPTID = this.ERPTID
            };
        }
    }
}