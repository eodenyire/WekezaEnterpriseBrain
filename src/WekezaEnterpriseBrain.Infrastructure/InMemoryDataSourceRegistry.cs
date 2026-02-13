using WekezaEnterpriseBrain.Core.DataSources;
using WekezaEnterpriseBrain.Infrastructure.Connectors;

namespace WekezaEnterpriseBrain.Infrastructure;

/// <summary>
/// In-memory implementation of data source registry
/// </summary>
public class InMemoryDataSourceRegistry : IDataSourceRegistry
{
    private readonly Dictionary<Guid, DataSourceConfiguration> _dataSources = new();
    private readonly Dictionary<Guid, IDataSourceConnector> _connectors = new();
    private readonly object _lock = new();

    public Task RegisterDataSourceAsync(DataSourceConfiguration config)
    {
        lock (_lock)
        {
            if (config.Id == Guid.Empty)
            {
                config.Id = Guid.NewGuid();
            }

            config.UpdatedAt = DateTime.UtcNow;
            _dataSources[config.Id] = config;

            // Create appropriate connector
            IDataSourceConnector connector = config.Type switch
            {
                DataSourceType.CoreBanking => new CoreBankingConnector(config),
                DataSourceType.MobileBanking => new MobileBankingConnector(config),
                DataSourceType.FraudSystem => new FraudSystemConnector(config),
                _ => throw new NotSupportedException($"Connector type {config.Type} is not yet implemented")
            };

            _connectors[config.Id] = connector;
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<DataSourceConfiguration>> GetAllDataSourcesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<DataSourceConfiguration>>(_dataSources.Values.ToList());
        }
    }

    public Task<DataSourceConfiguration?> GetDataSourceAsync(Guid id)
    {
        lock (_lock)
        {
            _dataSources.TryGetValue(id, out var config);
            return Task.FromResult(config);
        }
    }

    public Task<IDataSourceConnector?> GetConnectorAsync(Guid dataSourceId)
    {
        lock (_lock)
        {
            _connectors.TryGetValue(dataSourceId, out var connector);
            return Task.FromResult(connector);
        }
    }

    public Task<bool> EnableDataSourceAsync(Guid id)
    {
        lock (_lock)
        {
            if (_dataSources.TryGetValue(id, out var config))
            {
                config.IsEnabled = true;
                config.UpdatedAt = DateTime.UtcNow;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public Task<bool> DisableDataSourceAsync(Guid id)
    {
        lock (_lock)
        {
            if (_dataSources.TryGetValue(id, out var config))
            {
                config.IsEnabled = false;
                config.UpdatedAt = DateTime.UtcNow;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    public async Task<Dictionary<Guid, DataSourceConnectionResult>> TestAllConnectionsAsync()
    {
        var results = new Dictionary<Guid, DataSourceConnectionResult>();
        
        IEnumerable<DataSourceConfiguration> sources;
        lock (_lock)
        {
            sources = _dataSources.Values.ToList();
        }

        foreach (var source in sources)
        {
            var connector = await GetConnectorAsync(source.Id);
            if (connector != null)
            {
                var result = await connector.TestConnectionAsync();
                results[source.Id] = result;
            }
        }

        return results;
    }
}
