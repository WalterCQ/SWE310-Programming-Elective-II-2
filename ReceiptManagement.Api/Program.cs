using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReceiptManagement.Api.Configuration;
using ReceiptManagement.Api.Data;
using ReceiptManagement.Api.Middleware;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Repositories;
using ReceiptManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var receiptImageOptions = new ReceiptImageOptions();
builder.Configuration.GetSection(ReceiptImageOptions.SectionName).Bind(receiptImageOptions);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => char.ToLowerInvariant(entry.Key[0]) + entry.Key[1..],
                entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        return new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", errors));
    };
});

builder.Services.AddDbContext<ReceiptManagementDbContext>(options =>
{
    options.UseSqlServer(GetSqlServerConnectionString(builder.Configuration));
});

builder.Services.Configure<ReceiptImageOptions>(builder.Configuration.GetSection(ReceiptImageOptions.SectionName));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = receiptImageOptions.MaxFileSizeBytes;
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy(ReceiptManagementConstants.CorsPolicyName, policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IReceiptManagementVendorRepository, ReceiptManagementVendorRepository>();
builder.Services.AddScoped<IReceiptManagementExpenseCategoryRepository, ReceiptManagementExpenseCategoryRepository>();
builder.Services.AddScoped<IReceiptManagementReceiptRepository, ReceiptManagementReceiptRepository>();
builder.Services.AddScoped<IReceiptManagementVendorService, ReceiptManagementVendorService>();
builder.Services.AddScoped<IReceiptManagementExpenseCategoryService, ReceiptManagementExpenseCategoryService>();
builder.Services.AddScoped<IReceiptManagementReceiptService, ReceiptManagementReceiptService>();
builder.Services.AddHttpClient<IReceiptImageAnalysisService, SiliconFlowReceiptImageAnalysisService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(ReceiptManagementConstants.CorsPolicyName);
app.MapControllers();

app.Run();

static string GetSqlServerConnectionString(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
    }

    var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
    var sqlPassword = configuration["SQL_PASSWORD"];
    if (!string.IsNullOrWhiteSpace(sqlPassword))
    {
        connectionBuilder.Password = sqlPassword;
    }

    if (!connectionBuilder.IntegratedSecurity && string.IsNullOrWhiteSpace(connectionBuilder.Password))
    {
        throw new InvalidOperationException("SQL_PASSWORD must be set for SQL Server authentication.");
    }

    return connectionBuilder.ConnectionString;
}

public partial class Program
{
}
