using Xunit;
using WekezaEnterpriseBrain.Core.Services;
using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Tests;

public class DecisionEngineServiceTests
{
    [Fact]
    public async Task MakeDecision_WithUnknownCustomer_ShouldDecline()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var service = new DecisionEngineService(customer360Service);
        var request = new DecisionRequest
        {
            GlobalCustomerId = Guid.NewGuid(),
            EventType = "PAYMENT",
            Amount = 1000
        };

        // Act
        var response = await service.MakeDecisionAsync(request);

        // Assert
        Assert.Equal("DECLINE", response.Decision);
        Assert.Equal(1.0m, response.RiskScore);
    }

    [Fact]
    public async Task MakeDecision_LowRiskPayment_ShouldApprove()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var gcid = Guid.NewGuid();
        
        var customer = new Customer360
        {
            GlobalCustomerId = gcid,
            FirstName = "John",
            LastName = "Doe",
            Status = "Active",
            RiskScore = 0.1m,
            RiskCategory = "Low",
            AvailableBalance = 10000m,
            AverageDailyBalance = 5000m
        };
        await customer360Service.CreateOrUpdateAsync(customer);

        var service = new DecisionEngineService(customer360Service);
        var request = new DecisionRequest
        {
            GlobalCustomerId = gcid,
            EventType = "PAYMENT",
            Amount = 1000
        };

        // Act
        var response = await service.MakeDecisionAsync(request);

        // Assert
        Assert.Equal("APPROVE", response.Decision);
        Assert.True(response.RiskScore < 0.5m);
    }

    [Fact]
    public async Task MakeDecision_InsufficientBalance_ShouldDecline()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var gcid = Guid.NewGuid();
        
        var customer = new Customer360
        {
            GlobalCustomerId = gcid,
            FirstName = "Jane",
            LastName = "Doe",
            Status = "Active",
            RiskScore = 0.1m,
            RiskCategory = "Low",
            AvailableBalance = 500m,
            AverageDailyBalance = 500m
        };
        await customer360Service.CreateOrUpdateAsync(customer);

        var service = new DecisionEngineService(customer360Service);
        var request = new DecisionRequest
        {
            GlobalCustomerId = gcid,
            EventType = "PAYMENT",
            Amount = 1000
        };

        // Act
        var response = await service.MakeDecisionAsync(request);

        // Assert
        Assert.Equal("DECLINE", response.Decision);
        Assert.Contains("INSUFFICIENT_BALANCE", response.Flags);
    }

    [Fact]
    public async Task MakeDecision_UnusualAmount_ShouldRequireReview()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var gcid = Guid.NewGuid();
        
        var customer = new Customer360
        {
            GlobalCustomerId = gcid,
            FirstName = "Bob",
            LastName = "Smith",
            Status = "Active",
            RiskScore = 0.2m,
            RiskCategory = "Low",
            AvailableBalance = 10000m,
            AverageDailyBalance = 2000m
        };
        await customer360Service.CreateOrUpdateAsync(customer);

        var service = new DecisionEngineService(customer360Service);
        var request = new DecisionRequest
        {
            GlobalCustomerId = gcid,
            EventType = "PAYMENT",
            Amount = 5000 // More than 2x average daily balance
        };

        // Act
        var response = await service.MakeDecisionAsync(request);

        // Assert
        Assert.Equal("REVIEW", response.Decision);
        Assert.Contains("UNUSUAL_AMOUNT", response.Flags);
    }

    [Fact]
    public async Task CalculateRiskScore_WithHighFailedLoginAttempts_ShouldIncreaseScore()
    {
        // Arrange
        var customer360Service = new InMemoryCustomer360Service();
        var gcid = Guid.NewGuid();
        
        var customer = new Customer360
        {
            GlobalCustomerId = gcid,
            FirstName = "Test",
            LastName = "User",
            Status = "Active",
            RiskScore = 0.1m,
            FailedLoginAttempts = 5
        };
        await customer360Service.CreateOrUpdateAsync(customer);

        var service = new DecisionEngineService(customer360Service);

        // Act
        var riskScore = await service.CalculateRiskScoreAsync(gcid, "LOGIN");

        // Assert
        Assert.True(riskScore > 0.1m);
    }
}
