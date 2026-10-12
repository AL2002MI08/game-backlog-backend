using GameBacklog.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.Extensions
{
    public static class PaginationExtensions
    {
        public static async Task<PagedResponse<TOut>> ToPagedResponseAsync<TIn, TOut>(
            this IQueryable<TIn> query,
            PaginationParameters pagination,
            Func<TIn, TOut> map)
        {
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PagedResponse<TOut>(items.Select(map).ToList(), PaginationMetadata.From(pagination.Page, pagination.PageSize, totalCount));
        }
    }
}
