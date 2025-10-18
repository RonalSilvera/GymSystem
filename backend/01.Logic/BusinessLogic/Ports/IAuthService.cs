namespace BusinessLogic.Ports;

public interface IAuthService
{
    Task<string?> AuthenticateAsync(string email, string password);
    Task LogoutAsync();
}
