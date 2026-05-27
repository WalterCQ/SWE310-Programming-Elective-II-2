using System.ComponentModel.DataAnnotations;
using ReceiptManagement.Api.Configuration;
using ReceiptManagement.Api.Models.Domain;

namespace ReceiptManagement.Api.Models.DTO;

public class ReceiptItemDto
{
    public int ReceiptItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class ReceiptDto
{
    public int ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public int? VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int? ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = ReceiptManagementConstants.CurrencyCode;
    public ReceiptPaymentMethod PaymentMethod { get; set; }
    public ReceiptStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ReceiptItemDto> Items { get; set; } = [];
}

public class CreateReceiptItemRequest
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 99999.99)]
    public decimal Quantity { get; set; } = 1;

    [Range(0, 999999.99)]
    public decimal UnitPrice { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }
}

public class CreateReceiptRequest
{
    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string ReceiptNumber { get; set; } = string.Empty;

    [Required]
    public DateTime? ReceiptDate { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? VendorId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? ExpenseCategoryId { get; set; }

    [Range(0, 999999.99)]
    public decimal TaxAmount { get; set; }

    [Required]
    public ReceiptPaymentMethod? PaymentMethod { get; set; } = ReceiptPaymentMethod.Cash;

    [Required]
    public ReceiptStatus? Status { get; set; } = ReceiptStatus.Recorded;

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [MinLength(1, ErrorMessage = "At least one receipt item is required.")]
    public List<CreateReceiptItemRequest> Items { get; set; } = [];
}

public class UpdateReceiptRequest : CreateReceiptRequest
{
}
