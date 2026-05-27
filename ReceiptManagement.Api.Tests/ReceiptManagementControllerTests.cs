using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using ReceiptManagement.Api.Configuration;
using ReceiptManagement.Api.Controllers;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;
using Xunit;

namespace ReceiptManagement.Api.Tests;

public class ReceiptManagementControllerTests
{
    [Fact]
    public async Task VendorCreate_ReturnsCreatedAtActionForCreatedResult()
    {
        var controller = new VendorsController(new StubVendorService
        {
            CreateResult = ServiceResult<VendorDto>.Created(new VendorDto { VendorId = 12, Name = "Campus Cafe" })
        });

        var response = await controller.Create(new CreateVendorRequest { Name = "Campus Cafe" });

        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(VendorsController.GetById), createdResult.ActionName);
        var body = Assert.IsType<ApiResponse<VendorDto>>(createdResult.Value);
        Assert.True(body.Success);
        Assert.Equal(12, body.Data?.VendorId);
    }

    [Fact]
    public async Task VendorDelete_ReturnsNoContentForNoContentResult()
    {
        var controller = new VendorsController(new StubVendorService
        {
            DeleteResult = ServiceResult<object>.NoContent()
        });

        var response = await controller.Delete(12);

        Assert.IsType<NoContentResult>(response.Result);
    }

    [Fact]
    public async Task VendorGetById_ReturnsNotFoundForMissingResult()
    {
        var controller = new VendorsController(new StubVendorService
        {
            GetByIdResult = ServiceResult<VendorDto>.Fail("Vendor was not found.", StatusCodes.Status404NotFound)
        });

        var response = await controller.GetById(404);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        var body = Assert.IsType<ApiResponse<VendorDto>>(objectResult.Value);
        Assert.False(body.Success);
        Assert.Equal("Vendor was not found.", body.Message);
    }

    [Fact]
    public async Task VendorCreate_ReturnsConflictForDuplicateResult()
    {
        var controller = new VendorsController(new StubVendorService
        {
            CreateResult = ServiceResult<VendorDto>.Fail("A vendor with the same name already exists.", StatusCodes.Status409Conflict)
        });

        var response = await controller.Create(new CreateVendorRequest { Name = "Campus Cafe" });

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        var body = Assert.IsType<ApiResponse<VendorDto>>(objectResult.Value);
        Assert.False(body.Success);
    }

    [Fact]
    public async Task ReceiptUploadImage_ReturnsBadRequestWhenFileIsMissing()
    {
        var controller = new ReceiptsController(
            new StubReceiptService(),
            new StubReceiptImageAnalysisService(),
            Options.Create(new ReceiptImageOptions()),
            new StubWebHostEnvironment());

        var response = await controller.UploadImage(null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        var body = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(body.Success);
        Assert.Equal("No image file was uploaded.", body.Message);
    }

    [Fact]
    public async Task ReceiptAnalyzeImage_ReturnsServiceUnavailableWhenApiKeyIsMissing()
    {
        var controller = new ReceiptsController(
            new StubReceiptService(),
            new StubReceiptImageAnalysisService
            {
                AnalyzeResult = ServiceResult<ReceiptImageAnalysisDto>.Fail(
                    "SILICONFLOW_API_KEY is not configured. Add it to your local .env file or shell environment.",
                    StatusCodes.Status503ServiceUnavailable)
            },
            Options.Create(new ReceiptImageOptions()),
            new StubWebHostEnvironment());
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "receipt.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var response = await controller.AnalyzeImage(file);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
        var body = Assert.IsType<ApiResponse<ReceiptImageAnalysisDto>>(objectResult.Value);
        Assert.False(body.Success);
    }

    private sealed class StubVendorService : IReceiptManagementVendorService
    {
        public ServiceResult<List<VendorDto>> GetAllResult { get; set; } = ServiceResult<List<VendorDto>>.Ok([]);
        public ServiceResult<VendorDto> GetByIdResult { get; set; } = ServiceResult<VendorDto>.Ok(new VendorDto());
        public ServiceResult<VendorDto> CreateResult { get; set; } = ServiceResult<VendorDto>.Created(new VendorDto());
        public ServiceResult<VendorDto> UpdateResult { get; set; } = ServiceResult<VendorDto>.Ok(new VendorDto());
        public ServiceResult<object> DeleteResult { get; set; } = ServiceResult<object>.NoContent();

        public Task<ServiceResult<List<VendorDto>>> GetAllAsync()
        {
            return Task.FromResult(GetAllResult);
        }

        public Task<ServiceResult<VendorDto>> GetByIdAsync(int id)
        {
            return Task.FromResult(GetByIdResult);
        }

        public Task<ServiceResult<VendorDto>> CreateAsync(CreateVendorRequest request)
        {
            return Task.FromResult(CreateResult);
        }

        public Task<ServiceResult<VendorDto>> UpdateAsync(int id, UpdateVendorRequest request)
        {
            return Task.FromResult(UpdateResult);
        }

        public Task<ServiceResult<object>> DeleteAsync(int id)
        {
            return Task.FromResult(DeleteResult);
        }
    }

    private sealed class StubReceiptService : IReceiptManagementReceiptService
    {
        public Task<ServiceResult<List<ReceiptDto>>> GetAllAsync()
        {
            return Task.FromResult(ServiceResult<List<ReceiptDto>>.Ok([]));
        }

        public Task<ServiceResult<ReceiptDto>> GetByIdAsync(int id)
        {
            return Task.FromResult(ServiceResult<ReceiptDto>.Ok(new ReceiptDto()));
        }

        public Task<ServiceResult<ReceiptDto>> CreateAsync(CreateReceiptRequest request)
        {
            return Task.FromResult(ServiceResult<ReceiptDto>.Created(new ReceiptDto()));
        }

        public Task<ServiceResult<ReceiptDto>> UpdateAsync(int id, UpdateReceiptRequest request)
        {
            return Task.FromResult(ServiceResult<ReceiptDto>.Ok(new ReceiptDto()));
        }

        public Task<ServiceResult<object>> DeleteAsync(int id)
        {
            return Task.FromResult(ServiceResult<object>.NoContent());
        }
    }

    private sealed class StubReceiptImageAnalysisService : IReceiptImageAnalysisService
    {
        public ServiceResult<ReceiptImageAnalysisDto> AnalyzeResult { get; set; } = ServiceResult<ReceiptImageAnalysisDto>.Ok(new ReceiptImageAnalysisDto());

        public Task<ServiceResult<ReceiptImageAnalysisDto>> AnalyzeAsync(IFormFile file)
        {
            return Task.FromResult(AnalyzeResult);
        }
    }

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ReceiptManagement.Api.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
