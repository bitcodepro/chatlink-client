using ChatLink.Client.Models.Dtos;

namespace ChatLink.Client.Services.Interfaces;

public interface ISignalRService
{
    public Task Start(string jwtToken, Action<string, string> receivePublicKey);
    public Task Stop();

    public Task Send(string methodName, params string[] data);
    public Task Send(string methodName, string email, MessageTinyDto data);
}
