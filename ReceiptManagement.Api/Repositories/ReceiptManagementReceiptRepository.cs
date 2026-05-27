using Microsoft.EntityFrameworkCore;
using ReceiptManagement.Api.Data;
using ReceiptManagement.Api.Models.Domain;

namespace ReceiptManagement.Api.Repositories;

public class ReceiptManagementReceiptRepository : IReceiptManagementReceiptRepository
{
    private readonly ReceiptManagementDbContext _dbContext;

    public ReceiptManagementReceiptRepository(ReceiptManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ReceiptManagementReceipt>> GetAllAsync()
    {
        return _dbContext.Receipts
            .AsNoTracking()
            .Include(receipt => receipt.Items)
            .Include(receipt => receipt.Vendor)
            .Include(receipt => receipt.ExpenseCategory)
            .OrderByDescending(receipt => receipt.ReceiptDate)
            .ThenByDescending(receipt => receipt.ReceiptId)
            .ToListAsync();
    }

    public Task<ReceiptManagementReceipt?> GetByIdAsync(int id)
    {
        return _dbContext.Receipts
            .Include(receipt => receipt.Items)
            .Include(receipt => receipt.Vendor)
            .Include(receipt => receipt.ExpenseCategory)
            .FirstOrDefaultAsync(receipt => receipt.ReceiptId == id);
    }

    public Task<ReceiptManagementReceipt?> GetByReceiptNumberAsync(string receiptNumber)
    {
        return _dbContext.Receipts
            .FirstOrDefaultAsync(receipt => receipt.ReceiptNumber.ToLower() == receiptNumber.ToLower());
    }

    public async Task AddAsync(ReceiptManagementReceipt receipt)
    {
        await _dbContext.Receipts.AddAsync(receipt);
    }

    public void Delete(ReceiptManagementReceipt receipt)
    {
        _dbContext.Receipts.Remove(receipt);
    }

    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}
