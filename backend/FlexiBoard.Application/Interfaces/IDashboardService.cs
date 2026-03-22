using FlexiBoard.Domain.Entities;

namespace FlexiBoard.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardData> GetDashboardDataAsync();
    Task RefreshDashboardDataAsync();
}
