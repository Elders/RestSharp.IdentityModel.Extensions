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
            var client = new TokenClient(authenticator.AuthorizationEndpoint.AbsoluteUri, options.ClientId, options.ClientSecret, style: AuthenticationStyle.BasicAuthentication);

            TokenResponse tokenResponse = await client.RequestClientCredentialsAsync(options.Scope).ConfigureAwait(false);
            return new Authenticator(authenticator, tokenResponse);
        }

        public static async Task<Authenticator> GetResourceOwnerAuthenticatorAsync(this Authenticator authenticator)
        {
            var options = authenticator.TheOptions;
            var client = new TokenClient(authenticator.AuthorizationEndpoint.AbsoluteUri, options.ClientId, options.ClientSecret, style: AuthenticationStyle.BasicAuthentication);

            TokenResponse tokenResponse = await client.RequestResourceOwnerPasswordAsync(options.Username, options.Password, options.Scope).ConfigureAwait(false);
            return new Authenticator(authenticator, tokenResponse);
        }

        public static async Task<Authenticator> RefreshTokenAsync(this Authenticator authenticator, string refreshToken = null)
        {
            var options = authenticator.TheOptions;

            var client = new TokenClient(authenticator.AuthorizationEndpoint.AbsoluteUri, options.ClientId, options.ClientSecret, style: AuthenticationStyle.BasicAuthentication);
            TokenResponse tokenResponse = await client.RequestRefreshTokenAsync(refreshToken ?? authenticator.RefreshToken).ConfigureAwait(false);
            return new Authenticator(authenticator, tokenResponse);
        }
    }
}
