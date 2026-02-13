using Xunit;
using WekezaEnterpriseBrain.Core.Features;
using WekezaEnterpriseBrain.Core.Services;
using WekezaEnterpriseBrain.Core.Models;
using WekezaEnterpriseBrain.Infrastructure;

namespace WekezaEnterpriseBrain.Tests;

public class FeatureStoreTests
{
    [Fact]
    public async Task CalculateFeatures_ForExistingCustomer_ShouldReturnFeatures()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var featureStore = new InMemoryFeatureStore(customer360Service);
        
        var gcid = Guid.NewGuid();
        var customer = new Customer360
        {
            GlobalCustomerId = gcid,
            FirstName = "Test",
            LastName = "User",
            Status = "Active",
            RiskScore = 0.15m,
            AvailableBalance = 50000m,
            AverageDailyBalance = 45000m,
            LoginCount = 30,
            FailedLoginAttempts = 0
        };
        await customer360Service.CreateOrUpdateAsync(customer);

        // Act
        var features = await featureStore.CalculateFeaturesAsync(gcid);

        // Assert
        Assert.NotNull(features);
        Assert.Equal(gcid, features.GlobalCustomerId);
        Assert.Equal(0.15m, features.CurrentRiskScore);
        Assert.Equal(45000m, features.AverageDailyBalance);
    }

    [Fact]
    public async Task GetFeatures_BeforeCalculation_ShouldReturnNull()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var featureStore = new InMemoryFeatureStore(customer360Service);
        var gcid = Guid.NewGuid();

        // Act
        var features = await featureStore.GetFeaturesAsync(gcid);

        // Assert
        Assert.Null(features);
    }

    [Fact]
    public async Task GetFeatures_AfterCalculation_ShouldReturnCachedFeatures()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var featureStore = new InMemoryFeatureStore(customer360Service);
        
        var gcid = Guid.NewGuid();
        var customer = new Customer360
        {
            GlobalCustomerId = gcid,
            FirstName = "Test",
            LastName = "User",
            Status = "Active",
            RiskScore = 0.25m
        };
        await customer360Service.CreateOrUpdateAsync(customer);
        await featureStore.CalculateFeaturesAsync(gcid);

        // Act
        var features = await featureStore.GetFeaturesAsync(gcid);

        // Assert
        Assert.NotNull(features);
        Assert.Equal(0.25m, features.CurrentRiskScore);
    }

    [Fact]
    public async Task GetFeatureImportance_ShouldReturnScores()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var featureStore = new InMemoryFeatureStore(customer360Service);

        // Act
        var importance = await featureStore.GetFeatureImportanceAsync();

        // Assert
        Assert.NotEmpty(importance);
        Assert.Contains("CurrentRiskScore", importance.Keys);
        Assert.True(importance["CurrentRiskScore"] > 0);
    }
}
