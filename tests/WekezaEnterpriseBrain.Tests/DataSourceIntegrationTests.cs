using Xunit;
using WekezaEnterpriseBrain.Core.DataSources;
using WekezaEnterpriseBrain.Infrastructure;
using WekezaEnterpriseBrain.Infrastructure.Connectors;

namespace WekezaEnterpriseBrain.Tests;

public class DataSourceIntegrationTests
{
    [Fact]
    public async Task RegisterDataSource_ShouldCreateNewDataSource()
    {
        // Arrange
        var registry = new InMemoryDataSourceRegistry();
        var config = new DataSourceConfiguration
        {
            Name = "Test Core Banking",
            Type = DataSourceType.CoreBanking,
            ConnectionString = "test connection",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await registry.RegisterDataSourceAsync(config);

        // Assert
        var retrieved = await registry.GetDataSourceAsync(config.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Core Banking", retrieved.Name);
    }

    [Fact]
    public async Task GetConnector_ShouldReturnCorrectConnector()
    {
        // Arrange
        var registry = new InMemoryDataSourceRegistry();
        var config = new DataSourceConfiguration
        {
            Name = "Test Mobile Banking",
            Type = DataSourceType.MobileBanking,
            ConnectionString = "test",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await registry.RegisterDataSourceAsync(config);
        var connector = await registry.GetConnectorAsync(config.Id);

        // Assert
        Assert.NotNull(connector);
        Assert.Equal(DataSourceType.MobileBanking, connector.Type);
    }

    [Fact]
    public async Task TestConnection_ShouldSucceed()
    {
        // Arrange
        var config = new DataSourceConfiguration
        {
            Name = "Test Core Banking",
            Type = DataSourceType.CoreBanking,
            ConnectionString = "test"
        };
        var connector = new CoreBankingConnector(config);

        // Act
        var result = await connector.TestConnectionAsync();

        // Assert
        Assert.True(result.IsConnected);
        Assert.Contains("Successfully", result.Message);
    }

    [Fact]
    public async Task FetchCustomers_ShouldReturnSampleData()
    {
        // Arrange
        var config = new DataSourceConfiguration
        {
            Name = "Test Core Banking",
            Type = DataSourceType.CoreBanking,
            ConnectionString = "test"
        };
        var connector = new CoreBankingConnector(config);

        // Act
        var customers = await connector.FetchCustomersAsync();

        // Assert
        Assert.NotEmpty(customers);
        Assert.All(customers, c => Assert.Equal("CoreBanking", c.SourceSystem));
    }

    [Fact]
    public async Task EnableDisableDataSource_ShouldWork()
    {
        // Arrange
        var registry = new InMemoryDataSourceRegistry();
        var config = new DataSourceConfiguration
        {
            Name = "Test Source",
            Type = DataSourceType.CoreBanking,
            ConnectionString = "test",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        await registry.RegisterDataSourceAsync(config);

        // Act - Disable
        var disableResult = await registry.DisableDataSourceAsync(config.Id);
        var afterDisable = await registry.GetDataSourceAsync(config.Id);

        // Assert
        Assert.True(disableResult);
        Assert.False(afterDisable?.IsEnabled);

        // Act - Enable
        var enableResult = await registry.EnableDataSourceAsync(config.Id);
        var afterEnable = await registry.GetDataSourceAsync(config.Id);

        // Assert
        Assert.True(enableResult);
        Assert.True(afterEnable?.IsEnabled);
    }
}
