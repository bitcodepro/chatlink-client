namespace ChatLink.Client.Models;

public class UserSession
{
    public required Session Session { get; set; }

    public required User User { get; set; }
}
