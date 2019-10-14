using Newtonsoft.Json;

namespace Tam.Maestro.Common.Clients
{
    public class CognitoTokenResponse
    {
        /// <summary>
        /// Gets or sets access token.
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets expires in duration in seconds.
        /// </summary>
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets token type.
        /// </summary>
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }
    }
}