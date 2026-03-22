using FlexiBoard.Domain.Entities;
using FlexiBoard.Application.Interfaces;
using System.Net.Http.Json;
using Polly;
using Polly.CircuitBreaker;
using Microsoft.Extensions.Logging;

namespace FlexiBoard.Infrastructure.ExternalAPIs;

public class FakeStoreApiClient : IFakeStoreApiClient
{
    private const string BaseUrl = "https://fakestoreapi.com";
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _resilientPolicy;
    private readonly ILogger<FakeStoreApiClient> _logger;

    public FakeStoreApiClient(HttpClient httpClient, ILogger<FakeStoreApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _resilientPolicy = CreateResiliencePolicy();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching products from FakeStore API");
            var response = await _resilientPolicy.ExecuteAsync(() =>
                _httpClient.GetAsync($"{BaseUrl}/products"));

            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var products = System.Text.Json.JsonSerializer.Deserialize<List<Product>>(jsonString);
            _logger.LogInformation("Successfully fetched {Count} products", products?.Count ?? 0);
            return products ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from FakeStore API");
            throw;
        }
    }

    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            _logger.LogInformation("Fetching users from FakeStore API");
            var response = await _resilientPolicy.ExecuteAsync(() =>
                _httpClient.GetAsync($"{BaseUrl}/users"));

            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var users = System.Text.Json.JsonSerializer.Deserialize<List<User>>(jsonString);
            _logger.LogInformation("Successfully fetched {Count} users", users?.Count ?? 0);
            return users ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users from FakeStore API");
            throw;
        }
    }

    private IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy()
    {
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry attempt {RetryCount} after {DelaySeconds} seconds",
                        retryCount, timespan.TotalSeconds);
                });

        var circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .CircuitBreakerAsync<HttpResponseMessage>(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30));

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }
}
