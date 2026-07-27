namespace EduApoyos.Application.Common.Pagination;

/// <summary>
/// Generic envelope for paginated query results. Used by every list endpoint so the API
/// contract stays uniform across features (US-011 and future stories).
/// </summary>
/// <typeparam name="T">Type of the projected item.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages)
{
    /// <summary>
    /// Builds an empty page while keeping the pagination metadata consistent.
    /// </summary>
    public static PagedResult<T> Empty(int page, int pageSize) =>
        new(Array.Empty<T>(), page, pageSize, 0, 0);

    /// <summary>
    /// Convenience factory that computes <see cref="TotalPages"/> from
    /// <paramref name="totalItems"/> and <paramref name="pageSize"/>.
    /// </summary>
    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        int totalItems)
    {
        var totalPages = pageSize <= 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<T>(items, page, pageSize, totalItems, totalPages);
    }
}
