using Microsoft.EntityFrameworkCore;
using ReceiptManagement.Api.Data;
using ReceiptManagement.Api.Models.Domain;

namespace ReceiptManagement.Api.Repositories;

public class ReceiptManagementExpenseCategoryRepository : IReceiptManagementExpenseCategoryRepository
{
    private readonly ReceiptManagementDbContext _dbContext;

    public ReceiptManagementExpenseCategoryRepository(ReceiptManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ReceiptManagementExpenseCategory>> GetAllAsync()
    {
        return _dbContext.ExpenseCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();
    }

    public Task<ReceiptManagementExpenseCategory?> GetByIdAsync(int id)
    {
        return _dbContext.ExpenseCategories.FindAsync(id).AsTask();
    }

    public Task<ReceiptManagementExpenseCategory?> GetByNameAsync(string name)
    {
        return _dbContext.ExpenseCategories
            .FirstOrDefaultAsync(category => category.Name.ToLower() == name.ToLower());
    }

    public async Task AddAsync(ReceiptManagementExpenseCategory category)
    {
        await _dbContext.ExpenseCategories.AddAsync(category);
    }

    public void Update(ReceiptManagementExpenseCategory category)
    {
        _dbContext.ExpenseCategories.Update(category);
    }

    public void Delete(ReceiptManagementExpenseCategory category)
    {
        _dbContext.ExpenseCategories.Remove(category);
    }

    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}
