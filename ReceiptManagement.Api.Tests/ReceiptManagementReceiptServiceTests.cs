using Microsoft.AspNetCore.Http;
using ReceiptManagement.Api.Models.Domain;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;
using ReceiptManagement.Api.Tests.TestDoubles;
using Xunit;

namespace ReceiptManagement.Api.Tests;

public class ReceiptManagementReceiptServiceTests
{
    [Fact]
    public async Task CreateAsync_CalculatesTotalsAndStoresReceipt()
    {
        var fixture = ReceiptServiceFixture.Create();
        var request = new CreateReceiptRequest
        {
            ReceiptNumber = " RCP-TEST-001 ",
            ReceiptDate = new DateTime(2026, 5, 22),
            VendorId = 1,
            ExpenseCategoryId = 1,
            TaxAmount = 1.235m,
            PaymentMethod = ReceiptPaymentMethod.EWallet,
            Status = ReceiptStatus.Recorded,
            Items =
            [
                new CreateReceiptItemRequest { Description = "Notebook", Quantity = 2, UnitPrice = 4.555m },
                new CreateReceiptItemRequest { Description = "Pen", Quantity = 3, UnitPrice = 1.2m }
            ]
        };

        var result = await fixture.Service.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("RCP-TEST-001", result.Data.ReceiptNumber);
        Assert.Equal("Lotus Malaysia", result.Data.VendorName);
        Assert.Equal("Office Supplies", result.Data.CategoryName);
        Assert.Equal(12.72m, result.Data.SubtotalAmount);
        Assert.Equal(1.24m, result.Data.TaxAmount);
        Assert.Equal(13.96m, result.Data.TotalAmount);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Single(fixture.ReceiptRepository.Receipts);
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflictForDuplicateReceiptNumber()
    {
        var fixture = ReceiptServiceFixture.Create();
        fixture.ReceiptRepository.Receipts.Add(new ReceiptManagementReceipt
        {
            ReceiptId = 10,
            ReceiptNumber = "RCP-DUP",
            ReceiptDate = new DateTime(2026, 5, 1),
            VendorNameSnapshot = "Lotus Malaysia",
            CategoryNameSnapshot = "Office Supplies"
        });

        var result = await fixture.Service.CreateAsync(NewValidRequest("rcp-dup"));

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("A receipt with the same receipt number already exists.", result.Message);
    }

    [Fact]
    public async Task CreateAsync_ReturnsBadRequestForMissingRelatedRecords()
    {
        var fixture = ReceiptServiceFixture.Create();
        var request = NewValidRequest("RCP-MISSING");
        request.VendorId = 999;
        request.ExpenseCategoryId = 888;

        var result = await fixture.Service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Receipt contains invalid related records.", result.Message);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFoundWhenReceiptDoesNotExist()
    {
        var fixture = ReceiptServiceFixture.Create();

        var result = await fixture.Service.DeleteAsync(404);

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }

    private static CreateReceiptRequest NewValidRequest(string receiptNumber)
    {
        return new CreateReceiptRequest
        {
            ReceiptNumber = receiptNumber,
            ReceiptDate = new DateTime(2026, 5, 22),
            VendorId = 1,
            ExpenseCategoryId = 1,
            TaxAmount = 0,
            PaymentMethod = ReceiptPaymentMethod.Cash,
            Status = ReceiptStatus.Recorded,
            Items =
            [
                new CreateReceiptItemRequest
                {
                    Description = "Receipt item",
                    Quantity = 1,
                    UnitPrice = 10
                }
            ]
        };
    }

    private sealed class ReceiptServiceFixture
    {
        private ReceiptServiceFixture(
            ReceiptManagementReceiptService service,
            FakeReceiptRepository receiptRepository)
        {
            Service = service;
            ReceiptRepository = receiptRepository;
        }

        public ReceiptManagementReceiptService Service { get; }
        public FakeReceiptRepository ReceiptRepository { get; }

        public static ReceiptServiceFixture Create()
        {
            var receiptRepository = new FakeReceiptRepository();
            var vendorRepository = new FakeVendorRepository(
            [
                new ReceiptManagementVendor { VendorId = 1, Name = "Lotus Malaysia" }
            ]);
            var categoryRepository = new FakeCategoryRepository(
            [
                new ReceiptManagementExpenseCategory { ExpenseCategoryId = 1, Name = "Office Supplies" }
            ]);

            var service = new ReceiptManagementReceiptService(receiptRepository, vendorRepository, categoryRepository);
            return new ReceiptServiceFixture(service, receiptRepository);
        }
    }
}
