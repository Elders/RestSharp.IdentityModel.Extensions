using System;
using System.Net;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace RestSharp
{
    public sealed class Authenticator
    {
        readonly Uri authorizationEndpoint;
        public Options TheOptions { get; private set; }

        TokenResponse current;

        public Authenticator(Options options)
        {
            this.authorizationEndpoint = new Uri(options.Authority, options.AuthorizationEndpointRelativePath);
            this.TheOptions = options;
            current = new TokenResponse(HttpStatusCode.Unauthorized, "Unauthorized", "Unauthorized");
        }

        Authenticator(Authenticator authenticator, TokenResponse tokenResponse)
        {
            this.authorizationEndpoint = new Uri(authenticator.TheOptions.Authority, authenticator.TheOptions.AuthorizationEndpointRelativePath);
            this.TheOptions = authenticator.TheOptions;
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
            var client = new TokenClient(authorizationEndpoint.AbsoluteUri, TheOptions.ClientId, TheOptions.ClientSecret, AuthenticationStyle.BasicAuthentication);

            TokenResponse tokenResponse = await client.RequestClientCredentialsAsync(TheOptions.Scope, TheOptions.Extras).ConfigureAwait(false);
            return new Authenticator(this, tokenResponse);
        }

        public async Task<Authenticator> GetResourceOwnerAuthenticatorAsync()
        {
            var client = new TokenClient(authorizationEndpoint.AbsoluteUri, TheOptions.ClientId, TheOptions.ClientSecret, AuthenticationStyle.BasicAuthentication);

            TokenResponse tokenResponse = await client.RequestResourceOwnerPasswordAsync(TheOptions.Username, TheOptions.Password, TheOptions.Scope, TheOptions.Extras).ConfigureAwait(false);
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

            public static Options UseResourceOwnerPassword(Uri authority, string clientId, string clientSecret, string username, string password, string scope, string authorizationEndpointRelativePath = "connect/token")
            {
                if (ReferenceEquals(null, authority) == true) throw new ArgumentNullException(nameof(authority));
                if (string.IsNullOrEmpty(authorizationEndpointRelativePath) == true) throw new ArgumentNullException(nameof(authorizationEndpointRelativePath));
                if (string.IsNullOrEmpty(clientId) == true) throw new ArgumentNullException(nameof(clientId));
                if (string.IsNullOrEmpty(clientSecret) == true) throw new ArgumentNullException(nameof(clientSecret));
                if (string.IsNullOrEmpty(username) == true) throw new ArgumentNullException(nameof(username));
                if (string.IsNullOrEmpty(password) == true) throw new ArgumentNullException(nameof(password));

                return new Options(authority, authorizationEndpointRelativePath, clientId, clientSecret, scope, username, password, AuthenticationFlow.ResourceOwnerPassword);
            }

            public static Options UseClientCredentials(Uri authority, string clientId, string clientSecret, string scope, string authorizationEndpointRelativePath = "connect/token")
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

            public object Extras { get; set; }

            public AuthenticationFlow AuthenticationFlow { get; private set; }
        }

        public enum AuthenticationFlow
        {
            ResourceOwnerPassword,
            ClientCredentials
        }

        public AuthenticationFlow GetAuthenticationFlow()
        {
            return TheOptions.AuthenticationFlow;
        }
    }
}
