using ReceiptManagement.Api.Models.DTO;

namespace ReceiptManagement.Api.Services;

public interface IReceiptManagementExpenseCategoryService
{
    Task<ServiceResult<List<ExpenseCategoryDto>>> GetAllAsync();
    Task<ServiceResult<ExpenseCategoryDto>> GetByIdAsync(int id);
    Task<ServiceResult<ExpenseCategoryDto>> CreateAsync(CreateExpenseCategoryRequest request);
    Task<ServiceResult<ExpenseCategoryDto>> UpdateAsync(int id, UpdateExpenseCategoryRequest request);
    Task<ServiceResult<object>> DeleteAsync(int id);
}
