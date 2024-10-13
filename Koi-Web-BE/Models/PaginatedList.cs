using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; } = new();
    public int? PageNumber { get; set; }
    public int? TotalPages { get; set; }
    public int TotalCount { get; set; }

    protected PaginatedList()
    {
    }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }
    
    public PaginatedList(List<T> items, int count)
    {
        PageNumber = null;
        TotalPages = null;
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var query = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var items = await query.ToListAsync();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}