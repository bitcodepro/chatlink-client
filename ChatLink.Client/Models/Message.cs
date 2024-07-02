using SQLite;

namespace ChatLink.Client.Models;

[Table("message")]
public class Message
{
    [PrimaryKey]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("email")]
    public string Email { get; set; }

    [Column("encryptedMessage")]
    public string EncryptedMessage { get; set; } = string.Empty;

    [Column("decryptedMessage")]
    public string DecryptedMessage { get; set; } = string.Empty;

    [Column("type")]
    public string Type { get; set; }

    [Column("sessionId")]
    public Guid SessionId { get; set; }

    [Column("createdDt")]
    public DateTime CreatedDt { get; set; }
}
