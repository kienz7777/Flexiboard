using FlexiBoard.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlexiBoard.Infrastructure.DataConnectors;

/// <summary>
/// Webhook data source connector
/// Receives data pushed from external systems
/// </summary>
public class WebhookConnector : IDataSourceConnector
{
    private readonly ILogger<WebhookConnector> _logger;
    private DataSourceConfiguration? _configuration;
    private object? _lastReceivedData;
    private DateTime _lastDataReceivedAt = DateTime.MinValue;

    public string Id { get; }
    public string Name { get; private set; } = "Webhook Connector";
    public string ConnectorType => "Webhook";
    public bool IsActive { get; private set; }

    public WebhookConnector(ILogger<WebhookConnector> logger, string id)
    {
        _logger = logger;
        Id = id;
    }

    public Task<bool> ConnectAsync(DataSourceConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _configuration = configuration;
            Name = configuration.ConnectionString;
            IsActive = true;

            _logger.LogInformation("Webhook connector {Id} configured with endpoint {Endpoint}", Id, configuration.ConnectionString);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure webhook connector {Id}", Id);
            IsActive = false;
            return Task.FromResult(false);
        }
    }

    public Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        // Webhook connections are tested by receiving data
        if (_configuration == null)
            return Task.FromResult(new ConnectionTestResult { IsSuccessful = false, ErrorMessage = "Not configured" });

        var isHealthy = _lastDataReceivedAt > DateTime.UtcNow.AddMinutes(-5);

        return Task.FromResult(new ConnectionTestResult
        {
            IsSuccessful = isHealthy,
            ResponseTimeMs = 0,
            SampleData = _lastReceivedData,
            ErrorMessage = isHealthy ? null : "No data received in last 5 minutes"
        });
    }

    public Task<object> FetchDataAsync(string query = "", CancellationToken cancellationToken = default)
    {
        if (_lastReceivedData == null)
            return Task.FromResult<object>(new { Status = "Waiting for webhook data" });

        return Task.FromResult(_lastReceivedData);
    }

    public Task<List<FieldDefinition>> GetAvailableFieldsAsync(CancellationToken cancellationToken = default)
    {
        var fields = new List<FieldDefinition>();

        if (_lastReceivedData is System.Collections.Generic.IDictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                fields.Add(new FieldDefinition
                {
                    Name = kvp.Key,
                    Label = kvp.Key,
                    DataType = kvp.Value?.GetType().Name ?? "string",
                    IsAggregatable = kvp.Value is int or long or double or decimal
                });
            }
        }

        return Task.FromResult(fields);
    }

    public Task DisconnectAsync()
    {
        IsActive = false;
        _logger.LogInformation("Webhook connector {Id} disconnected", Id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Receives webhook data from external system
    /// Called by API endpoint when webhook is posted to
    /// </summary>
    public void ReceiveWebhookData(object data)
    {
        _lastReceivedData = data;
        _lastDataReceivedAt = DateTime.UtcNow;
        _logger.LogInformation("Webhook connector {Id} received data at {Time}", Id, _lastDataReceivedAt);
    }
}
