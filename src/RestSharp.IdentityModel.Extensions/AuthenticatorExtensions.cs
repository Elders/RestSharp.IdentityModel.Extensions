using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace RestSharp
{
    public static class AuthenticatorExtensions
    {
        public static async Task<Authenticator> GetClientCredentialsAuthenticatorAsync(this Authenticator authenticator)
        {
            var options = authenticator.TheOptions;
            var client = new TokenClient(authenticator.AuthorizationEndpoint.AbsoluteUri, options.ClientId, options.ClientSecret, AuthenticationStyle.BasicAuthentication);

            TokenResponse tokenResponse = await client.RequestClientCredentialsAsync(options.Scope).ConfigureAwait(false);
            return new Authenticator(authenticator, tokenResponse);
        }

        public static async Task<Authenticator> GetResourceOwnerAuthenticatorAsync(this Authenticator authenticator)
        {
            var options = authenticator.TheOptions;
            var client = new TokenClient(authenticator.AuthorizationEndpoint.AbsoluteUri, options.ClientId, options.ClientSecret, AuthenticationStyle.BasicAuthentication);

            TokenResponse tokenResponse = await client.RequestResourceOwnerPasswordAsync(options.Username, options.Password, options.Scope).ConfigureAwait(false);
            return new Authenticator(authenticator, tokenResponse);
        }

        public static async Task<Authenticator> ImpersonateAsync(this Authenticator authenticator, string username, string accessToken)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(accessToken)) throw new ArgumentNullException(nameof(accessToken));

            var options = authenticator.TheOptions;

            var values = new Dictionary<string, string>();
            values.Add("acr_values", $"Impersonate:{username} access_token:{accessToken}");

            var client = new TokenClient(options.Authority.AbsolutePath, options.ClientId, options.ClientSecret, AuthenticationStyle.BasicAuthentication);
            TokenResponse tokenResponse = await client.RequestResourceOwnerPasswordAsync("impersonate", Guid.NewGuid().ToString("n"), options.Scope, values).ConfigureAwait(false);

            return new Authenticator(authenticator, tokenResponse);
        }
    }
}
