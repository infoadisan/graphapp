﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Core;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;

namespace graphconsoleapp.helpers
{
    public class MsalAuthenticationProvider : IAuthenticationProvider
    {
        private static MsalAuthenticationProvider? _singleton;
        private IPublicClientApplication _clientApplication;
        private string[] _scopes;
        private string? _userId;

        private MsalAuthenticationProvider(IPublicClientApplication clientApplication, string[] scopes)
        {
            _clientApplication = clientApplication;
            _scopes = scopes;
            _userId = null;
        }

        public static MsalAuthenticationProvider GetInstance(IPublicClientApplication clientApplication, string[] scopes)
        {
            if (_singleton == null)
            {
                _singleton = new MsalAuthenticationProvider(clientApplication, scopes);
            }

            return _singleton;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var accessToken = await GetTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
        }

        public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_userId))
            {
                try
                {
                    var account = await _clientApplication.GetAccountAsync(_userId);

                    if (account != null)
                    {
                        var silentResult = await _clientApplication.AcquireTokenSilent(_scopes, account).ExecuteAsync();
                        return silentResult.AccessToken;
                    }
                }
                catch (MsalUiRequiredException) { }
            }

            var result = await _clientApplication.AcquireTokenInteractive(_scopes).ExecuteAsync();
            _userId = result.Account.HomeAccountId.Identifier;
            return result.AccessToken;
        }
    }
}
