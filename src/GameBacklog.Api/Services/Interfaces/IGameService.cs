using GameBacklog.Api.Dtos;

namespace GameBacklog.Api.Services.Interfaces {
    public interface IGameService
    {
        Task<GameResponse> CreateGameAsync(Guid userId, CreateGameRequest request);
        Task<List<GameResponse>> GetGamesAsync(Guid userId);
        Task<GameResponse> GetGameAsync(Guid userId, Guid gameId);
        Task<GameResponse> UpdateGameAsync(Guid userId, Guid gameId, UpdateGameRequest request);
        Task DeleteGameAsync(Guid userId, Guid gameId);
    }
}
