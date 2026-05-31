using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ReceiptManagement.Api.Configuration;
using ReceiptManagement.Api.Models.DTO;

namespace ReceiptManagement.Api.Services;

public class SiliconFlowReceiptImageAnalysisService : IReceiptImageAnalysisService
{
    private const string DefaultBaseUrl = "https://api.siliconflow.cn/v1";
    private const string DefaultVisionModel = "Qwen/Qwen3-VL-32B-Instruct";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SiliconFlowReceiptImageAnalysisService(
        HttpClient httpClient,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<ServiceResult<ReceiptImageAnalysisDto>> AnalyzeAsync(IFormFile file)
    {
        var apiKey = _configuration["SILICONFLOW_API_KEY"] ?? _configuration["SiliconFlow:ApiKey"];
        if (ShouldUseMockAnalysis(apiKey))
        {
            return await AnalyzeWithMockDataAsync();
        }

        try
        {
            var requestBody = await BuildSiliconFlowRequestAsync(file);
            using var request = new HttpRequestMessage(HttpMethod.Post, GetChatCompletionsEndpoint())
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<ReceiptImageAnalysisDto>.Fail(
                    $"AI receipt analysis failed with HTTP {(int)response.StatusCode}.",
                    StatusCodes.Status502BadGateway,
                    new { details = Truncate(responseJson, 800) });
            }

            var outputText = ExtractJsonObject(ExtractOutputText(responseJson));
            var analysis = JsonSerializer.Deserialize<ReceiptImageAnalysisDto>(outputText, JsonOptions);
            if (analysis is null)
            {
                return ServiceResult<ReceiptImageAnalysisDto>.Fail(
                    "AI receipt analysis returned an empty response.",
                    StatusCodes.Status502BadGateway);
            }

            NormalizeAnalysis(analysis);
            return ServiceResult<ReceiptImageAnalysisDto>.Ok(analysis, "Receipt image analyzed successfully.");
        }
        catch (Exception exception)
        {
            return ServiceResult<ReceiptImageAnalysisDto>.Fail(
                "AI receipt analysis could not be completed.",
                StatusCodes.Status502BadGateway,
                new { details = exception.Message });
        }
    }

    private async Task<object> BuildSiliconFlowRequestAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var dataUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
        var model = _configuration["SILICONFLOW_VISION_MODEL"] ?? _configuration["SiliconFlow:VisionModel"] ?? DefaultVisionModel;

        var request = new Dictionary<string, object>
        {
            ["model"] = model,
            ["messages"] = new object[]
            {
                new
                {
                    role = "system",
                    content = "You extract receipt data and return machine-readable JSON only."
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image_url",
                            image_url = new
                            {
                                url = dataUrl,
                                detail = "high"
                            }
                        },
                        new
                        {
                            type = "text",
                            text = $$"""
                                Analyze this receipt image and extract fields for a {{ReceiptManagementConstants.CurrencyCode}} receipt management form.
                                Return valid JSON only, with exactly this structure:
                                {
                                  "receiptNumber": "string",
                                  "receiptDate": "YYYY-MM-DD or empty string",
                                  "vendorName": "string",
                                  "categoryName": "string",
                                  "taxAmount": 0,
                                  "totalAmount": 0,
                                  "currencyCode": "{{ReceiptManagementConstants.CurrencyCode}}",
                                  "paymentMethod": "Cash|CreditCard|DebitCard|EWallet|BankTransfer|Unknown",
                                  "confidence": 0.0,
                                  "rawTextSummary": "short visible text summary",
                                  "items": [
                                    {
                                      "description": "string",
                                      "quantity": 1,
                                      "unitPrice": 0,
                                      "lineTotal": 0
                                    }
                                  ]
                                }
                                Return best-effort data only from visible receipt content.
                                Use empty strings when a value is not visible. Do not invent vendor, receipt number, date, tax, total, or items.
                                Payment method must be one of Cash, CreditCard, DebitCard, EWallet, BankTransfer, or Unknown.
                                Receipt date must be YYYY-MM-DD when visible, otherwise empty.
                                For items, use visible line items. If line items are not visible but a total is visible, return one item named "Receipt total before tax" using total minus tax as unit price.
                                """
                        }
                    }
                }
            },
            ["temperature"] = 0.1,
            ["max_tokens"] = 1400
        };

        if (IsJsonModeEnabled())
        {
            request["response_format"] = new { type = "json_object" };
        }

        return request;
    }

    private async Task<ServiceResult<ReceiptImageAnalysisDto>> AnalyzeWithMockDataAsync()
    {
        var mockPath = GetMockResponsePath();
        var mockJson = await File.ReadAllTextAsync(mockPath);
        var analysis = JsonSerializer.Deserialize<ReceiptImageAnalysisDto>(mockJson, JsonOptions);
        if (analysis is null)
        {
            return ServiceResult<ReceiptImageAnalysisDto>.Fail(
                "Local mock receipt analysis returned an empty response.",
                StatusCodes.Status500InternalServerError);
        }

        NormalizeAnalysis(analysis);
        return ServiceResult<ReceiptImageAnalysisDto>.Ok(analysis, "Receipt image analyzed with local mock data.");
    }

    private bool ShouldUseMockAnalysis(string? apiKey)
    {
        return string.IsNullOrWhiteSpace(apiKey) || _webHostEnvironment.IsDevelopment();
    }

    private string GetMockResponsePath()
    {
        var configuredPath = _configuration["SiliconFlow:MockResponsePath"];
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(_webHostEnvironment.ContentRootPath, configuredPath);
        }

        return Path.Combine(_webHostEnvironment.ContentRootPath, "MockData", "receipt-image-analysis.mock.json");
    }

    private string GetChatCompletionsEndpoint()
    {
        var baseUrl = _configuration["SILICONFLOW_BASE_URL"] ?? _configuration["SiliconFlow:BaseUrl"] ?? DefaultBaseUrl;
        return $"{baseUrl.TrimEnd('/')}/chat/completions";
    }

    private bool IsJsonModeEnabled()
    {
        var value = _configuration["SILICONFLOW_JSON_MODE"] ?? _configuration["SiliconFlow:JsonMode"];
        return bool.TryParse(value, out var enabled) && enabled;
    }

    private static string ExtractOutputText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        if (!document.RootElement.TryGetProperty("choices", out var choices))
        {
            throw new InvalidOperationException("The SiliconFlow response did not include choices.");
        }

        foreach (var choice in choices.EnumerateArray())
        {
            if (!choice.TryGetProperty("message", out var message) ||
                !message.TryGetProperty("content", out var content))
            {
                continue;
            }

            if (content.ValueKind == JsonValueKind.String)
            {
                return content.GetString() ?? string.Empty;
            }

            if (content.ValueKind == JsonValueKind.Array)
            {
                var builder = new StringBuilder();
                foreach (var contentItem in content.EnumerateArray())
                {
                    if (contentItem.TryGetProperty("text", out var text))
                    {
                        builder.Append(text.GetString());
                    }
                }

                if (builder.Length > 0)
                {
                    return builder.ToString();
                }
            }
        }

        throw new InvalidOperationException("The SiliconFlow response did not include message content.");
    }

    private static string ExtractJsonObject(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            trimmed = trimmed.Trim('`').Trim();
            if (trimmed.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed[4..].Trim();
            }
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start < 0 || end < start)
        {
            throw new InvalidOperationException("The SiliconFlow response did not contain a JSON object.");
        }

        return trimmed[start..(end + 1)];
    }

    private static void NormalizeAnalysis(ReceiptImageAnalysisDto analysis)
    {
        analysis.ReceiptNumber = NormalizeText(analysis.ReceiptNumber);
        analysis.ReceiptDate = DateTime.TryParse(analysis.ReceiptDate, out var receiptDate)
            ? receiptDate.ToString("yyyy-MM-dd")
            : string.Empty;
        analysis.VendorName = NormalizeText(analysis.VendorName);
        analysis.CategoryName = NormalizeText(analysis.CategoryName);
        analysis.CurrencyCode = string.IsNullOrWhiteSpace(analysis.CurrencyCode) ? ReceiptManagementConstants.CurrencyCode : analysis.CurrencyCode.Trim().ToUpperInvariant();
        analysis.PaymentMethod = NormalizePaymentMethod(analysis.PaymentMethod);
        analysis.TaxAmount = RoundMoney(Math.Max(0, analysis.TaxAmount));
        analysis.TotalAmount = RoundMoney(Math.Max(0, analysis.TotalAmount));
        analysis.Confidence = Math.Clamp(decimal.Round(analysis.Confidence, 2, MidpointRounding.AwayFromZero), 0, 1);
        analysis.RawTextSummary = NormalizeText(analysis.RawTextSummary);

        analysis.Items = analysis.Items
            .Where(item => !string.IsNullOrWhiteSpace(item.Description))
            .Select(NormalizeItem)
            .Where(item => item.Quantity > 0)
            .ToList();
    }

    private static ReceiptImageAnalysisItemDto NormalizeItem(ReceiptImageAnalysisItemDto item)
    {
        var quantity = decimal.Round(Math.Max(0.01m, item.Quantity), 2, MidpointRounding.AwayFromZero);
        var lineTotal = RoundMoney(Math.Max(0, item.LineTotal));
        var unitPrice = RoundMoney(Math.Max(0, item.UnitPrice));
        if (unitPrice == 0 && lineTotal > 0)
        {
            unitPrice = RoundMoney(lineTotal / quantity);
        }

        if (lineTotal == 0)
        {
            lineTotal = RoundMoney(quantity * unitPrice);
        }

        return new ReceiptImageAnalysisItemDto
        {
            Description = NormalizeText(item.Description),
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = lineTotal
        };
    }

    private static string NormalizePaymentMethod(string value)
    {
        var allowedValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Cash",
            "CreditCard",
            "DebitCard",
            "EWallet",
            "BankTransfer",
            "Unknown"
        };

        return allowedValues.TryGetValue(value.Trim(), out var normalized) ? normalized : "Unknown";
    }

    private static decimal RoundMoney(decimal value)
    {
        return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
