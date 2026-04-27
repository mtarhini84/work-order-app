namespace WorkOrderApp.Controllers
{
    public class ApiResultModel
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
