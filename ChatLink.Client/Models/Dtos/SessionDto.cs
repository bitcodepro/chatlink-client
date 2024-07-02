namespace ChatLink.Client.Models.Dtos;

public class SessionDto
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Email { get; set; }

    public required string UserName { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string LastMessage { get; set; } = string.Empty;

    public DateTime MessageCreatedDt { get; set; } = DateTime.UtcNow;
}
