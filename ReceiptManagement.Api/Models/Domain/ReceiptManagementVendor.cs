using System.ComponentModel.DataAnnotations;

namespace ReceiptManagement.Api.Models.Domain;

public class ReceiptManagementVendor
{
    public int VendorId { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(120)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(60)]
    public string? TaxRegistrationNumber { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ReceiptManagementReceipt> Receipts { get; set; } = new List<ReceiptManagementReceipt>();
}
