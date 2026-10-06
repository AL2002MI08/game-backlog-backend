using GameBacklog.Api.Dtos;

namespace GameBacklog.Api.Services.Interfaces {
    public interface IObjectiveService
    {
        Task<ObjectiveResponse> CreateObjectiveAsync(Guid userId, Guid gameId, CreateObjectiveRequest request);
        Task<List<ObjectiveResponse>> GetObjectivesAsync(Guid userId, Guid gameId);
        Task<ObjectiveResponse> UpdateObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId, UpdateObjectiveRequest request);
        Task DeleteObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId);
    }
}
