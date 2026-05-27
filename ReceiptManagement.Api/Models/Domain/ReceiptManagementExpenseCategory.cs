using System.ComponentModel.DataAnnotations;

namespace ReceiptManagement.Api.Models.Domain;

public class ReceiptManagementExpenseCategory
{
    public int ExpenseCategoryId { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public decimal MonthlyBudget { get; set; }

    [StringLength(7)]
    public string ColorHex { get; set; } = "#00F5FF";

    [StringLength(40)]
    public string IconName { get; set; } = "receipt";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ReceiptManagementReceipt> Receipts { get; set; } = new List<ReceiptManagementReceipt>();
}
