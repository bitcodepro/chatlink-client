using System.Text;
using ChatLink.Client.Services.Interfaces;
using Newtonsoft.Json;

namespace ChatLink.Client.Services;

public class ApiService(HttpClient httpClient) : IApiService
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string uri, HttpContent? content = null, Dictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(method, uri)
        {
            Content = content
        };

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        var response = await httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new HttpRequestException($"Resource not found: {uri}", null, System.Net.HttpStatusCode.NotFound);
        }

        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<TResponse?> GetAsync<TResponse>(string uri, Dictionary<string, string>? headers = null)
    {
        var response = await SendAsync(HttpMethod.Get, uri, null, headers);
        var jsonString = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<TResponse>(jsonString);
    }

    public async Task PostAsync<TRequest>(string uri, TRequest data, Dictionary<string, string>? headers = null)
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        await SendAsync(HttpMethod.Post, uri, content, headers);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data, Dictionary<string, string>? headers = null)
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await SendAsync(HttpMethod.Post, uri, content, headers);
        var jsonString = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<TResponse>(jsonString);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data, Dictionary<string, string>? headers = null)
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await SendAsync(HttpMethod.Put, uri, content, headers);
        var jsonString = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<TResponse>(jsonString);
    }

    public async Task DeleteAsync(string uri, Dictionary<string, string>? headers = null)
    {
        await SendAsync(HttpMethod.Delete, uri, null, headers);
    }

    public async Task<byte[]> GetBinaryData(string url, string jwtToken)
    {
        if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");
        }

        return await httpClient.GetByteArrayAsync(url);
    }
}
