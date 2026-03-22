using FlexiBoard.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using FlexiBoard.API.Hubs;

namespace FlexiBoard.API.BackgroundServices;

/// <summary>
/// Background service that polls the external API every 5 seconds
/// and broadcasts updates via SignalR
/// </summary>
public class DashboardRefreshService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DashboardRefreshService> _logger;
    private const int RefreshIntervalSeconds = 5;

    public DashboardRefreshService(IServiceProvider serviceProvider, ILogger<DashboardRefreshService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dashboard refresh service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DashboardHub>>();

                    // Refresh dashboard data
                    await dashboardService.RefreshDashboardDataAsync();

                    // Get updated data and broadcast
                    var data = await dashboardService.GetDashboardDataAsync();
                    await hubContext.Clients.All.SendAsync("ReceiveDashboardUpdate", data, stoppingToken);

                    _logger.LogInformation("Dashboard data refreshed and broadcasted");
                }

                await Task.Delay(TimeSpan.FromSeconds(RefreshIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Dashboard refresh service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in dashboard refresh service");
                // Wait before retrying on error
                await Task.Delay(TimeSpan.FromSeconds(RefreshIntervalSeconds), stoppingToken);
            }
        }

        _logger.LogInformation("Dashboard refresh service stopped");
    }
}
