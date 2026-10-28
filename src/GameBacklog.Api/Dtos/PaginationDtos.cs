using System.ComponentModel.DataAnnotations;

namespace GameBacklog.Api.Dtos
{
    public record PaginationParameters
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int Page { get; init; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; init; } = 10;
    }

    public record PaginationMetadata(int CurrentPage, int PageSize, int TotalCount, int TotalPages)
    {
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public static PaginationMetadata From(int page, int pageSize, int totalCount) =>
            new(page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public record PagedResponse<T>(IReadOnlyList<T> Items, PaginationMetadata Pagination);
}
