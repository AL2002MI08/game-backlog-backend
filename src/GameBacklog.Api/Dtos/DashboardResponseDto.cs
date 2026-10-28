namespace GameBacklog.Api.Dtos
{
    public record DashboardResponse(
        int TotalGames,
        Dictionary<string, int> GamesByStatus,
        int FullyCompleteGames,
        int GamesWithNoObjectives,
        double? AverageRating,
        List<GameListResponse> RecentlyUpdatedGames);
}
