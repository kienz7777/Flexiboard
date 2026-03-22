using FlexiBoard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlexiBoard.API.Controllers;

/// <summary>
/// API endpoints for managing data sources
/// Allows users to configure and manage multiple data sources
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DataSourcesController : ControllerBase
{
    private readonly IDataSourceManager _dataSourceManager;
    private readonly ILogger<DataSourcesController> _logger;

    public DataSourcesController(IDataSourceManager dataSourceManager, ILogger<DataSourcesController> logger)
    {
        _dataSourceManager = dataSourceManager;
        _logger = logger;
    }

    /// <summary>
    /// List all registered data sources
    /// GET /api/datasources
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DataSourceInfo>>> GetDataSources(CancellationToken cancellationToken)
    {
        try
        {
            var sources = await _dataSourceManager.ListDataSourcesAsync(cancellationToken);
            return Ok(sources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list data sources");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get details of a specific data source
    /// GET /api/datasources/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetDataSource(string id, CancellationToken cancellationToken)
    {
        try
        {
            var connector = await _dataSourceManager.GetConnectorAsync(id, cancellationToken);
            if (connector == null)
                return NotFound();

            return Ok(new
            {
                Id = id,
                Name = connector.Name,
                Type = connector.ConnectorType,
                IsActive = connector.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data source {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Register a new data source
    /// POST /api/datasources
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<string>> RegisterDataSource(
        [FromBody] DataSourceConfiguration config,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(config.ConnectorType) || string.IsNullOrEmpty(config.ConnectionString))
                return BadRequest(new { error = "ConnectorType and ConnectionString are required" });

            var id = await _dataSourceManager.RegisterDataSourceAsync(config, cancellationToken);
            _logger.LogInformation("Registered data source {Id}", id);

            return CreatedAtAction(nameof(GetDataSource), new { id }, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register data source");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test a data source connection before saving
    /// POST /api/datasources/test
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult<ConnectionTestResult>> TestConnection(
        [FromBody] DataSourceConfiguration config,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(config.ConnectorType) || string.IsNullOrEmpty(config.ConnectionString))
                return BadRequest(new { error = "ConnectorType and ConnectionString are required" });

            var result = await _dataSourceManager.TestDataSourceAsync(config, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test data source");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing data source
    /// PUT /api/datasources/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateDataSource(
        string id,
        [FromBody] DataSourceConfiguration config,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _dataSourceManager.UpdateDataSourceAsync(id, config, cancellationToken);
            if (!success)
                return NotFound();

            _logger.LogInformation("Updated data source {Id}", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update data source {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a data source
    /// DELETE /api/datasources/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDataSource(string id, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _dataSourceManager.DeleteDataSourceAsync(id, cancellationToken);
            if (!success)
                return NotFound();

            _logger.LogInformation("Deleted data source {Id}", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete data source {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Fetch data from a data source
    /// GET /api/datasources/{id}/data
    /// </summary>
    [HttpGet("{id}/data")]
    public async Task<ActionResult<object>> FetchData(
        string id,
        [FromQuery] string query = "",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connector = await _dataSourceManager.GetConnectorAsync(id, cancellationToken);
            if (connector == null)
                return NotFound();

            if (!connector.IsActive)
                return BadRequest(new { error = "Data source is not active" });

            var data = await connector.FetchDataAsync(query, cancellationToken);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch data from source {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get available fields from a data source
    /// GET /api/datasources/{id}/fields
    /// </summary>
    [HttpGet("{id}/fields")]
    public async Task<ActionResult<List<FieldDefinition>>> GetFields(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var connector = await _dataSourceManager.GetConnectorAsync(id, cancellationToken);
            if (connector == null)
                return NotFound();

            if (!connector.IsActive)
                return BadRequest(new { error = "Data source is not active" });

            var fields = await connector.GetAvailableFieldsAsync(cancellationToken);
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get fields from source {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Receive webhook data
    /// POST /api/datasources/{id}/webhook
    /// </summary>
    [HttpPost("{id}/webhook")]
    public async Task<ActionResult> ReceiveWebhookData(
        string id,
        [FromBody] object data,
        CancellationToken cancellationToken)
    {
        try
        {
            var connector = await _dataSourceManager.GetConnectorAsync(id, cancellationToken);
            if (connector == null)
                return NotFound();

            if (connector is not WebhookConnector webhookConnector)
                return BadRequest(new { error = "Data source is not a webhook connector" });

            webhookConnector.ReceiveWebhookData(data);
            _logger.LogInformation("Received webhook data for source {Id}", id);

            return Ok(new { status = "received" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive webhook data");
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Re-export types for API usage
public class WebhookConnector : FlexiBoard.Infrastructure.DataConnectors.WebhookConnector
{
    public WebhookConnector(ILogger<FlexiBoard.Infrastructure.DataConnectors.WebhookConnector> logger, string id)
        : base(logger, id) { }
}
