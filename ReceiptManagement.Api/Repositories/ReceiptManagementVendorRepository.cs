using Microsoft.EntityFrameworkCore;
using ReceiptManagement.Api.Data;
using ReceiptManagement.Api.Models.Domain;

namespace ReceiptManagement.Api.Repositories;

public class ReceiptManagementVendorRepository : IReceiptManagementVendorRepository
{
    private readonly ReceiptManagementDbContext _dbContext;

    public ReceiptManagementVendorRepository(ReceiptManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ReceiptManagementVendor>> GetAllAsync()
    {
        return _dbContext.Vendors
            .AsNoTracking()
            .OrderBy(vendor => vendor.Name)
            .ToListAsync();
    }

    public Task<ReceiptManagementVendor?> GetByIdAsync(int id)
    {
        return _dbContext.Vendors.FindAsync(id).AsTask();
    }

    public Task<ReceiptManagementVendor?> GetByNameAsync(string name)
    {
        return _dbContext.Vendors
            .FirstOrDefaultAsync(vendor => vendor.Name.ToLower() == name.ToLower());
    }

    public async Task AddAsync(ReceiptManagementVendor vendor)
    {
        await _dbContext.Vendors.AddAsync(vendor);
    }

    public void Update(ReceiptManagementVendor vendor)
    {
        _dbContext.Vendors.Update(vendor);
    }

    public void Delete(ReceiptManagementVendor vendor)
    {
        _dbContext.Vendors.Remove(vendor);
    }

    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}
