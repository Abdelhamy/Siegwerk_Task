namespace Pricing.Application.Common.Models;

public record PaginationRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public const int MaxPageSize = 100;

    public PaginationRequest Normalize()
    {
        var normalizedPage = Math.Max(1, Page);
        var normalizedPageSize = Math.Min(Math.Max(1, PageSize), MaxPageSize);

        return this with 
        { 
            Page = normalizedPage, 
            PageSize = normalizedPageSize 
        };
    }

    public int Skip => (Page - 1) * PageSize;

    public int Take => PageSize;
}

public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }

    public bool HasPreviousPage { get; init; }

    public bool HasNextPage { get; init; }

    public int Count => Items.Count;

    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };
    }

    public static PagedResult<T> Empty(int page = 1, int pageSize = 10)
    {
        return Create(Array.Empty<T>(), 0, page, pageSize);
    }
}

public record SortRequest
{
    public string? SortBy { get; init; }

    public string SortDirection { get; init; } = "asc";

    public bool IsDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

    public bool IsValid(params string[] allowedSortFields)
    {
        if (string.IsNullOrWhiteSpace(SortBy))
            return true;

        return allowedSortFields.Contains(SortBy, StringComparer.OrdinalIgnoreCase);
    }
}