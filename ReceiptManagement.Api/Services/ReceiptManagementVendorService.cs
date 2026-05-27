using ReceiptManagement.Api.Models.Domain;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Repositories;

namespace ReceiptManagement.Api.Services;

public class ReceiptManagementVendorService : IReceiptManagementVendorService
{
    private readonly IReceiptManagementVendorRepository _vendorRepository;

    public ReceiptManagementVendorService(IReceiptManagementVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<ServiceResult<List<VendorDto>>> GetAllAsync()
    {
        var vendors = await _vendorRepository.GetAllAsync();
        return ServiceResult<List<VendorDto>>.Ok(vendors.Select(MapToDto).ToList(), "Vendors retrieved successfully.");
    }

    public async Task<ServiceResult<VendorDto>> GetByIdAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor is null)
        {
            return ServiceResult<VendorDto>.Fail("Vendor was not found.", StatusCodes.Status404NotFound);
        }

        return ServiceResult<VendorDto>.Ok(MapToDto(vendor), "Vendor retrieved successfully.");
    }

    public async Task<ServiceResult<VendorDto>> CreateAsync(CreateVendorRequest request)
    {
        NormalizeRequest(request);
        var validationErrors = RequestValidation.ValidateObject(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<VendorDto>.Fail("Validation failed.", StatusCodes.Status400BadRequest, validationErrors);
        }

        var duplicate = await _vendorRepository.GetByNameAsync(request.Name.Trim());
        if (duplicate is not null)
        {
            return ServiceResult<VendorDto>.Fail("A vendor with the same name already exists.", StatusCodes.Status409Conflict);
        }

        var vendor = new ReceiptManagementVendor();
        ApplyRequest(vendor, request);

        await _vendorRepository.AddAsync(vendor);
        await _vendorRepository.SaveChangesAsync();

        return ServiceResult<VendorDto>.Created(MapToDto(vendor), "Vendor created successfully.");
    }

    public async Task<ServiceResult<VendorDto>> UpdateAsync(int id, UpdateVendorRequest request)
    {
        NormalizeRequest(request);
        var validationErrors = RequestValidation.ValidateObject(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<VendorDto>.Fail("Validation failed.", StatusCodes.Status400BadRequest, validationErrors);
        }

        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor is null)
        {
            return ServiceResult<VendorDto>.Fail("Vendor was not found.", StatusCodes.Status404NotFound);
        }

        var duplicate = await _vendorRepository.GetByNameAsync(request.Name.Trim());
        if (duplicate is not null && duplicate.VendorId != id)
        {
            return ServiceResult<VendorDto>.Fail("A vendor with the same name already exists.", StatusCodes.Status409Conflict);
        }

        ApplyRequest(vendor, request);
        _vendorRepository.Update(vendor);
        await _vendorRepository.SaveChangesAsync();

        return ServiceResult<VendorDto>.Ok(MapToDto(vendor), "Vendor updated successfully.");
    }

    public async Task<ServiceResult<object>> DeleteAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor is null)
        {
            return ServiceResult<object>.Fail("Vendor was not found.", StatusCodes.Status404NotFound);
        }

        _vendorRepository.Delete(vendor);
        await _vendorRepository.SaveChangesAsync();
        return ServiceResult<object>.NoContent();
    }

    private static void ApplyRequest(ReceiptManagementVendor vendor, CreateVendorRequest request)
    {
        vendor.Name = request.Name.Trim();
        vendor.ContactPerson = NormalizeOptionalText(request.ContactPerson);
        vendor.Phone = NormalizeOptionalText(request.Phone);
        vendor.Email = NormalizeOptionalText(request.Email);
        vendor.Address = NormalizeOptionalText(request.Address);
        vendor.TaxRegistrationNumber = NormalizeOptionalText(request.TaxRegistrationNumber);
        vendor.Notes = NormalizeOptionalText(request.Notes);
    }

    private static void NormalizeRequest(CreateVendorRequest? request)
    {
        if (request is null)
        {
            return;
        }

        request.Name = request.Name?.Trim() ?? string.Empty;
        request.ContactPerson = NormalizeOptionalText(request.ContactPerson);
        request.Phone = NormalizeOptionalText(request.Phone);
        request.Email = NormalizeOptionalText(request.Email);
        request.Address = NormalizeOptionalText(request.Address);
        request.TaxRegistrationNumber = NormalizeOptionalText(request.TaxRegistrationNumber);
        request.Notes = NormalizeOptionalText(request.Notes);
    }

    private static VendorDto MapToDto(ReceiptManagementVendor vendor)
    {
        return new VendorDto
        {
            VendorId = vendor.VendorId,
            Name = vendor.Name,
            ContactPerson = vendor.ContactPerson,
            Phone = vendor.Phone,
            Email = vendor.Email,
            Address = vendor.Address,
            TaxRegistrationNumber = vendor.TaxRegistrationNumber,
            Notes = vendor.Notes,
            CreatedAt = vendor.CreatedAt
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
