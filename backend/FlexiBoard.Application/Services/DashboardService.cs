using FlexiBoard.Domain.Entities;
using FlexiBoard.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FlexiBoard.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IFakeStoreApiClient _apiClient;
    private readonly IOrderGenerator _orderGenerator;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardService> _logger;
    private const string CacheKey = "DashboardData";
    private const int CacheDurationSeconds = 30;

    public DashboardService(
        IFakeStoreApiClient apiClient,
        IOrderGenerator orderGenerator,
        IMemoryCache cache,
        ILogger<DashboardService> logger)
    {
        _apiClient = apiClient;
        _orderGenerator = orderGenerator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        if (_cache.TryGetValue(CacheKey, out DashboardData? cachedData) && cachedData != null)
        {
            _logger.LogInformation("Dashboard data retrieved from cache");
            return cachedData;
        }

        _logger.LogWarning("Dashboard data not found in cache, refreshing now");
        // If cache is empty, refresh immediately instead of returning empty data
        await RefreshDashboardDataAsync();
        
        // Try to get the freshly cached data
        if (_cache.TryGetValue(CacheKey, out DashboardData? freshData) && freshData != null)
        {
            return freshData;
        }

        // Fallback to empty data if refresh failed
        return new DashboardData { LastUpdated = DateTime.UtcNow };
    }

    public async Task RefreshDashboardDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting dashboard data refresh");

            var products = await _apiClient.GetProductsAsync();
            var users = await _apiClient.GetUsersAsync();
            var orders = await _orderGenerator.GenerateOrdersAsync(products, users);

            var dashboardData = new DashboardData
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.Total),
                TotalUsers = users.Count,
                RevenueTrends = CalculateRevenueTrends(orders),
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(CacheKey, dashboardData, TimeSpan.FromSeconds(CacheDurationSeconds));
            _logger.LogInformation("Dashboard data refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing dashboard data");
            throw;
        }
    }

    private List<RevenueTrend> CalculateRevenueTrends(List<Order> orders)
    {
        return orders
            .GroupBy(o => o.Date.Date)
            .OrderBy(g => g.Key)
            .Select(g => new RevenueTrend
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.Total),
                OrderCount = g.Count()
            })
            .ToList();
    }
}
