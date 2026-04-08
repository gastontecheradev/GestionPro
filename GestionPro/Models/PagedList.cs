using Microsoft.EntityFrameworkCore;

namespace GestionPro.Models
{
    /// <summary>
    /// Lista paginada genérica. Reemplaza X.PagedList para evitar
    /// incompatibilidades de versión. Funciona con cualquier IQueryable.
    /// </summary>
    public class PagedList<T> : List<T>
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalItemCount { get; }
        public int PageCount { get; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < PageCount;

        public int FirstItemOnPage => (PageNumber - 1) * PageSize + 1;
        public int LastItemOnPage => Math.Min(PageNumber * PageSize, TotalItemCount);

        public PagedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItemCount = totalCount;
            PageCount = (int)Math.Ceiling(totalCount / (double)pageSize);
            AddRange(items);
        }

        /// <summary>
        /// Crea una lista paginada desde un IQueryable (ejecuta COUNT + SKIP/TAKE en SQL).
        /// </summary>
        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
