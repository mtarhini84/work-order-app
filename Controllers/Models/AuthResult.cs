namespace WorkOrderApp.Controllers
{
    public class AuthResult
    {
        public string Token { get; set; } = string.Empty;
        public object Data { get; set; } = new();
    }
}
