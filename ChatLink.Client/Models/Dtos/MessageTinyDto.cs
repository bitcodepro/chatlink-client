using ChatLink.Client.Models.Enums;

namespace ChatLink.Client.Models.Dtos;

public class MessageTinyDto
{
    public Guid SessionId { get; set; }

    public MessageType Type { get; set; }

    public string EncryptedMessage { get; set; } = string.Empty;
}
