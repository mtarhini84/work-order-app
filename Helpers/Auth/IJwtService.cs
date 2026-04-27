namespace WorkOrderApp.Helpers.Auth
{
    public interface IJwtService
    {
        string GenerateToken(string id, string name, string email, string role);
    }
}
