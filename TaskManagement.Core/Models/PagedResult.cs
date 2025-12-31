namespace TaskManagement.Core.Models
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public PaginationMetadata Pagination { get; set; } = null!;

        public PagedResult() { }

        public PagedResult(List<T> data, int totalItems, int currentPage, int pageSize)
        {
            Data = data;
            Pagination = new PaginationMetadata
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                HasNextPage = currentPage < (int)Math.Ceiling(totalItems / (double)pageSize),
                HasPreviousPage = currentPage > 1
            };
        }
    }

    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
