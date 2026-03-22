using FlexiBoard.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace FlexiBoard.API.Hubs;

public class DashboardHub : Hub
{
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(ILogger<DashboardHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcast dashboard data to all connected clients
    /// </summary>
    public async Task BroadcastDashboardData(DashboardData data)
    {
        try
        {
            _logger.LogInformation("Broadcasting dashboard data to all clients");
            await Clients.All.SendAsync("ReceiveDashboardUpdate", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting dashboard data");
        }
    }
}
