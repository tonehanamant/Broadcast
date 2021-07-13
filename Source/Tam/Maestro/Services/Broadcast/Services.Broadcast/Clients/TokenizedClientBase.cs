using Services.Broadcast.Helpers;
using System;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Clients;

namespace Services.Broadcast.Clients
{
    public class TokenizedClientBase
    {
        private readonly IAwsCognitoClient _TokenClient;

        private string _TokenUrl;
        private string _ClientId;
        private string _ClientSecret;
        

        public TokenizedClientBase(IAwsCognitoClient tokenClient)
        {
            _TokenClient = tokenClient;
        }

        protected void _ConfigureTokenizedClient(string tokenUrl, string clientId, string encryptedSecret)
        {
            _TokenUrl = tokenUrl;
            _ClientId = clientId;
            _ClientSecret = EncryptionHelper.DecryptString(encryptedSecret, EncryptionHelper.EncryptionKey);
        }

        protected AwsToken _GetToken()
        {
            if (string.IsNullOrWhiteSpace(_TokenUrl) || string.IsNullOrWhiteSpace(_ClientId) || string.IsNullOrWhiteSpace(_ClientSecret))
            {
                throw new InvalidOperationException("Client not configured.  Call _ConfigureTokenizedClient(...) before calling this method.");
            }

            try
            {
                return _TokenClient.GetToken(new AwsTokenRequest { TokenUrl = _TokenUrl, ClientId = _ClientId, ClientSecret = _ClientSecret });
            }
            catch (Exception e)
            {
                throw new Exception("Error acquiring authentication token.", e);
            }
        }
    }
}