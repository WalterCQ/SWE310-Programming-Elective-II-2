using ReceiptManagement.Api.Models.Domain;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Repositories;

namespace ReceiptManagement.Api.Services;

public class ReceiptManagementExpenseCategoryService : IReceiptManagementExpenseCategoryService
{
    private readonly IReceiptManagementExpenseCategoryRepository _categoryRepository;

    public ReceiptManagementExpenseCategoryService(IReceiptManagementExpenseCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ServiceResult<List<ExpenseCategoryDto>>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return ServiceResult<List<ExpenseCategoryDto>>.Ok(categories.Select(MapToDto).ToList(), "Expense categories retrieved successfully.");
    }

    public async Task<ServiceResult<ExpenseCategoryDto>> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return ServiceResult<ExpenseCategoryDto>.Fail("Expense category was not found.", StatusCodes.Status404NotFound);
        }

        return ServiceResult<ExpenseCategoryDto>.Ok(MapToDto(category), "Expense category retrieved successfully.");
    }

    public async Task<ServiceResult<ExpenseCategoryDto>> CreateAsync(CreateExpenseCategoryRequest request)
    {
        NormalizeRequest(request);
        var validationErrors = RequestValidation.ValidateObject(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<ExpenseCategoryDto>.Fail("Validation failed.", StatusCodes.Status400BadRequest, validationErrors);
        }

        var duplicate = await _categoryRepository.GetByNameAsync(request.Name.Trim());
        if (duplicate is not null)
        {
            return ServiceResult<ExpenseCategoryDto>.Fail("An expense category with the same name already exists.", StatusCodes.Status409Conflict);
        }

        var category = new ReceiptManagementExpenseCategory();
        ApplyRequest(category, request);

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return ServiceResult<ExpenseCategoryDto>.Created(MapToDto(category), "Expense category created successfully.");
    }

    public async Task<ServiceResult<ExpenseCategoryDto>> UpdateAsync(int id, UpdateExpenseCategoryRequest request)
    {
        NormalizeRequest(request);
        var validationErrors = RequestValidation.ValidateObject(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<ExpenseCategoryDto>.Fail("Validation failed.", StatusCodes.Status400BadRequest, validationErrors);
        }

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return ServiceResult<ExpenseCategoryDto>.Fail("Expense category was not found.", StatusCodes.Status404NotFound);
        }

        var duplicate = await _categoryRepository.GetByNameAsync(request.Name.Trim());
        if (duplicate is not null && duplicate.ExpenseCategoryId != id)
        {
            return ServiceResult<ExpenseCategoryDto>.Fail("An expense category with the same name already exists.", StatusCodes.Status409Conflict);
        }

        ApplyRequest(category, request);
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        return ServiceResult<ExpenseCategoryDto>.Ok(MapToDto(category), "Expense category updated successfully.");
    }

    public async Task<ServiceResult<object>> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return ServiceResult<object>.Fail("Expense category was not found.", StatusCodes.Status404NotFound);
        }

        _categoryRepository.Delete(category);
        await _categoryRepository.SaveChangesAsync();
        return ServiceResult<object>.NoContent();
    }

    private static void ApplyRequest(ReceiptManagementExpenseCategory category, CreateExpenseCategoryRequest request)
    {
        category.Name = request.Name.Trim();
        category.Description = NormalizeOptionalText(request.Description);
        category.MonthlyBudget = decimal.Round(request.MonthlyBudget, 2, MidpointRounding.AwayFromZero);
        category.ColorHex = request.ColorHex.Trim();
        category.IconName = request.IconName.Trim();
    }

    private static void NormalizeRequest(CreateExpenseCategoryRequest? request)
    {
        if (request is null)
        {
            return;
        }

        request.Name = request.Name?.Trim() ?? string.Empty;
        request.Description = NormalizeOptionalText(request.Description);
        request.ColorHex = request.ColorHex?.Trim() ?? string.Empty;
        request.IconName = request.IconName?.Trim() ?? string.Empty;
    }

    private static ExpenseCategoryDto MapToDto(ReceiptManagementExpenseCategory category)
    {
        return new ExpenseCategoryDto
        {
            ExpenseCategoryId = category.ExpenseCategoryId,
            Name = category.Name,
            Description = category.Description,
            MonthlyBudget = category.MonthlyBudget,
            ColorHex = category.ColorHex,
            IconName = category.IconName,
            CreatedAt = category.CreatedAt
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
