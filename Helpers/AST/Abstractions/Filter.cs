namespace WorkOrderApp.Helpers.AST
{
    public class Filter
    {
        public string FieldName { get; set; }
        public FilterOperation Operation { get; set; }
        public object Value { get; set; }
    }
}
