using Microsoft.AspNetCore.Mvc;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;

namespace ReceiptManagement.Api.Controllers;

[Route("api/vendors")]
public class VendorsController : ReceiptManagementControllerBase
{
    private readonly IReceiptManagementVendorService _vendorService;

    public VendorsController(IReceiptManagementVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VendorDto>>>> GetAll()
    {
        return ToResponse(await _vendorService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VendorDto>>> GetById(int id)
    {
        return ToResponse(await _vendorService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VendorDto>>> Create(CreateVendorRequest request)
    {
        var result = await _vendorService.CreateAsync(request);
        if (!result.Success)
        {
            return ToResponse(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.VendorId }, ApiResponse<VendorDto>.Ok(result.Data, result.Message));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<VendorDto>>> Update(int id, UpdateVendorRequest request)
    {
        return ToResponse(await _vendorService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        return ToResponse(await _vendorService.DeleteAsync(id));
    }
}
