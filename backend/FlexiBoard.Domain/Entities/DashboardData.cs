namespace FlexiBoard.Domain.Entities;

public class DashboardData
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalUsers { get; set; }
    public List<RevenueTrend> RevenueTrends { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
