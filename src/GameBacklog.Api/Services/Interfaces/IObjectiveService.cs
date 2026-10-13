using GameBacklog.Api.Dtos;

namespace GameBacklog.Api.Services.Interfaces {
    public interface IObjectiveService
    {
        Task<ObjectiveResponse> CreateObjectiveAsync(Guid userId, Guid gameId, CreateObjectiveRequest request);
        Task<List<ObjectiveResponse>> GetObjectivesAsync(Guid userId, Guid gameId);
        Task<ObjectiveResponse> ReplaceObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId, ReplaceObjectiveRequest request);
        Task<ObjectiveResponse> CompleteObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId);
        Task<ObjectiveResponse> ReopenObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId);
        Task<List<ObjectiveResponse>> ReorderObjectivesAsync(Guid userId, Guid gameId, ReorderObjectivesRequest request);
        Task DeleteObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId);
    }
}