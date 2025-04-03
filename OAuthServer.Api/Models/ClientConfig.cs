namespace OAuthServer.Api.Models
{
    public class ClientConfig
    {
        public string ClientId { get; set; }
        public List<string> AllowedGrantTypes { get; set; }
        public List<string> ClientSecrets { get; set; }
        public List<string> AllowedScopes { get; set; }
    }
}
