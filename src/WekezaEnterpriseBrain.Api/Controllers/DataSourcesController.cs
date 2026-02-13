using Microsoft.AspNetCore.Mvc;
using WekezaEnterpriseBrain.Core.DataSources;
using WekezaEnterpriseBrain.Core.Interfaces;

namespace WekezaEnterpriseBrain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataSourcesController : ControllerBase
{
    private readonly IDataSourceRegistry _dataSourceRegistry;
    private readonly IDataAggregationService _aggregationService;
    private readonly ILogger<DataSourcesController> _logger;

    public DataSourcesController(
        IDataSourceRegistry dataSourceRegistry,
        IDataAggregationService aggregationService,
        ILogger<DataSourcesController> logger)
    {
        _dataSourceRegistry = dataSourceRegistry;
        _aggregationService = aggregationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DataSourceConfiguration>>> GetAllDataSources()
    {
        var dataSources = await _dataSourceRegistry.GetAllDataSourcesAsync();
        return Ok(dataSources);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DataSourceConfiguration>> GetDataSource(Guid id)
    {
        var dataSource = await _dataSourceRegistry.GetDataSourceAsync(id);
        if (dataSource == null)
        {
            return NotFound(new { message = $"Data source {id} not found" });
        }
        return Ok(dataSource);
    }

    [HttpPost]
    public async Task<ActionResult<DataSourceConfiguration>> RegisterDataSource([FromBody] DataSourceConfiguration config)
    {
        await _dataSourceRegistry.RegisterDataSourceAsync(config);
        return CreatedAtAction(nameof(GetDataSource), new { id = config.Id }, config);
    }

    [HttpPost("{id}/enable")]
    public async Task<ActionResult> EnableDataSource(Guid id)
    {
        var result = await _dataSourceRegistry.EnableDataSourceAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Data source {id} not found" });
        }
        return Ok(new { message = "Data source enabled successfully" });
    }

    [HttpPost("{id}/disable")]
    public async Task<ActionResult> DisableDataSource(Guid id)
    {
        var result = await _dataSourceRegistry.DisableDataSourceAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Data source {id} not found" });
        }
        return Ok(new { message = "Data source disabled successfully" });
    }

    [HttpGet("test-connections")]
    public async Task<ActionResult<Dictionary<Guid, DataSourceConnectionResult>>> TestAllConnections()
    {
        var results = await _dataSourceRegistry.TestAllConnectionsAsync();
        return Ok(results);
    }

    [HttpPost("{id}/sync")]
    public async Task<ActionResult<DataSyncResult>> SyncDataSource(Guid id)
    {
        _logger.LogInformation("Starting sync for data source {DataSourceId}", id);
        var result = await _aggregationService.SyncDataSourceAsync(id);
        
        if (!result.IsSuccessful)
        {
            _logger.LogError("Sync failed for data source {DataSourceId}: {Error}", id, result.ErrorMessage);
            return BadRequest(result);
        }

        _logger.LogInformation("Sync completed for data source {DataSourceId}. Customers: {Customers}, Accounts: {Accounts}, Transactions: {Transactions}",
            id, result.CustomersProcessed, result.AccountsProcessed, result.TransactionsProcessed);
        
        return Ok(result);
    }

    [HttpPost("sync-all")]
    public async Task<ActionResult<IEnumerable<DataSyncResult>>> SyncAllDataSources()
    {
        _logger.LogInformation("Starting sync for all data sources");
        var results = await _aggregationService.SyncAllDataSourcesAsync();
        return Ok(results);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<DataAggregationStats>> GetStatistics()
    {
        var stats = await _aggregationService.GetStatisticsAsync();
        return Ok(stats);
    }
}
