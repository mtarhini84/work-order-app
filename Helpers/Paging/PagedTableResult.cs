namespace WorkOrderApp.Helpers.Paging
{
    public class PagedTableResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public bool HasNext { get; }
        public bool HasPrevious => PageNumber > 1;
        public string? ContinuationToken { get; }
        public int ItemCount => Items.Count;

        public PagedTableResult(
            IReadOnlyList<T> items,
            int pageNumber,
            int pageSize,
            bool hasNext = false,
            string? continuationToken = null)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            HasNext = hasNext;
            ContinuationToken = continuationToken;
        }
    }
}