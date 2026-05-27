using ReceiptManagement.Api.Configuration;
using ReceiptManagement.Api.Models.Domain;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Repositories;

namespace ReceiptManagement.Api.Services;

public class ReceiptManagementReceiptService : IReceiptManagementReceiptService
{
    private readonly IReceiptManagementReceiptRepository _receiptRepository;
    private readonly IReceiptManagementVendorRepository _vendorRepository;
    private readonly IReceiptManagementExpenseCategoryRepository _categoryRepository;

    public ReceiptManagementReceiptService(
        IReceiptManagementReceiptRepository receiptRepository,
        IReceiptManagementVendorRepository vendorRepository,
        IReceiptManagementExpenseCategoryRepository categoryRepository)
    {
        _receiptRepository = receiptRepository;
        _vendorRepository = vendorRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ServiceResult<List<ReceiptDto>>> GetAllAsync()
    {
        var receipts = await _receiptRepository.GetAllAsync();
        return ServiceResult<List<ReceiptDto>>.Ok(receipts.Select(MapToDto).ToList(), "Receipts retrieved successfully.");
    }

    public async Task<ServiceResult<ReceiptDto>> GetByIdAsync(int id)
    {
        var receipt = await _receiptRepository.GetByIdAsync(id);
        if (receipt is null)
        {
            return ServiceResult<ReceiptDto>.Fail("Receipt was not found.", StatusCodes.Status404NotFound);
        }

        return ServiceResult<ReceiptDto>.Ok(MapToDto(receipt), "Receipt retrieved successfully.");
    }

    public async Task<ServiceResult<ReceiptDto>> CreateAsync(CreateReceiptRequest request)
    {
        NormalizeRequest(request);
        var validationErrors = ValidateReceiptRequest(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<ReceiptDto>.Fail("Validation failed.", StatusCodes.Status400BadRequest, validationErrors);
        }

        var duplicate = await _receiptRepository.GetByReceiptNumberAsync(request.ReceiptNumber.Trim());
        if (duplicate is not null)
        {
            return ServiceResult<ReceiptDto>.Fail("A receipt with the same receipt number already exists.", StatusCodes.Status409Conflict);
        }

        var validation = await ValidateRelatedRecordsAsync(request);
        if (!validation.Success)
        {
            return ServiceResult<ReceiptDto>.Fail(validation.Message, validation.StatusCode, validation.Errors);
        }

        var receipt = BuildReceipt(request, validation.Vendor!, validation.Category!);
        await _receiptRepository.AddAsync(receipt);
        await _receiptRepository.SaveChangesAsync();

        return ServiceResult<ReceiptDto>.Created(MapToDto(receipt), "Receipt created successfully.");
    }

    public async Task<ServiceResult<ReceiptDto>> UpdateAsync(int id, UpdateReceiptRequest request)
    {
        NormalizeRequest(request);
        var validationErrors = ValidateReceiptRequest(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<ReceiptDto>.Fail("Validation failed.", StatusCodes.Status400BadRequest, validationErrors);
        }

        var receipt = await _receiptRepository.GetByIdAsync(id);
        if (receipt is null)
        {
            return ServiceResult<ReceiptDto>.Fail("Receipt was not found.", StatusCodes.Status404NotFound);
        }

        var duplicate = await _receiptRepository.GetByReceiptNumberAsync(request.ReceiptNumber.Trim());
        if (duplicate is not null && duplicate.ReceiptId != id)
        {
            return ServiceResult<ReceiptDto>.Fail("A receipt with the same receipt number already exists.", StatusCodes.Status409Conflict);
        }

        var validation = await ValidateRelatedRecordsAsync(request);
        if (!validation.Success)
        {
            return ServiceResult<ReceiptDto>.Fail(validation.Message, validation.StatusCode, validation.Errors);
        }

        ApplyReceiptRequest(receipt, request, validation.Vendor!, validation.Category!);
        receipt.UpdatedAt = DateTime.UtcNow;

        await _receiptRepository.SaveChangesAsync();

        return ServiceResult<ReceiptDto>.Ok(MapToDto(receipt), "Receipt updated successfully.");
    }

    public async Task<ServiceResult<object>> DeleteAsync(int id)
    {
        var receipt = await _receiptRepository.GetByIdAsync(id);
        if (receipt is null)
        {
            return ServiceResult<object>.Fail("Receipt was not found.", StatusCodes.Status404NotFound);
        }

        _receiptRepository.Delete(receipt);
        await _receiptRepository.SaveChangesAsync();
        return ServiceResult<object>.NoContent();
    }

    private async Task<RelatedRecordValidation> ValidateRelatedRecordsAsync(CreateReceiptRequest request)
    {
        var vendor = await _vendorRepository.GetByIdAsync(request.VendorId.GetValueOrDefault());
        var category = await _categoryRepository.GetByIdAsync(request.ExpenseCategoryId.GetValueOrDefault());
        var errors = new Dictionary<string, string[]>();

        if (vendor is null)
        {
            errors["vendorId"] = ["The selected vendor does not exist."];
        }

        if (category is null)
        {
            errors["expenseCategoryId"] = ["The selected expense category does not exist."];
        }

        if (errors.Count > 0)
        {
            return RelatedRecordValidation.Fail("Receipt contains invalid related records.", StatusCodes.Status400BadRequest, errors);
        }

        return RelatedRecordValidation.Ok(vendor!, category!);
    }

    private static ReceiptManagementReceipt BuildReceipt(
        CreateReceiptRequest request,
        ReceiptManagementVendor vendor,
        ReceiptManagementExpenseCategory category)
    {
        var receipt = new ReceiptManagementReceipt();
        ApplyReceiptRequest(receipt, request, vendor, category);
        return receipt;
    }

    private static void ApplyReceiptRequest(
        ReceiptManagementReceipt receipt,
        CreateReceiptRequest request,
        ReceiptManagementVendor vendor,
        ReceiptManagementExpenseCategory category)
    {
        receipt.ReceiptNumber = request.ReceiptNumber.Trim();
        receipt.ReceiptDate = request.ReceiptDate.GetValueOrDefault().Date;
        receipt.VendorId = vendor.VendorId;
        receipt.VendorNameSnapshot = vendor.Name;
        receipt.ExpenseCategoryId = category.ExpenseCategoryId;
        receipt.CategoryNameSnapshot = category.Name;
        receipt.TaxAmount = RoundMoney(request.TaxAmount);
        receipt.PaymentMethod = request.PaymentMethod.GetValueOrDefault();
        receipt.Status = request.Status.GetValueOrDefault();
        receipt.Notes = NormalizeOptionalText(request.Notes);
        receipt.ImageUrl = NormalizeOptionalText(request.ImageUrl);
        receipt.CurrencyCode = ReceiptManagementConstants.CurrencyCode;

        receipt.Items.Clear();
        foreach (var itemRequest in request.Items)
        {
            var quantity = RoundQuantity(itemRequest.Quantity);
            var unitPrice = RoundMoney(itemRequest.UnitPrice);
            receipt.Items.Add(new ReceiptManagementReceiptItem
            {
                Description = itemRequest.Description.Trim(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                LineTotal = RoundMoney(quantity * unitPrice),
                Notes = NormalizeOptionalText(itemRequest.Notes)
            });
        }

        receipt.SubtotalAmount = RoundMoney(receipt.Items.Sum(item => item.LineTotal));
        receipt.TotalAmount = RoundMoney(receipt.SubtotalAmount + receipt.TaxAmount);
    }

    private static ReceiptDto MapToDto(ReceiptManagementReceipt receipt)
    {
        return new ReceiptDto
        {
            ReceiptId = receipt.ReceiptId,
            ReceiptNumber = receipt.ReceiptNumber,
            ReceiptDate = receipt.ReceiptDate,
            VendorId = receipt.VendorId,
            VendorName = receipt.Vendor?.Name ?? receipt.VendorNameSnapshot,
            ExpenseCategoryId = receipt.ExpenseCategoryId,
            CategoryName = receipt.ExpenseCategory?.Name ?? receipt.CategoryNameSnapshot,
            SubtotalAmount = receipt.SubtotalAmount,
            TaxAmount = receipt.TaxAmount,
            TotalAmount = receipt.TotalAmount,
            CurrencyCode = receipt.CurrencyCode,
            PaymentMethod = receipt.PaymentMethod,
            Status = receipt.Status,
            Notes = receipt.Notes,
            ImageUrl = receipt.ImageUrl,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt,
            Items = receipt.Items
                .OrderBy(item => item.ReceiptItemId)
                .Select(MapItemToDto)
                .ToList()
        };
    }

    private static ReceiptItemDto MapItemToDto(ReceiptManagementReceiptItem item)
    {
        return new ReceiptItemDto
        {
            ReceiptItemId = item.ReceiptItemId,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal,
            Notes = item.Notes
        };
    }

    private static decimal RoundMoney(decimal value)
    {
        return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundQuantity(decimal value)
    {
        return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void NormalizeRequest(CreateReceiptRequest? request)
    {
        if (request is null)
        {
            return;
        }

        request.ReceiptNumber = request.ReceiptNumber?.Trim() ?? string.Empty;
        request.Notes = NormalizeOptionalText(request.Notes);
        request.ImageUrl = NormalizeOptionalText(request.ImageUrl);

        if (request.Items is null)
        {
            return;
        }

        foreach (var item in request.Items)
        {
            if (item is null)
            {
                continue;
            }

            item.Description = item.Description?.Trim() ?? string.Empty;
            item.Notes = NormalizeOptionalText(item.Notes);
        }
    }

    private static Dictionary<string, string[]> ValidateReceiptRequest(CreateReceiptRequest? request)
    {
        var errors = RequestValidation.ValidateObject(request);
        if (request is null)
        {
            return errors;
        }

        if (request.Items is null)
        {
            errors["items"] = ["At least one receipt item is required."];
            return errors;
        }

        for (var index = 0; index < request.Items.Count; index++)
        {
            foreach (var itemError in RequestValidation.ValidateObject(request.Items[index], $"items.{index}"))
            {
                errors[itemError.Key] = itemError.Value;
            }
        }

        return errors;
    }

    private sealed class RelatedRecordValidation
    {
        public bool Success { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public int StatusCode { get; private init; }
        public object? Errors { get; private init; }
        public ReceiptManagementVendor? Vendor { get; private init; }
        public ReceiptManagementExpenseCategory? Category { get; private init; }

        public static RelatedRecordValidation Ok(ReceiptManagementVendor vendor, ReceiptManagementExpenseCategory category)
        {
            return new RelatedRecordValidation
            {
                Success = true,
                Vendor = vendor,
                Category = category
            };
        }

        public static RelatedRecordValidation Fail(string message, int statusCode, object errors)
        {
            return new RelatedRecordValidation
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors
            };
        }
    }
}
