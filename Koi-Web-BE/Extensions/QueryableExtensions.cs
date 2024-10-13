using Koi_Web_BE.Common.Constants;
using Koi_Web_BE.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Koi_Web_BE.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> Paginate<TEntity>(this IQueryable<TEntity> items, int page, int size)
    {
        return items.Skip((page - 1) * size).Take(size);
    }
        
    public static async Task<PaginatedList<TEntityDto>> ListPaginateWithOrderAsync<TEntity, TEntityDto>(
        this IQueryable<TEntity> items,
        int? page,
        int? size,
        Expression<Func<TEntity, object>> orderPredicate,
        string? sortOrder,
        Func<TEntity, TEntityDto> entityMapper)
    {
        var pageNumber = page is null or <= 0 ? 1 : page;
        var sizeNumber = size is null or <= 0 ? 10 : size;
        
        var count = await items.CountAsync();

        items = sortOrder is not null && sortOrder == QueryParameters.Sort.Descending
            ? items.OrderByDescending(orderPredicate)
            : items.OrderBy(orderPredicate);

        var list = await items
            .Paginate(pageNumber.Value, sizeNumber.Value)
            .ToListAsync();

        var result = new List<TEntityDto>();
        result.AddRange(list.Select(entityMapper));
        return new PaginatedList<TEntityDto>(result, count, pageNumber.Value, sizeNumber.Value);
    }

}