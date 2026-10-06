using GameBacklog.Api.Dtos;

namespace GameBacklog.Api.Services.Interfaces {
    public interface IObjectiveService
    {
        Task<bool> GameExistsAsync(Guid userId, Guid gameId);
        Task<ObjectiveResponse?> CreateObjectiveAsync(Guid gameId, CreateObjectiveRequest request);
        Task<List<ObjectiveResponse>> GetObjectivesAsync(Guid gameId);
        Task<ObjectiveResponse?> UpdateObjectiveAsync(Guid gameId, Guid objectiveId, UpdateObjectiveRequest request);
        Task<bool> DeleteObjectiveAsync(Guid gameId, Guid objectiveId);
    }
}
