using FlexiBoard.Application.Interfaces;
using FlexiBoard.Infrastructure.DataConnectors;
using Microsoft.Extensions.Logging;

namespace FlexiBoard.Infrastructure.Services;

/// <summary>
/// Manages multiple data source connectors
/// Handles registration, lifecycle, and access to different data sources
/// </summary>
public class DataSourceManager : IDataSourceManager
{
    private readonly Dictionary<string, IDataSourceConnector> _connectors = new();
    private readonly Dictionary<string, DataSourceConfiguration> _configurations = new();
    private readonly HttpClient _httpClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DataSourceManager> _logger;

    public DataSourceManager(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<DataSourceManager>();
    }

    public async Task<string> RegisterDataSourceAsync(DataSourceConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test the connection first
            var testResult = await TestDataSourceAsync(config, cancellationToken);
            if (!testResult.IsSuccessful)
                throw new InvalidOperationException($"Connection test failed: {testResult.ErrorMessage}");

            var id = Guid.NewGuid().ToString();
            var connector = CreateConnector(config.ConnectorType, id);

            if (await connector.ConnectAsync(config, cancellationToken))
            {
                _connectors[id] = connector;
                _configurations[id] = config;
                _logger.LogInformation("Registered data source {Id} of type {Type}", id, config.ConnectorType);
                return id;
            }

            throw new InvalidOperationException("Failed to connect to data source");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register data source");
            throw;
        }
    }

    public Task<IDataSourceConnector?> GetConnectorAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        _connectors.TryGetValue(dataSourceId, out var connector);
        return Task.FromResult(connector);
    }

    public Task<List<DataSourceInfo>> ListDataSourcesAsync(CancellationToken cancellationToken = default)
    {
        var sources = _connectors.Select(kvp =>
        {
            var connector = kvp.Value;
            _configurations.TryGetValue(kvp.Key, out var config);

            return new DataSourceInfo
            {
                Id = kvp.Key,
                Name = connector.Name,
                ConnectorType = connector.ConnectorType,
                IsActive = connector.IsActive,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };
        }).ToList();

        return Task.FromResult(sources);
    }

    public async Task<bool> UpdateDataSourceAsync(string dataSourceId, DataSourceConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connectors.TryGetValue(dataSourceId, out var connector))
                return false;

            // Test new configuration
            var testResult = await TestDataSourceAsync(config, cancellationToken);
            if (!testResult.IsSuccessful)
                return false;

            // Disconnect old
            await connector.DisconnectAsync();

            // Connect new
            var connected = await connector.ConnectAsync(config, cancellationToken);
            if (connected)
            {
                _configurations[dataSourceId] = config;
                _logger.LogInformation("Updated data source {Id}", dataSourceId);
            }

            return connected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update data source {Id}", dataSourceId);
            return false;
        }
    }

    public async Task<bool> DeleteDataSourceAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connectors.TryGetValue(dataSourceId, out var connector))
            {
                await connector.DisconnectAsync();
                _connectors.Remove(dataSourceId);
                _configurations.Remove(dataSourceId);
                _logger.LogInformation("Deleted data source {Id}", dataSourceId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete data source {Id}", dataSourceId);
            return false;
        }
    }

    public async Task<ConnectionTestResult> TestDataSourceAsync(DataSourceConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            var tempConnector = CreateConnector(config.ConnectorType, "test-" + Guid.NewGuid());
            
            // For testing, we don't actually connect, just test the connection
            if (tempConnector is RestApiConnector restConnector)
            {
                await restConnector.ConnectAsync(config, cancellationToken);
                var result = await restConnector.TestConnectionAsync(cancellationToken);
                await restConnector.DisconnectAsync();
                return result;
            }
            else if (tempConnector is CsvConnector csvConnector)
            {
                var result = await csvConnector.TestConnectionAsync(cancellationToken);
                return result;
            }
            else if (tempConnector is WebhookConnector webhookConnector)
            {
                var result = await webhookConnector.TestConnectionAsync(cancellationToken);
                return result;
            }

            return new ConnectionTestResult { IsSuccessful = false, ErrorMessage = "Unknown connector type" };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Data source test failed");
            return new ConnectionTestResult { IsSuccessful = false, ErrorMessage = ex.Message };
        }
    }

    private IDataSourceConnector CreateConnector(string connectorType, string id)
    {
        return connectorType.ToLower() switch
        {
            "rest" => new RestApiConnector(_httpClient, _loggerFactory.CreateLogger<RestApiConnector>(), id),
            "csv" => new CsvConnector(_loggerFactory.CreateLogger<CsvConnector>(), id),
            "webhook" => new WebhookConnector(_loggerFactory.CreateLogger<WebhookConnector>(), id),
            _ => throw new InvalidOperationException($"Unknown connector type: {connectorType}")
        };
    }
}
