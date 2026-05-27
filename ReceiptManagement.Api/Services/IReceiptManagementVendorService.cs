using ReceiptManagement.Api.Models.DTO;

namespace ReceiptManagement.Api.Services;

public interface IReceiptManagementVendorService
{
    Task<ServiceResult<List<VendorDto>>> GetAllAsync();
    Task<ServiceResult<VendorDto>> GetByIdAsync(int id);
    Task<ServiceResult<VendorDto>> CreateAsync(CreateVendorRequest request);
    Task<ServiceResult<VendorDto>> UpdateAsync(int id, UpdateVendorRequest request);
    Task<ServiceResult<object>> DeleteAsync(int id);
}
