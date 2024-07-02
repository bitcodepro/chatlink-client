using SQLite;

namespace ChatLink.Client.Models;

[Table("session")]
public class Session
{
    [PrimaryKey]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("createdDt")]
    public DateTime CreatedDt { get; set; } = DateTime.Now;

    [Column("email")]
    public string Email { get; set; } = string.Empty;
}
