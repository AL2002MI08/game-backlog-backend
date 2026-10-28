using GameBacklog.Api.Dtos;

namespace GameBacklog.Api.Services.Interfaces {
    public interface IGameService
    {
        Task<GameDetailResponse> CreateGameAsync(Guid userId, CreateGameRequest request);
        Task<PagedResponse<GameListResponse>> GetGamesAsync(Guid userId, GameQueryParameters query);
        Task<GameDetailResponse> GetGameAsync(Guid userId, Guid gameId);
        Task<GameResponse> UpdateGameAsync(Guid userId, Guid gameId, UpdateGameRequest request);
        Task DeleteGameAsync(Guid userId, Guid gameId);
        Task<UpdateGameStatusResponse> UpdateGameStatusAsync(Guid userId, Guid gameId, UpdateGameStatusRequest request);
        Task<CompletionProgressResponse> GetGameProgressAsync(Guid userId, Guid gameId);
        Task<PagedResponse<GameStatusHistoryResponse>> GetGameHistoryAsync(Guid userId, Guid gameId, PaginationParameters pagination);
    }
}
