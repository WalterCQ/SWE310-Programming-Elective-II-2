using ReceiptManagement.Api.Models.Domain;

namespace ReceiptManagement.Api.Repositories;

public interface IReceiptManagementExpenseCategoryRepository
{
    Task<List<ReceiptManagementExpenseCategory>> GetAllAsync();
    Task<ReceiptManagementExpenseCategory?> GetByIdAsync(int id);
    Task<ReceiptManagementExpenseCategory?> GetByNameAsync(string name);
    Task AddAsync(ReceiptManagementExpenseCategory category);
    void Update(ReceiptManagementExpenseCategory category);
    void Delete(ReceiptManagementExpenseCategory category);
    Task SaveChangesAsync();
}
