using Microsoft.AspNetCore.Mvc;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;

namespace ReceiptManagement.Api.Controllers;

[ApiController]
public abstract class ReceiptManagementControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> ToResponse<T>(ServiceResult<T> result)
    {
        if (result.Success)
        {
            if (result.StatusCode == StatusCodes.Status204NoContent)
            {
                return NoContent();
            }

            return StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<T>.Fail(result.Message, result.Errors));
    }
}
