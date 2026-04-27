namespace WorkOrderApp.Helpers.Paging
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public IReadOnlyDictionary<string, T>? ItemsDictionary { get; }

        public int TotalCount { get; }
        public int PageNumber { get; }
        public int PageSize { get; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNext => PageNumber < TotalPages;
        public bool HasPrevious => PageNumber > 1;

        public PagedResult(
            IReadOnlyList<T> items,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public PagedResult(
            IReadOnlyDictionary<string, T> items,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            ItemsDictionary = items;
            Items = items.Values.ToList();
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
