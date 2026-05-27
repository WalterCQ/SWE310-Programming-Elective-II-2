using ReceiptManagement.Api.Models.Domain;
using ReceiptManagement.Api.Repositories;

namespace ReceiptManagement.Api.Tests.TestDoubles;

public sealed class FakeReceiptRepository : IReceiptManagementReceiptRepository
{
    private int _nextReceiptId = 1;
    private int _nextItemId = 1;

    public List<ReceiptManagementReceipt> Receipts { get; } = [];

    public Task<List<ReceiptManagementReceipt>> GetAllAsync()
    {
        return Task.FromResult(Receipts.ToList());
    }

    public Task<ReceiptManagementReceipt?> GetByIdAsync(int id)
    {
        return Task.FromResult(Receipts.FirstOrDefault(receipt => receipt.ReceiptId == id));
    }

    public Task<ReceiptManagementReceipt?> GetByReceiptNumberAsync(string receiptNumber)
    {
        return Task.FromResult(Receipts.FirstOrDefault(receipt =>
            string.Equals(receipt.ReceiptNumber, receiptNumber, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddAsync(ReceiptManagementReceipt receipt)
    {
        receipt.ReceiptId = _nextReceiptId++;
        foreach (var item in receipt.Items)
        {
            item.ReceiptId = receipt.ReceiptId;
            item.ReceiptItemId = _nextItemId++;
        }

        Receipts.Add(receipt);
        return Task.CompletedTask;
    }

    public void Delete(ReceiptManagementReceipt receipt)
    {
        Receipts.Remove(receipt);
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}

public sealed class FakeVendorRepository : IReceiptManagementVendorRepository
{
    private int _nextVendorId = 1;

    public FakeVendorRepository(IEnumerable<ReceiptManagementVendor>? vendors = null)
    {
        Vendors = vendors?.ToList() ?? [];
        if (Vendors.Count > 0)
        {
            _nextVendorId = Vendors.Max(vendor => vendor.VendorId) + 1;
        }
    }

    public List<ReceiptManagementVendor> Vendors { get; }

    public Task<List<ReceiptManagementVendor>> GetAllAsync()
    {
        return Task.FromResult(Vendors.ToList());
    }

    public Task<ReceiptManagementVendor?> GetByIdAsync(int id)
    {
        return Task.FromResult(Vendors.FirstOrDefault(vendor => vendor.VendorId == id));
    }

    public Task<ReceiptManagementVendor?> GetByNameAsync(string name)
    {
        return Task.FromResult(Vendors.FirstOrDefault(vendor =>
            string.Equals(vendor.Name, name, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddAsync(ReceiptManagementVendor vendor)
    {
        vendor.VendorId = _nextVendorId++;
        Vendors.Add(vendor);
        return Task.CompletedTask;
    }

    public void Update(ReceiptManagementVendor vendor)
    {
    }

    public void Delete(ReceiptManagementVendor vendor)
    {
        Vendors.Remove(vendor);
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}

public sealed class FakeCategoryRepository : IReceiptManagementExpenseCategoryRepository
{
    private int _nextCategoryId = 1;

    public FakeCategoryRepository(IEnumerable<ReceiptManagementExpenseCategory>? categories = null)
    {
        Categories = categories?.ToList() ?? [];
        if (Categories.Count > 0)
        {
            _nextCategoryId = Categories.Max(category => category.ExpenseCategoryId) + 1;
        }
    }

    public List<ReceiptManagementExpenseCategory> Categories { get; }

    public Task<List<ReceiptManagementExpenseCategory>> GetAllAsync()
    {
        return Task.FromResult(Categories.ToList());
    }

    public Task<ReceiptManagementExpenseCategory?> GetByIdAsync(int id)
    {
        return Task.FromResult(Categories.FirstOrDefault(category => category.ExpenseCategoryId == id));
    }

    public Task<ReceiptManagementExpenseCategory?> GetByNameAsync(string name)
    {
        return Task.FromResult(Categories.FirstOrDefault(category =>
            string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddAsync(ReceiptManagementExpenseCategory category)
    {
        category.ExpenseCategoryId = _nextCategoryId++;
        Categories.Add(category);
        return Task.CompletedTask;
    }

    public void Update(ReceiptManagementExpenseCategory category)
    {
    }

    public void Delete(ReceiptManagementExpenseCategory category)
    {
        Categories.Remove(category);
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}
