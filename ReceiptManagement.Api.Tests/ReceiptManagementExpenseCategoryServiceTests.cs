using Microsoft.AspNetCore.Http;
using ReceiptManagement.Api.Models.Domain;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;
using ReceiptManagement.Api.Tests.TestDoubles;
using Xunit;

namespace ReceiptManagement.Api.Tests;

public class ReceiptManagementExpenseCategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_StoresRoundedCategoryAndReturnsCreated()
    {
        var repository = new FakeCategoryRepository();
        var service = new ReceiptManagementExpenseCategoryService(repository);
        var request = new CreateExpenseCategoryRequest
        {
            Name = " Study ",
            Description = " Books and materials ",
            MonthlyBudget = 125.555m,
            ColorHex = " #ABC123 ",
            IconName = " graduation-cap "
        };

        var result = await service.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Study", result.Data.Name);
        Assert.Equal("Books and materials", result.Data.Description);
        Assert.Equal(125.56m, result.Data.MonthlyBudget);
        Assert.Equal("#ABC123", result.Data.ColorHex);
        Assert.Equal("graduation-cap", result.Data.IconName);
        Assert.Single(repository.Categories);
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflictForDuplicateCategoryName()
    {
        var repository = new FakeCategoryRepository(
        [
            new ReceiptManagementExpenseCategory { ExpenseCategoryId = 1, Name = "Study" }
        ]);
        var service = new ReceiptManagementExpenseCategoryService(repository);

        var result = await service.CreateAsync(new CreateExpenseCategoryRequest { Name = "study" });

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("An expense category with the same name already exists.", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFoundWhenCategoryDoesNotExist()
    {
        var service = new ReceiptManagementExpenseCategoryService(new FakeCategoryRepository());

        var result = await service.UpdateAsync(404, new UpdateExpenseCategoryRequest { Name = "Missing Category" });

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategoryAndReturnsNoContent()
    {
        var repository = new FakeCategoryRepository(
        [
            new ReceiptManagementExpenseCategory { ExpenseCategoryId = 1, Name = "Study" }
        ]);
        var service = new ReceiptManagementExpenseCategoryService(repository);

        var result = await service.DeleteAsync(1);

        Assert.True(result.Success);
        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
        Assert.Empty(repository.Categories);
    }
}
