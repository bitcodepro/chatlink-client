using SQLite;

namespace ChatLink.Client.Models;

[Table("user")]
public class User
{
    [PrimaryKey]
    [Column("email")]
    public string Email { get; set; }

    [Column("userName")]
    public string UserName { get; set; }

    [Column("imageUrl")]
    public string? ImageUrl { get; set; }
}
