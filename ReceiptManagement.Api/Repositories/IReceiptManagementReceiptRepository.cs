using ReceiptManagement.Api.Models.Domain;

namespace ReceiptManagement.Api.Repositories;

public interface IReceiptManagementReceiptRepository
{
    Task<List<ReceiptManagementReceipt>> GetAllAsync();
    Task<ReceiptManagementReceipt?> GetByIdAsync(int id);
    Task<ReceiptManagementReceipt?> GetByReceiptNumberAsync(string receiptNumber);
    Task AddAsync(ReceiptManagementReceipt receipt);
    void Delete(ReceiptManagementReceipt receipt);
    Task SaveChangesAsync();
}
