namespace WorkOrderApp.Helpers.AST
{
    public class FilterGroup
    {
        public List<Filter> Filters { get; set; } = new List<Filter>();
        public LogicalOperation Operation { get; set; }
    }
}
