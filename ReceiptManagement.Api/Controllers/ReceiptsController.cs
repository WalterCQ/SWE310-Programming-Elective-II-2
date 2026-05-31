using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ReceiptManagement.Api.Configuration;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;

namespace ReceiptManagement.Api.Controllers;

[Route("api/receipts")]
public class ReceiptsController : ReceiptManagementControllerBase
{
    private readonly IReceiptManagementReceiptService _receiptService;
    private readonly IReceiptImageAnalysisService _receiptImageAnalysisService;
    private readonly ReceiptImageOptions _receiptImageOptions;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ReceiptsController(
        IReceiptManagementReceiptService receiptService,
        IReceiptImageAnalysisService receiptImageAnalysisService,
        IOptions<ReceiptImageOptions> receiptImageOptions,
        IWebHostEnvironment webHostEnvironment)
    {
        _receiptService = receiptService;
        _receiptImageAnalysisService = receiptImageAnalysisService;
        _receiptImageOptions = receiptImageOptions.Value;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReceiptDto>>>> GetAll()
    {
        return ToResponse(await _receiptService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ReceiptDto>>> GetById(int id)
    {
        return ToResponse(await _receiptService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReceiptDto>>> Create(CreateReceiptRequest request)
    {
        var result = await _receiptService.CreateAsync(request);
        if (!result.Success)
        {
            return ToResponse(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.ReceiptId }, ApiResponse<ReceiptDto>.Ok(result.Data, result.Message));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ReceiptDto>>> Update(int id, UpdateReceiptRequest request)
    {
        return ToResponse(await _receiptService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        return ToResponse(await _receiptService.DeleteAsync(id));
    }

    [HttpPost("upload-image")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> UploadImage(IFormFile? file)
    {
        var validationError = await ValidateReceiptImageAsync<object>(file);
        if (validationError is not null)
        {
            return validationError;
        }

        var extension = Path.GetExtension(file!.FileName).ToLowerInvariant();
        var uploadsRoot = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "receipts");
        Directory.CreateDirectory(uploadsRoot);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsRoot, storedFileName);

        await using (var fileStream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(fileStream);
        }

        var imageUrl = $"/uploads/receipts/{storedFileName}";
        return Ok(ApiResponse<object>.Ok(new { imageUrl, fileName = storedFileName }, "Receipt image uploaded successfully."));
    }

    [HttpPost("analyze-image")]
    [ProducesResponseType(typeof(ApiResponse<ReceiptImageAnalysisDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReceiptImageAnalysisDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReceiptImageAnalysisDto>), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ApiResponse<ReceiptImageAnalysisDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ReceiptImageAnalysisDto>>> AnalyzeImage(IFormFile? file)
    {
        var validationError = await ValidateReceiptImageAsync<ReceiptImageAnalysisDto>(file);
        if (validationError is not null)
        {
            return validationError;
        }

        return ToResponse(await _receiptImageAnalysisService.AnalyzeAsync(file!));
    }

    private async Task<BadRequestObjectResult?> ValidateReceiptImageAsync<T>(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return new BadRequestObjectResult(ApiResponse<T>.Fail("No image file was uploaded."));
        }

        if (file.Length > _receiptImageOptions.MaxFileSizeBytes)
        {
            return new BadRequestObjectResult(ApiResponse<T>.Fail($"Receipt image must not exceed {FormatFileSize(_receiptImageOptions.MaxFileSizeBytes)}."));
        }

        if (!_receiptImageOptions.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return new BadRequestObjectResult(ApiResponse<T>.Fail("Only JPEG, PNG, and WEBP receipt images are allowed."));
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_receiptImageOptions.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return new BadRequestObjectResult(ApiResponse<T>.Fail("The image file extension is not supported."));
        }

        var detectedContentType = await DetectImageContentTypeAsync(file);
        if (detectedContentType is null)
        {
            return new BadRequestObjectResult(ApiResponse<T>.Fail("The uploaded file content is not a supported receipt image."));
        }

        if (!ImageMetadataMatchesContent(file.ContentType, extension, detectedContentType))
        {
            return new BadRequestObjectResult(ApiResponse<T>.Fail("The uploaded file content does not match its image type or extension."));
        }

        return null;
    }

    // Validate file signatures to reject fake images with renamed extensions.
    private static async Task<string?> DetectImageContentTypeAsync(IFormFile file)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header);

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (bytesRead >= 8
            && header[0] == 0x89
            && header[1] == 0x50
            && header[2] == 0x4E
            && header[3] == 0x47
            && header[4] == 0x0D
            && header[5] == 0x0A
            && header[6] == 0x1A
            && header[7] == 0x0A)
        {
            return "image/png";
        }

        if (bytesRead >= 12
            && header[0] == 0x52
            && header[1] == 0x49
            && header[2] == 0x46
            && header[3] == 0x46
            && header[8] == 0x57
            && header[9] == 0x45
            && header[10] == 0x42
            && header[11] == 0x50)
        {
            return "image/webp";
        }

        return null;
    }

    private static bool ImageMetadataMatchesContent(string contentType, string extension, string detectedContentType)
    {
        if (!string.Equals(contentType, detectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return detectedContentType switch
        {
            "image/jpeg" => string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase),
            "image/png" => string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase),
            "image/webp" => string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static string FormatFileSize(long bytes)
    {
        const decimal bytesPerMegabyte = 1024 * 1024;
        return $"{bytes / bytesPerMegabyte:0.#} MB";
    }
}
