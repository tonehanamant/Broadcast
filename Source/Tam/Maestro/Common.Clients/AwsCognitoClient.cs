using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Tam.Maestro.Common.Clients
{
    public interface IAwsCognitoClient
    {
        AwsToken GetToken(AwsTokenRequest tokenRequest);

        void ClearToken(AwsToken token);
    }

    /// <summary>
    /// TODO: Move this to the Maestro Common.Clients nuget during PRI-17015
    /// </summary>
    public class AwsCognitoClient : WebApiClientBase, IAwsCognitoClient
    {
        private readonly MemoryCache _Cache = MemoryCache.Default;
        private const string TOKEN_KEY = "AccessToken";
        private const string GRANT_TYPE = "grant_type";
        private const string CLIENT_CREDENTIALS_GRANT = "client_credentials";
        private const string CLIENT_ID = "client_id";
        private const string CLIENT_SECRET = "client_secret";

        public AwsToken GetToken(AwsTokenRequest tokenRequest)
        {
            var tokenKey = GetTokenKey(tokenRequest.ClientId);
            var accessToken = _Cache.Get(tokenKey) as string;

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return new AwsToken
                {
                    ClientId = tokenRequest.ClientId,
                    AccessToken = accessToken
                };
            }

            var content = new Dictionary<string, string>
            {
                {GRANT_TYPE, CLIENT_CREDENTIALS_GRANT},
                {CLIENT_ID,  tokenRequest.ClientId},
                {CLIENT_SECRET, tokenRequest.ClientSecret }
            };

            var cognitoResponse = _PostFormData<CognitoTokenResponse>(tokenRequest.TokenUrl, content);
            accessToken = cognitoResponse.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Cognito Client Error: Invalid token returned");
            }

            var policy = new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(cognitoResponse.ExpiresIn)
            };
            _Cache.Set(tokenKey, accessToken, policy);

            return new AwsToken
            {
                ClientId = tokenRequest.ClientId,
                AccessToken = accessToken
            };
        }

        public void ClearToken(AwsToken token)
        {
            _Cache.Remove(GetTokenKey(token.ClientId));
        }

        private string GetTokenKey(string clientId)
        {
            return $"{clientId}: {TOKEN_KEY}";
        }
    }
}