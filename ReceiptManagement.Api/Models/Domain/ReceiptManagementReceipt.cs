using System.ComponentModel.DataAnnotations;
using ReceiptManagement.Api.Configuration;

namespace ReceiptManagement.Api.Models.Domain;

public class ReceiptManagementReceipt
{
    public int ReceiptId { get; set; }

    [Required]
    [StringLength(40)]
    public string ReceiptNumber { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public int? VendorId { get; set; }

    [Required]
    [StringLength(120)]
    public string VendorNameSnapshot { get; set; } = string.Empty;

    public ReceiptManagementVendor? Vendor { get; set; }

    public int? ExpenseCategoryId { get; set; }

    [Required]
    [StringLength(80)]
    public string CategoryNameSnapshot { get; set; } = string.Empty;

    public ReceiptManagementExpenseCategory? ExpenseCategory { get; set; }

    public decimal SubtotalAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public ReceiptPaymentMethod PaymentMethod { get; set; } = ReceiptPaymentMethod.Cash;

    public ReceiptStatus Status { get; set; } = ReceiptStatus.Recorded;

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [StringLength(3)]
    public string CurrencyCode { get; set; } = ReceiptManagementConstants.CurrencyCode;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ReceiptManagementReceiptItem> Items { get; set; } = new List<ReceiptManagementReceiptItem>();
}
