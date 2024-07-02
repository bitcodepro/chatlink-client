using ChatLink.Client.Models.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatLink.Client.Models.Dtos;

public partial class MessageDto : ObservableObject
{
    public Guid MessageId { get; set; }

    public MessageType Type { get; set; }

    public required string Email { get; set; }

    public required string UserName { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string EncryptedMessage { get; set; } = string.Empty;
    public string DecryptedMessage { get; set; } = string.Empty;

    public DateTime MessageCreatedDt { get; set; } = DateTime.UtcNow;

    public Guid SessionId { get; set; }

    public bool IsCurrentUser { get; set; }

    public bool IsLastMessage { get; set; }

    [ObservableProperty] 
    private double progress;

    [ObservableProperty]
    private bool isPlay;

    [ObservableProperty]
    private bool isPlaying;
}
