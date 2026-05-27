using ReceiptManagement.Api.Models.DTO;

namespace ReceiptManagement.Api.Services;

public interface IReceiptManagementReceiptService
{
    Task<ServiceResult<List<ReceiptDto>>> GetAllAsync();
    Task<ServiceResult<ReceiptDto>> GetByIdAsync(int id);
    Task<ServiceResult<ReceiptDto>> CreateAsync(CreateReceiptRequest request);
    Task<ServiceResult<ReceiptDto>> UpdateAsync(int id, UpdateReceiptRequest request);
    Task<ServiceResult<object>> DeleteAsync(int id);
}
