using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReceiptManagement.Api.Data;
using ReceiptManagement.Api.Models.Domain;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;
using Xunit;

namespace ReceiptManagement.Api.Tests;

public class ReceiptManagementIntegrationTests
{
    [Fact]
    public async Task CreateVendor_ReturnsCreatedJsonShape()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/vendors", new CreateVendorRequest
        {
            Name = "Print Shop",
            Email = "print@example.com"
        });

        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.Equal("Vendor created successfully.", body.GetProperty("message").GetString());
        Assert.Equal("Print Shop", body.GetProperty("data").GetProperty("name").GetString());
        Assert.True(body.GetProperty("data").GetProperty("vendorId").GetInt32() > 0);
    }

    [Fact]
    public async Task CreateVendor_ReturnsConflictForDuplicateName()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/vendors", new CreateVendorRequest
        {
            Name = "Campus Cafe"
        });

        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("A vendor with the same name already exists.", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task CreateVendor_ReturnsBadRequestFieldErrorsForInvalidEmail()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/vendors", new CreateVendorRequest
        {
            Name = "Stationery Desk",
            Email = "not-an-email"
        });

        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("Validation failed.", body.GetProperty("message").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("email", out var emailErrors));
        Assert.NotEmpty(emailErrors.EnumerateArray());
    }

    [Fact]
    public async Task GetVendorById_ReturnsNotFoundJsonShapeForMissingId()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/vendors/404");

        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("Vendor was not found.", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task GetCategories_ReturnsSuccessfulListShape()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/expense-categories");

        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("success").GetBoolean());
        var categories = body.GetProperty("data").EnumerateArray().ToList();
        Assert.Contains(categories, category => category.GetProperty("name").GetString() == "Meals");
        Assert.All(categories, category => Assert.True(category.TryGetProperty("expenseCategoryId", out _)));
    }

    [Fact]
    public async Task CreateReceipt_ReturnsCreatedWithCalculatedTotalsAndItems()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/receipts", new CreateReceiptRequest
        {
            ReceiptNumber = "R-HTTP-001",
            ReceiptDate = new DateTime(2026, 5, 26),
            VendorId = 1,
            ExpenseCategoryId = 1,
            TaxAmount = 1.25m,
            PaymentMethod = ReceiptPaymentMethod.EWallet,
            Status = ReceiptStatus.Recorded,
            Items =
            [
                new CreateReceiptItemRequest { Description = "Nasi lemak", Quantity = 2, UnitPrice = 6.50m },
                new CreateReceiptItemRequest { Description = "Coffee", Quantity = 1, UnitPrice = 4.20m }
            ]
        });

        var body = await ReadJsonAsync(response);
        var data = body.GetProperty("data");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.Equal(17.2m, data.GetProperty("subtotalAmount").GetDecimal());
        Assert.Equal(18.45m, data.GetProperty("totalAmount").GetDecimal());
        Assert.Equal("MYR", data.GetProperty("currencyCode").GetString());
        Assert.Equal(2, data.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task CreateReceipt_ReturnsBadRequestForMissingRelatedRecords()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/receipts", new CreateReceiptRequest
        {
            ReceiptNumber = "R-HTTP-002",
            ReceiptDate = new DateTime(2026, 5, 26),
            VendorId = 999,
            ExpenseCategoryId = 999,
            TaxAmount = 0,
            PaymentMethod = ReceiptPaymentMethod.Cash,
            Status = ReceiptStatus.Recorded,
            Items = [new CreateReceiptItemRequest { Description = "Notebook", Quantity = 1, UnitPrice = 8.90m }]
        });

        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.True(body.GetProperty("errors").TryGetProperty("vendorId", out _));
        Assert.True(body.GetProperty("errors").TryGetProperty("expenseCategoryId", out _));
    }

    [Fact]
    public async Task DeleteReceipt_ReturnsNoContentThenGetReturnsNotFound()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();

        var deleteResponse = await client.DeleteAsync("/api/receipts/1");
        var getResponse = await client.GetAsync("/api/receipts/1");
        var getBody = await ReadJsonAsync(getResponse);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.False(getBody.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task UploadImage_ReturnsOkForValidJpegFileContent()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "receipt.jpg");

        var response = await client.PostAsync("/api/receipts/upload-image", content);
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body.GetProperty("success").GetBoolean());
        Assert.StartsWith("/uploads/receipts/", body.GetProperty("data").GetProperty("imageUrl").GetString());
        Assert.EndsWith(".jpg", body.GetProperty("data").GetProperty("fileName").GetString());
    }

    [Fact]
    public async Task UploadImage_ReturnsBadRequestForFakeImageContent()
    {
        using var factory = new ReceiptManagementApiFactory();
        using var client = factory.CreateClient();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("not an image"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "receipt.png");

        var response = await client.PostAsync("/api/receipts/upload-image", content);
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("The uploaded file content is not a supported receipt image.", body.GetProperty("message").GetString());
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }

    private sealed class ReceiptManagementApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"receipt-management-tests-{Guid.NewGuid():N}";
        private readonly InMemoryDatabaseRoot _databaseRoot = new();
        private readonly string _webRoot = Path.Combine(Path.GetTempPath(), $"receipt-management-wwwroot-{Guid.NewGuid():N}");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Directory.CreateDirectory(_webRoot);

            builder.UseEnvironment("Testing");
            builder.UseWebRoot(_webRoot);
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SQL_PASSWORD"] = "Test_password_123!"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ReceiptManagementDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ReceiptManagementDbContext>>();
                services.RemoveAll<IReceiptImageAnalysisService>();

                services.AddDbContext<ReceiptManagementDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName, _databaseRoot));
                services.AddSingleton<IReceiptImageAnalysisService, StubReceiptImageAnalysisService>();

                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ReceiptManagementDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                Seed(dbContext);
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && Directory.Exists(_webRoot))
            {
                Directory.Delete(_webRoot, recursive: true);
            }
        }

        private static void Seed(ReceiptManagementDbContext dbContext)
        {
            dbContext.Vendors.AddRange(
                new ReceiptManagementVendor { VendorId = 1, Name = "Campus Cafe", Email = "cafe@example.com" },
                new ReceiptManagementVendor { VendorId = 2, Name = "Bookstore", Email = "books@example.com" });

            dbContext.ExpenseCategories.AddRange(
                new ReceiptManagementExpenseCategory { ExpenseCategoryId = 1, Name = "Meals", ColorHex = "#3B82F6", IconName = "utensils" },
                new ReceiptManagementExpenseCategory { ExpenseCategoryId = 2, Name = "Stationery", ColorHex = "#22C55E", IconName = "pen-tool" });

            dbContext.Receipts.Add(new ReceiptManagementReceipt
            {
                ReceiptId = 1,
                ReceiptNumber = "R-SEED-001",
                ReceiptDate = new DateTime(2026, 5, 20),
                VendorId = 1,
                VendorNameSnapshot = "Campus Cafe",
                ExpenseCategoryId = 1,
                CategoryNameSnapshot = "Meals",
                SubtotalAmount = 10,
                TaxAmount = 0.60m,
                TotalAmount = 10.60m,
                PaymentMethod = ReceiptPaymentMethod.EWallet,
                Status = ReceiptStatus.Recorded,
                Items =
                [
                    new ReceiptManagementReceiptItem
                    {
                        ReceiptItemId = 1,
                        Description = "Lunch",
                        Quantity = 1,
                        UnitPrice = 10,
                        LineTotal = 10
                    }
                ]
            });

            dbContext.SaveChanges();
        }
    }

    private sealed class StubReceiptImageAnalysisService : IReceiptImageAnalysisService
    {
        public Task<ServiceResult<ReceiptImageAnalysisDto>> AnalyzeAsync(IFormFile file)
        {
            return Task.FromResult(ServiceResult<ReceiptImageAnalysisDto>.Ok(new ReceiptImageAnalysisDto
            {
                ReceiptNumber = "AI-001",
                VendorName = "Campus Cafe",
                CategoryName = "Meals",
                Confidence = 0.92m
            }));
        }
    }
}
