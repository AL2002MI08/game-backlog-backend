using GameBacklog.Api.Dtos;

namespace GameBacklog.Api.Services.Interfaces {
    public interface IDashboardService
    {
        Task<DashboardResponse> GetDashboardAsync(Guid userId);
    }
}
