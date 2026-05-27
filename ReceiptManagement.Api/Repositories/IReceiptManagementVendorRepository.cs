using ReceiptManagement.Api.Models.Domain;

namespace ReceiptManagement.Api.Repositories;

public interface IReceiptManagementVendorRepository
{
    Task<List<ReceiptManagementVendor>> GetAllAsync();
    Task<ReceiptManagementVendor?> GetByIdAsync(int id);
    Task<ReceiptManagementVendor?> GetByNameAsync(string name);
    Task AddAsync(ReceiptManagementVendor vendor);
    void Update(ReceiptManagementVendor vendor);
    void Delete(ReceiptManagementVendor vendor);
    Task SaveChangesAsync();
}
