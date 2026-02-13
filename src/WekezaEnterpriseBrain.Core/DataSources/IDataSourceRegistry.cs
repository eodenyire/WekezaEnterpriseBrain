namespace WekezaEnterpriseBrain.Core.DataSources;

/// <summary>
/// Registry for managing data source connectors
/// </summary>
public interface IDataSourceRegistry
{
    Task RegisterDataSourceAsync(DataSourceConfiguration config);
    Task<IEnumerable<DataSourceConfiguration>> GetAllDataSourcesAsync();
    Task<DataSourceConfiguration?> GetDataSourceAsync(Guid id);
    Task<IDataSourceConnector?> GetConnectorAsync(Guid dataSourceId);
    Task<bool> EnableDataSourceAsync(Guid id);
    Task<bool> DisableDataSourceAsync(Guid id);
    Task<Dictionary<Guid, DataSourceConnectionResult>> TestAllConnectionsAsync();
}
