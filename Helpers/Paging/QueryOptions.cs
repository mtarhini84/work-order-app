namespace WorkOrderApp.Helpers.Paging
{
    public class QueryOptions
    {
        private const int _maxPageSize = 100;
        private int _pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > _maxPageSize) ? _maxPageSize : value;
        }
        public Dictionary<string, string[]> Filters { get; set; }
            = new Dictionary<string, string[]>();
        public string? SearchTerm { get; set; }
        public string[] SearchColumns { get; set; }
            = Array.Empty<string>();
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}