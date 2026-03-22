using FlexiBoard.Application.Interfaces;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace FlexiBoard.Infrastructure.DataConnectors;

/// <summary>
/// REST API data source connector
/// Fetches data from HTTP/HTTPS endpoints
/// </summary>
public class RestApiConnector : IDataSourceConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestApiConnector> _logger;
    private DataSourceConfiguration? _configuration;
    private IAsyncPolicy<HttpResponseMessage>? _resiliencePolicy;

    public string Id { get; }
    public string Name { get; private set; } = "REST API Connector";
    public string ConnectorType => "REST";
    public bool IsActive { get; private set; }

    public RestApiConnector(HttpClient httpClient, ILogger<RestApiConnector> logger, string id)
    {
        _httpClient = httpClient;
        _logger = logger;
        Id = id;
    }

    public async Task<bool> ConnectAsync(DataSourceConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _configuration = configuration;
            Name = configuration.ConnectionString;

            // Setup resilience policy
            _resiliencePolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)))
                .WrapAsync(
                    HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(30))
                );

            var testResult = await TestConnectionAsync(cancellationToken);
            IsActive = testResult.IsSuccessful;

            _logger.LogInformation("REST API connector {Id} connected to {Url}", Id, configuration.ConnectionString);
            return IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect REST API connector {Id}", Id);
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

            using var request = new HttpRequestMessage(HttpMethod.Get, _configuration.ConnectionString);

            // Add authentication if provided
            if (!string.IsNullOrEmpty(_configuration.AuthenticationToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", _configuration.AuthenticationToken);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                var data = System.Text.Json.JsonSerializer.Deserialize<object>(jsonString);
                return new ConnectionTestResult
                {
                    IsSuccessful = true,
                    ResponseTimeMs = responseTime,
                    SampleData = data
                };
            }
            else
            {
                return new ConnectionTestResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                    ResponseTimeMs = responseTime
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "REST API connection test failed for {Url}", _configuration.ConnectionString);
            return new ConnectionTestResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<object> FetchDataAsync(string query = "", CancellationToken cancellationToken = default)
    {
        if (_configuration == null || !IsActive)
            throw new InvalidOperationException("Connector not connected");

        try
        {
            var url = _configuration.ConnectionString;

            // Append query if provided
            if (!string.IsNullOrEmpty(query))
            {
                url += query.StartsWith("?") ? query : "?" + query;
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(_configuration.AuthenticationToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", _configuration.AuthenticationToken);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = System.Text.Json.JsonSerializer.Deserialize<object>(jsonString);
            _logger.LogInformation("Fetched data from REST API {Url}", url);

            return data ?? new { };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch data from REST API");
            throw;
        }
    }

    public async Task<List<FieldDefinition>> GetAvailableFieldsAsync(CancellationToken cancellationToken = default)
    {
        if (_configuration == null || !IsActive)
            return new List<FieldDefinition>();

        try
        {
            var data = await FetchDataAsync("", cancellationToken);

            // Try to extract fields from the response
            var fields = new List<FieldDefinition>();

            if (data is System.Collections.Generic.IDictionary<string, object> dict)
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

            return fields;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get available fields");
            return new List<FieldDefinition>();
        }
    }

    public Task DisconnectAsync()
    {
        IsActive = false;
        _logger.LogInformation("REST API connector {Id} disconnected", Id);
        return Task.CompletedTask;
    }
}
