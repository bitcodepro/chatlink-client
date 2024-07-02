using ChatLink.Client.Constants;
using ChatLink.Client.Models.Dtos;
using ChatLink.Client.Providers.Handlers;
using ChatLink.Client.Services.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatLink.Client.Services;

public class SignalRService : ISignalRService
{
    private readonly ContactHandler _contactHandler;
    private readonly ChatsHandler _chatsHandler;
    private readonly ChatHandler _chatHandler;
    private HubConnection? Connection { get; set; }
    private string targetUserEmail;

    public SignalRService(ContactHandler contactHandler, ChatsHandler chatsHandler, ChatHandler chatHandler)
    {
        _contactHandler = contactHandler;
        _chatsHandler = chatsHandler;
        _chatHandler = chatHandler;
    }
    
    public async Task Send(string methodName, string email, MessageTinyDto data)
    {
        await Connection.InvokeAsync(methodName, email, data);
    }

    public async Task Send(string methodName, params string[] data)
    {
        if (data.Length == 1)
        {
            await Connection.InvokeAsync(methodName, data[0]);
        }
        else if (data.Length == 2)
        {
            await Connection.InvokeAsync(methodName, data[0], data[1]);
        }
        else if (data.Length == 3)
        {
            await Connection.InvokeAsync(methodName, data[0], data[1], data[2]);
        }
        else
        {
            throw new ArgumentException("Invalid number of arguments");
        }
    }

    public async Task Start(string jwtToken, Action<string, string> receivePublicKey)
    {
        try
        {
            if (Connection == null || Connection.State == HubConnectionState.Disconnected)
            {
                var url = GetUrl() + "/api/signalr";

                Connection = new HubConnectionBuilder()
                    .WithAutomaticReconnect()
                    .WithUrl(url, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(jwtToken)!;
                    })
                    .Build();

                 Connection.On<string, string>(SignalRConstants.ReceivePublicKey, receivePublicKey);

                 Connection.On(SignalRConstants.UpdateContacts, _contactHandler.UpdateContacts);

                 Connection.On(SignalRConstants.UpdateChats, _chatsHandler.UpdateChats);

                 Connection.On<string, string>(SignalRConstants.UpdateChat, _chatHandler.UpdateChat);

                 Connection.On<string>(SignalRConstants.ReceiveTheCall, _chatHandler.ReceiveTheCall);

                 Connection.On(SignalRConstants.FinishCall, _chatHandler.FinishCall);
            }

            if (Connection.State == HubConnectionState.Disconnected)
            {
                await Connection.StartAsync();
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task Stop()
    {
        if (Connection is null || Connection.State != HubConnectionState.Connected)
        {
            return;
        }

        await Connection.StopAsync();
    }

    public static string GetUrl()
    {
        string url = "http://localhost:40000";

#if __ANDROID__

        url = "http://10.0.2.2:40000";
#endif

        return url;
    }
}
