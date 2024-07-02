using Newtonsoft.Json;

namespace ChatLink.Client.Models.Dtos;

public class JwtDto
{
    [JsonProperty("access_token")] 
    public string AccessToken { get; set; } = string.Empty;
}
