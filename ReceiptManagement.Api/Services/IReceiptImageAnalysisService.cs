using ReceiptManagement.Api.Models.DTO;

namespace ReceiptManagement.Api.Services;

public interface IReceiptImageAnalysisService
{
    Task<ServiceResult<ReceiptImageAnalysisDto>> AnalyzeAsync(IFormFile file);
}
