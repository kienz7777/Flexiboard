using FlexiBoard.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlexiBoard.Infrastructure.DataConnectors;

/// <summary>
/// CSV file data source connector
/// Reads and parses CSV files (local or remote)
/// </summary>
public class CsvConnector : IDataSourceConnector
{
    private readonly ILogger<CsvConnector> _logger;
    private DataSourceConfiguration? _configuration;
    private List<Dictionary<string, object>>? _data;

    public string Id { get; }
    public string Name { get; private set; } = "CSV Connector";
    public string ConnectorType => "CSV";
    public bool IsActive { get; private set; }

    public CsvConnector(ILogger<CsvConnector> logger, string id)
    {
        _logger = logger;
        Id = id;
    }

    public async Task<bool> ConnectAsync(DataSourceConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _configuration = configuration;
            Name = configuration.ConnectionString;

            var testResult = await TestConnectionAsync(cancellationToken);
            IsActive = testResult.IsSuccessful;

            if (IsActive)
            {
                // Parse the CSV on connect
                await ParseCsvAsync(cancellationToken);
            }

            _logger.LogInformation("CSV connector {Id} connected to {Path}", Id, configuration.ConnectionString);
            return IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect CSV connector {Id}", Id);
            IsActive = false;
            return false;
        }
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_configuration == null)
            return new ConnectionTestResult { IsSuccessful = false, ErrorMessage = "Not configured" };

        try
        {
            var startTime = DateTime.UtcNow;

            string csvContent;

            // Check if it's a URL or file path
            if (_configuration.ConnectionString.StartsWith("http"))
            {
                using var client = new HttpClient();
                csvContent = await client.GetStringAsync(_configuration.ConnectionString, cancellationToken);
            }
            else
            {
                // Local file
                if (!File.Exists(_configuration.ConnectionString))
                    return new ConnectionTestResult { IsSuccessful = false, ErrorMessage = "File not found" };

                csvContent = await File.ReadAllTextAsync(_configuration.ConnectionString, cancellationToken);
            }

            var responseTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            return new ConnectionTestResult
            {
                IsSuccessful = lines.Length > 1,
                ResponseTimeMs = responseTime,
                SampleData = new { LineCount = lines.Length, FirstLine = lines.FirstOrDefault() }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CSV connection test failed for {Path}", _configuration.ConnectionString);
            return new ConnectionTestResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<object> FetchDataAsync(string query = "", CancellationToken cancellationToken = default)
    {
        if (_data == null || !IsActive)
            throw new InvalidOperationException("Connector not connected or data not loaded");

        // Apply simple filtering if query is provided
        var result = _data;

        if (!string.IsNullOrEmpty(query))
        {
            // Very basic query support - could be enhanced for SQL-like syntax
            // For now, just limit results
            if (query.Contains("limit"))
            {
                var limit = int.Parse(query.Split("limit")[1].Trim());
                result = _data.Take(limit).ToList();
            }
        }

        return Task.FromResult<object>(new { Data = result, Count = result.Count });
    }

    public Task<List<FieldDefinition>> GetAvailableFieldsAsync(CancellationToken cancellationToken = default)
    {
        var fields = new List<FieldDefinition>();

        if (_data?.Count > 0)
        {
            var firstRow = _data[0];
            foreach (var kvp in firstRow)
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
        _data = null;
        _logger.LogInformation("CSV connector {Id} disconnected", Id);
        return Task.CompletedTask;
    }

    private async Task ParseCsvAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_configuration == null) return;

            string csvContent;

            if (_configuration.ConnectionString.StartsWith("http"))
            {
                using var client = new HttpClient();
                csvContent = await client.GetStringAsync(_configuration.ConnectionString, cancellationToken);
            }
            else
            {
                csvContent = await File.ReadAllTextAsync(_configuration.ConnectionString, cancellationToken);
            }

            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return;

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
            _data = new List<Dictionary<string, object>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',').Select(v => v.Trim()).ToList();
                var row = new Dictionary<string, object>();

                for (int j = 0; j < headers.Count && j < values.Count; j++)
                {
                    object value = values[j];

                    // Try to parse as number
                    if (double.TryParse(values[j], out var number))
                        value = number;

                    row[headers[j]] = value;
                }

                _data.Add(row);
            }

            _logger.LogInformation("Parsed CSV with {RowCount} rows", _data.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse CSV file");
            throw;
        }
    }
}
