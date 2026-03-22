using FlexiBoard.Domain.Entities;

namespace FlexiBoard.Application.Interfaces;

/// <summary>
/// Defines the contract for pluggable data source connectors.
/// Allows FlexiBoard to connect to any data source: REST API, Database, CSV, Webhooks, etc.
/// </summary>
public interface IDataSourceConnector
{
    /// <summary>
    /// Unique identifier for this data source connector instance
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable name of the connector
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Type of connector (REST, Database, CSV, Webhook, etc.)
    /// </summary>
    string ConnectorType { get; }

    /// <summary>
    /// Whether this connector is currently active and ready to fetch data
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Connect to the data source with the provided configuration
    /// </summary>
    /// <param name="configuration">Connection details (API URL, connection string, file path, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection successful</returns>
    Task<bool> ConnectAsync(DataSourceConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test the connection to verify data source is accessible
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection test result with status and error message if failed</returns>
    Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch data from the configured data source
    /// Returns raw data that can be transformed into dashboard metrics
    /// </summary>
    /// <param name="query">Optional query parameters (filter, limit, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw data from source as dynamic object</returns>
    Task<object> FetchDataAsync(string query = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available fields/columns from the data source
    /// Allows frontend to display selectable fields for dashboard
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available field names and their types</returns>
    Task<List<FieldDefinition>> GetAvailableFieldsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the data source
    /// </summary>
    Task DisconnectAsync();
}

/// <summary>
/// Configuration for a data source connector
/// </summary>
public class DataSourceConfiguration
{
    /// <summary>
    /// Type of connector: "REST", "Database", "CSV", "Webhook"
    /// </summary>
    public string ConnectorType { get; set; } = string.Empty;

    /// <summary>
    /// Connection string or URL (e.g., API endpoint, DB connection string, file path)
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Authentication token if required (Bearer token, API key, etc.)
    /// </summary>
    public string? AuthenticationToken { get; set; }

    /// <summary>
    /// Additional configuration options as key-value pairs
    /// Examples: {"timeout": "30000", "retryCount": "3", "pageSize": "100"}
    /// </summary>
    public Dictionary<string, string> Options { get; set; } = new();

    /// <summary>
    /// How often to refresh data (milliseconds)
    /// </summary>
    public int RefreshIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Query/filter to apply when fetching data
    /// </summary>
    public string? Query { get; set; }
}

/// <summary>
/// Result of a connection test
/// </summary>
public class ConnectionTestResult
{
    /// <summary>
    /// Whether the connection was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if connection failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Sample data from the connection test
    /// </summary>
    public object? SampleData { get; set; }
}

/// <summary>
/// Defines a field available in a data source
/// </summary>
public class FieldDefinition
{
    /// <summary>
    /// Field name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Field data type (string, number, date, boolean)
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Human-readable field label
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Whether this field can be used for aggregation
    /// </summary>
    public bool IsAggregatable { get; set; }
}

/// <summary>
/// Service for managing multiple data source connectors
/// </summary>
public interface IDataSourceManager
{
    /// <summary>
    /// Register a new data source connector
    /// </summary>
    Task<string> RegisterDataSourceAsync(DataSourceConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a connector by ID
    /// </summary>
    Task<IDataSourceConnector?> GetConnectorAsync(string dataSourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all registered data sources
    /// </summary>
    Task<List<DataSourceInfo>> ListDataSourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a connector configuration
    /// </summary>
    Task<bool> UpdateDataSourceAsync(string dataSourceId, DataSourceConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a data source connector
    /// </summary>
    Task<bool> DeleteDataSourceAsync(string dataSourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test connection before saving
    /// </summary>
    Task<ConnectionTestResult> TestDataSourceAsync(DataSourceConfiguration config, CancellationToken cancellationToken = default);
}

/// <summary>
/// Information about a registered data source
/// </summary>
public class DataSourceInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ConnectorType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}
