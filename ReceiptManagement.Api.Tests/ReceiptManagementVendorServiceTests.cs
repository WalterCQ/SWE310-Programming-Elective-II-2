using Microsoft.AspNetCore.Http;
using ReceiptManagement.Api.Models.Domain;
using ReceiptManagement.Api.Models.DTO;
using ReceiptManagement.Api.Services;
using ReceiptManagement.Api.Tests.TestDoubles;
using Xunit;

namespace ReceiptManagement.Api.Tests;

public class ReceiptManagementVendorServiceTests
{
    [Fact]
    public async Task CreateAsync_StoresTrimmedVendorAndReturnsCreated()
    {
        var repository = new FakeVendorRepository();
        var service = new ReceiptManagementVendorService(repository);
        var request = new CreateVendorRequest
        {
            Name = " Campus Cafe ",
            ContactPerson = " Alex Tan ",
            Phone = " 012-3456789 ",
            Email = " finance@campuscafe.test ",
            Address = " Block A ",
            TaxRegistrationNumber = " MY-12345 ",
            Notes = " Student meals "
        };

        var result = await service.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Campus Cafe", result.Data.Name);
        Assert.Equal("Alex Tan", result.Data.ContactPerson);
        Assert.Equal("012-3456789", result.Data.Phone);
        Assert.Equal("finance@campuscafe.test", result.Data.Email);
        Assert.Equal("Block A", result.Data.Address);
        Assert.Equal("MY-12345", result.Data.TaxRegistrationNumber);
        Assert.Equal("Student meals", result.Data.Notes);
        Assert.Single(repository.Vendors);
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflictForDuplicateVendorName()
    {
        var repository = new FakeVendorRepository(
        [
            new ReceiptManagementVendor { VendorId = 1, Name = "Campus Cafe" }
        ]);
        var service = new ReceiptManagementVendorService(repository);

        var result = await service.CreateAsync(new CreateVendorRequest { Name = "campus cafe" });

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("A vendor with the same name already exists.", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFoundWhenVendorDoesNotExist()
    {
        var service = new ReceiptManagementVendorService(new FakeVendorRepository());

        var result = await service.UpdateAsync(404, new UpdateVendorRequest { Name = "Missing Vendor" });

        Assert.False(result.Success);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_RemovesVendorAndReturnsNoContent()
    {
        var repository = new FakeVendorRepository(
        [
            new ReceiptManagementVendor { VendorId = 1, Name = "Campus Cafe" }
        ]);
        var service = new ReceiptManagementVendorService(repository);

        var result = await service.DeleteAsync(1);

        Assert.True(result.Success);
        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
        Assert.Empty(repository.Vendors);
    }
}
