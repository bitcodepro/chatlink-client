namespace ChatLink.Client.Models;

public class AuthUser
{
    public required string Server { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
