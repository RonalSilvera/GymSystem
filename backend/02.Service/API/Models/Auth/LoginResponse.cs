namespace API.Models.Auth;

public class LoginResponse
{
    public string Token { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }

    public LoginResponse(string token, DateTime expiresAtUtc, string role, string email)
    {
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        Role = role;
        Email = email;
    }
}
