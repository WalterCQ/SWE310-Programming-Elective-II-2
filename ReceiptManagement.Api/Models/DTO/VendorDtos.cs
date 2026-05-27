using System.ComponentModel.DataAnnotations;

namespace ReceiptManagement.Api.Models.DTO;

public class VendorDto
{
    public int VendorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVendorRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(120)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(60)]
    public string? TaxRegistrationNumber { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }
}

public class UpdateVendorRequest : CreateVendorRequest
{
}
