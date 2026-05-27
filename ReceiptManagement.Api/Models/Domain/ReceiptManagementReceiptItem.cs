using System.ComponentModel.DataAnnotations;

namespace ReceiptManagement.Api.Models.Domain;

public class ReceiptManagementReceiptItem
{
    public int ReceiptItemId { get; set; }

    public int ReceiptId { get; set; }

    public ReceiptManagementReceipt? Receipt { get; set; }

    [Required]
    [StringLength(160)]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }
}
