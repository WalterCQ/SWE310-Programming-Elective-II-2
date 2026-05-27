using Microsoft.AspNetCore.Mvc;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;

namespace ReceiptManagement.Api.Controllers;

[Route("api/expense-categories")]
public class ExpenseCategoriesController : ReceiptManagementControllerBase
{
    private readonly IReceiptManagementExpenseCategoryService _categoryService;

    public ExpenseCategoriesController(IReceiptManagementExpenseCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ExpenseCategoryDto>>>> GetAll()
    {
        return ToResponse(await _categoryService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> GetById(int id)
    {
        return ToResponse(await _categoryService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Create(CreateExpenseCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        if (!result.Success)
        {
            return ToResponse(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.ExpenseCategoryId }, ApiResponse<ExpenseCategoryDto>.Ok(result.Data, result.Message));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Update(int id, UpdateExpenseCategoryRequest request)
    {
        return ToResponse(await _categoryService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        return ToResponse(await _categoryService.DeleteAsync(id));
    }
}
