using System.ComponentModel.DataAnnotations;

namespace ReceiptManagement.Api.Models.DTO;

public class ExpenseCategoryDto
{
    public int ExpenseCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyBudget { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseCategoryRequest
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    [Range(0, 999999.99)]
    public decimal MonthlyBudget { get; set; }

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "ColorHex must be a valid hex color such as #00F5FF.")]
    public string ColorHex { get; set; } = "#00F5FF";

    [Required]
    [StringLength(40)]
    public string IconName { get; set; } = "receipt";
}

public class UpdateExpenseCategoryRequest : CreateExpenseCategoryRequest
{
}
