using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;

namespace graphconsoleapp.helpers
{
    public class MsalAuthProviderG5 : IAuthenticationProvider
    {
        private static MsalAuthProviderG5 singletonField;
        private readonly IPublicClientApplication clientApplicationField;
        private readonly string[] scopesField;
        private readonly string usernameField;
        private readonly SecureString passwordField;
        private string userIdField;

        public MsalAuthProviderG5(IPublicClientApplication clientApplication, string[] scopes, string username, SecureString password)
        {
            clientApplicationField = clientApplication;
            scopesField = scopes;
            usernameField = username;
            passwordField = password;
            userIdField = null;

        }

        public static MsalAuthProviderG5 GetInstance(IPublicClientApplication clientApplication, string[] scopes, string username, SecureString password)
        {
            if(singletonField == null)
            {
                singletonField = new MsalAuthProviderG5(clientApplication, scopes, username, password);
            }
            return singletonField;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var accessToken = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
        }

        public async Task<string> GetTokenAsync()
        {
            if(!string.IsNullOrEmpty(userIdField))
            {
                try
                {
                    var account = await clientApplicationField.GetAccountAsync(userIdField);

                    if (account != null)
                    {
                        var silentResult = await clientApplicationField.AcquireTokenSilent(scopesField, account).ExecuteAsync();
                        return silentResult.AccessToken;
                    }
                }
                catch (MsalUiRequiredException) { }

            }
            var result = await clientApplicationField.AcquireTokenByUsernamePassword(scopesField, usernameField, passwordField).ExecuteAsync();
            userIdField = result.Account.HomeAccountId.Identifier;
            return result.AccessToken;
        }

        public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
