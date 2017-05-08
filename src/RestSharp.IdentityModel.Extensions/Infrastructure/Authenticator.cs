using System;
using System.Net;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace RestSharp.IdentityModel.Extensions.Infrastructure
{
    public sealed class Authenticator
    {
        readonly Uri authorizationEndpoint;
        readonly Options options;

        TokenResponse current;

        public Authenticator(Options options)
        {
            this.authorizationEndpoint = new Uri(options.Authority, options.AuthorizationEndpointRelativePath);
            this.options = options;
            current = new TokenResponse(HttpStatusCode.Unauthorized, "Unauthorized");
        }

        Authenticator(Authenticator authenticator, TokenResponse tokenResponse)
        {
            this.authorizationEndpoint = new Uri(authenticator.options.Authority, authenticator.options.AuthorizationEndpointRelativePath);
            this.options = authenticator.options;
            current = tokenResponse;
            InitializedAt = DateTime.UtcNow;
        }

        public DateTime InitializedAt { get; private set; }

        public string AccessToken { get { return current.AccessToken; } }

        public string Error { get { return current.Error; } }

        public double ExpiresIn { get { return current.ExpiresIn - (DateTime.UtcNow - InitializedAt).TotalSeconds; } }

        public string HttpErrorReason { get { return current.HttpErrorReason; } }

        public string IdentityToken { get { return current.IdentityToken; } }

        public bool IsError { get { return current.IsError; } }

        public JObject Json { get { return current.Json; } }

        public string Raw { get { return current.Raw; } }

        public string RefreshToken { get { return current.RefreshToken; } }

        public string TokenType { get { return current.TokenType; } }

        public async Task<Authenticator> GetClientCredentialsAuthenticatorAsync()
        {
            var client = new TokenClient(authorizationEndpoint.AbsoluteUri, options.ClientId, options.ClientSecret, AuthenticationStyle.BasicAuthentication);

            TokenResponse tokenResponse = await client.RequestClientCredentialsAsync(options.Scope).ConfigureAwait(false);
            return new Authenticator(this, tokenResponse);
        }

        public async Task<Authenticator> GetResourceOwnerAuthenticatorAsync()
        {
            var client = new TokenClient(authorizationEndpoint.AbsoluteUri, options.ClientId, options.ClientSecret, AuthenticationStyle.BasicAuthentication);

            TokenResponse tokenResponse = await client.RequestResourceOwnerPasswordAsync(options.Username, options.Password, options.Scope).ConfigureAwait(false);
            return new Authenticator(this, tokenResponse);
        }

        public class Options
        {
            Options(Uri authority, string authorizationEndpointRelativePath, string clientId, string clientSecret, string scope, string username, string password, AuthenticationFlow authenticationFlow)
            {
                Authority = authority;
                ClientId = clientId;
                ClientSecret = clientSecret;
                Scope = scope;
                Username = username;
                Password = password;
                AuthenticationFlow = authenticationFlow;
                AuthorizationEndpointRelativePath = authorizationEndpointRelativePath;
            }

            public static Options UseResourceOwnerPassword(Uri authority, string authorizationEndpointRelativePath, string clientId, string clientSecret, string username, string password, string scope = "")
            {
                if (ReferenceEquals(null, authority) == true) throw new ArgumentNullException(nameof(authority));
                if (string.IsNullOrEmpty(authorizationEndpointRelativePath) == true) throw new ArgumentNullException(nameof(authorizationEndpointRelativePath));
                if (string.IsNullOrEmpty(clientId) == true) throw new ArgumentNullException(nameof(clientId));
                if (string.IsNullOrEmpty(clientSecret) == true) throw new ArgumentNullException(nameof(clientSecret));
                if (string.IsNullOrEmpty(username) == true) throw new ArgumentNullException(nameof(username));
                if (string.IsNullOrEmpty(password) == true) throw new ArgumentNullException(nameof(password));

                return new Options(authority, authorizationEndpointRelativePath, clientId, clientSecret, scope, username, password, AuthenticationFlow.ResourceOwnerPassword);
            }

            public static Options UseClientCredentials(Uri authority, string authorizationEndpointRelativePath, string clientId, string clientSecret, string scope)
            {
                if (ReferenceEquals(null, authority) == true) throw new ArgumentNullException(nameof(authority));
                if (string.IsNullOrEmpty(authorizationEndpointRelativePath) == true) throw new ArgumentNullException(nameof(authorizationEndpointRelativePath));
                if (string.IsNullOrEmpty(clientId) == true) throw new ArgumentNullException(nameof(clientId));
                if (string.IsNullOrEmpty(clientSecret) == true) throw new ArgumentNullException(nameof(clientSecret));
                if (scope == null) throw new ArgumentNullException(nameof(scope));
                return new Options(authority, authorizationEndpointRelativePath, clientId, clientSecret, scope, string.Empty, string.Empty, AuthenticationFlow.ClientCredentials);
            }

            public Uri Authority { get; private set; }

            /// <summary>
            /// Usually connect/token
            /// </summary>
            public string AuthorizationEndpointRelativePath { get; private set; }

            public string ClientId { get; private set; }

            public string ClientSecret { get; private set; }

            public string Scope { get; private set; }

            public string Username { get; private set; }

            public string Password { get; private set; }

            public AuthenticationFlow AuthenticationFlow { get; private set; }
        }

        public enum AuthenticationFlow
        {
            ResourceOwnerPassword,
            ClientCredentials
        }

        public AuthenticationFlow GetAuthenticationFlow()
        {
            return options.AuthenticationFlow;
        }
    }
}
