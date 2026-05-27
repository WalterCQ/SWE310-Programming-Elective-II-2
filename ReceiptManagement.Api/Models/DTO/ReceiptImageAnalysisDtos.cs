using ReceiptManagement.Api.Configuration;

namespace ReceiptManagement.Api.Models.DTO;

public class ReceiptImageAnalysisItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class ReceiptImageAnalysisDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public string ReceiptDate { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = ReceiptManagementConstants.CurrencyCode;
    public string PaymentMethod { get; set; } = "Unknown";
    public decimal Confidence { get; set; }
    public string RawTextSummary { get; set; } = string.Empty;
    public List<ReceiptImageAnalysisItemDto> Items { get; set; } = [];
}
