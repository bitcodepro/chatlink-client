namespace ChatLink.Client.Services.Interfaces;

public interface IApiService
{
    Task<TResponse?> GetAsync<TResponse>(string uri, Dictionary<string, string>? headers = null);
    Task PostAsync<TRequest>(string uri, TRequest data, Dictionary<string, string>? headers = null);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data, Dictionary<string, string>? headers = null);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data, Dictionary<string, string>? headers = null);
    Task DeleteAsync(string uri, Dictionary<string, string>? headers = null);

    Task<byte[]> GetBinaryData(string url, string jwtToken);
}
