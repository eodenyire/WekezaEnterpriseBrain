using Xunit;
using WekezaEnterpriseBrain.Core.Services;
using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Tests;

public class IdentityResolutionServiceTests
{
    [Fact]
    public async Task CreateGlobalCustomerId_ShouldCreateNewGcid()
    {
        // Arrange
        var service = new InMemoryIdentityResolutionService();

        // Act
        var gcid = await service.CreateGlobalCustomerIdAsync();

        // Assert
        Assert.NotNull(gcid);
        Assert.NotEqual(Guid.Empty, gcid.Id);
        Assert.NotEmpty(gcid.GcidValue);
        Assert.StartsWith("GCID", gcid.GcidValue);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithNationalId_ShouldCreateNewCustomer()
    {
        // Arrange
        var service = new InMemoryIdentityResolutionService();
        var nationalId = "12345678";

        // Act
        var gcid = await service.ResolveOrCreateAsync(nationalId, null, null);

        // Assert
        Assert.NotNull(gcid);
        Assert.NotEqual(Guid.Empty, gcid.Id);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithExistingNationalId_ShouldReturnExistingGcid()
    {
        // Arrange
        var service = new InMemoryIdentityResolutionService();
        var nationalId = "12345678";

        // Act
        var gcid1 = await service.ResolveOrCreateAsync(nationalId, null, null);
        var gcid2 = await service.ResolveOrCreateAsync(nationalId, null, null);

        // Assert
        Assert.Equal(gcid1.Id, gcid2.Id);
    }

    [Fact]
    public async Task FindByNationalId_ShouldReturnMatchingCustomer()
    {
        // Arrange
        var service = new InMemoryIdentityResolutionService();
        var nationalId = "12345678";
        await service.ResolveOrCreateAsync(nationalId, null, null);

        // Act
        var found = await service.FindByNationalIdAsync(nationalId);

        // Assert
        Assert.NotNull(found);
    }

    [Fact]
    public async Task FindByPhone_ShouldReturnMatchingCustomer()
    {
        // Arrange
        var service = new InMemoryIdentityResolutionService();
        var phone = "+254700000000";
        await service.ResolveOrCreateAsync(null, phone, null);

        // Act
        var found = await service.FindByPhoneAsync(phone);

        // Assert
        Assert.NotNull(found);
    }
}
