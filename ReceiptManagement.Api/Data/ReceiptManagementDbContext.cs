using Microsoft.EntityFrameworkCore;
using ReceiptManagement.Api.Configuration;
using ReceiptManagement.Api.Models.Domain;

namespace ReceiptManagement.Api.Data;

public class ReceiptManagementDbContext : DbContext
{
    public ReceiptManagementDbContext(DbContextOptions<ReceiptManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReceiptManagementVendor> Vendors => Set<ReceiptManagementVendor>();
    public DbSet<ReceiptManagementExpenseCategory> ExpenseCategories => Set<ReceiptManagementExpenseCategory>();
    public DbSet<ReceiptManagementReceipt> Receipts => Set<ReceiptManagementReceipt>();
    public DbSet<ReceiptManagementReceiptItem> ReceiptItems => Set<ReceiptManagementReceiptItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReceiptManagementVendor>(entity =>
        {
            entity.ToTable("ReceiptManagementVendors");
            entity.HasKey(vendor => vendor.VendorId);
            entity.HasIndex(vendor => vendor.Name).IsUnique();
            entity.Property(vendor => vendor.Name).HasMaxLength(120).IsRequired();
            entity.Property(vendor => vendor.ContactPerson).HasMaxLength(100);
            entity.Property(vendor => vendor.Phone).HasMaxLength(30);
            entity.Property(vendor => vendor.Email).HasMaxLength(120);
            entity.Property(vendor => vendor.Address).HasMaxLength(250);
            entity.Property(vendor => vendor.TaxRegistrationNumber).HasMaxLength(60);
            entity.Property(vendor => vendor.Notes).HasMaxLength(300);
            entity.Property(vendor => vendor.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<ReceiptManagementExpenseCategory>(entity =>
        {
            entity.ToTable("ReceiptManagementExpenseCategories", table =>
            {
                table.HasCheckConstraint("CK_ReceiptManagementExpenseCategories_MonthlyBudget", "[MonthlyBudget] >= 0");
                table.HasCheckConstraint("CK_ReceiptManagementExpenseCategories_ColorHex", "[ColorHex] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'");
            });
            entity.HasKey(category => category.ExpenseCategoryId);
            entity.HasIndex(category => category.Name).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(80).IsRequired();
            entity.Property(category => category.Description).HasMaxLength(250);
            entity.Property(category => category.MonthlyBudget).HasColumnType("decimal(12,2)");
            entity.Property(category => category.ColorHex).HasMaxLength(7).IsRequired();
            entity.Property(category => category.IconName).HasMaxLength(40).IsRequired();
            entity.Property(category => category.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<ReceiptManagementReceipt>(entity =>
        {
            entity.ToTable("ReceiptManagementReceipts", table =>
            {
                table.HasCheckConstraint("CK_ReceiptManagementReceipts_SubtotalAmount", "[SubtotalAmount] >= 0");
                table.HasCheckConstraint("CK_ReceiptManagementReceipts_TaxAmount", "[TaxAmount] >= 0");
                table.HasCheckConstraint("CK_ReceiptManagementReceipts_TotalAmount", "[TotalAmount] >= 0");
                table.HasCheckConstraint("CK_ReceiptManagementReceipts_CurrencyCode", $"[CurrencyCode] = '{ReceiptManagementConstants.CurrencyCode}'");
            });
            entity.HasKey(receipt => receipt.ReceiptId);
            entity.HasIndex(receipt => receipt.ReceiptNumber).IsUnique();
            entity.Property(receipt => receipt.ReceiptNumber).HasMaxLength(40).IsRequired();
            entity.Property(receipt => receipt.VendorNameSnapshot).HasMaxLength(120).IsRequired();
            entity.Property(receipt => receipt.CategoryNameSnapshot).HasMaxLength(80).IsRequired();
            entity.Property(receipt => receipt.SubtotalAmount).HasColumnType("decimal(12,2)");
            entity.Property(receipt => receipt.TaxAmount).HasColumnType("decimal(12,2)");
            entity.Property(receipt => receipt.TotalAmount).HasColumnType("decimal(12,2)");
            entity.Property(receipt => receipt.PaymentMethod).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(receipt => receipt.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(receipt => receipt.Notes).HasMaxLength(500);
            entity.Property(receipt => receipt.ImageUrl).HasMaxLength(300);
            entity.Property(receipt => receipt.CurrencyCode).HasMaxLength(3).HasDefaultValue(ReceiptManagementConstants.CurrencyCode).IsRequired();
            entity.Property(receipt => receipt.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity
                .HasOne(receipt => receipt.Vendor)
                .WithMany(vendor => vendor.Receipts)
                .HasForeignKey(receipt => receipt.VendorId)
                // Keep receipt history even when a vendor or category is removed.
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasOne(receipt => receipt.ExpenseCategory)
                .WithMany(category => category.Receipts)
                .HasForeignKey(receipt => receipt.ExpenseCategoryId)
                // Keep receipt history even when a vendor or category is removed.
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReceiptManagementReceiptItem>(entity =>
        {
            entity.ToTable("ReceiptManagementReceiptItems", table =>
            {
                table.HasCheckConstraint("CK_ReceiptManagementReceiptItems_Quantity", "[Quantity] > 0");
                table.HasCheckConstraint("CK_ReceiptManagementReceiptItems_UnitPrice", "[UnitPrice] >= 0");
                table.HasCheckConstraint("CK_ReceiptManagementReceiptItems_LineTotal", "[LineTotal] >= 0");
            });
            entity.HasKey(item => item.ReceiptItemId);
            entity.Property(item => item.Description).HasMaxLength(160).IsRequired();
            entity.Property(item => item.Quantity).HasColumnType("decimal(10,2)");
            entity.Property(item => item.UnitPrice).HasColumnType("decimal(12,2)");
            entity.Property(item => item.LineTotal).HasColumnType("decimal(12,2)");
            entity.Property(item => item.Notes).HasMaxLength(250);

            entity
                .HasOne(item => item.Receipt)
                .WithMany(receipt => receipt.Items)
                .HasForeignKey(item => item.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
