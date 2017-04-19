using System;
using RestSharp.IdentityModel.Extensions.Infrastructure;

namespace RestSharp.IdentityModel.Extensions
{
    public class Client
    {
        Options options;
        Authenticator authenticator;
        IRestClient client;

        public Client(Options options, Authenticator authenticator = null)
        {
            if (ReferenceEquals(null, options)) throw new ArgumentNullException(nameof(options));
            this.options = options;

            this.authenticator = authenticator;

            client = new RestClient(options.ApiAddress);
            client.ClearHandlers();
            client.AddHandler("application/json", options.JsonSerializer);
            client.AddHandler("text/json", options.JsonSerializer);
            client.AddHandler("text/x-json", options.JsonSerializer);
            client.AddHandler("text/javascript", options.JsonSerializer);
            client.AddHandler("*+json", options.JsonSerializer);
        }

        public IRestResponse<T> Execute<T>(IRestRequest request) where T : new()
        {
            return client.Execute<T>(request);
        }

        public IRestResponse<T> Execute<T>(string resource, Method method, object body, Authenticator inlineAuthenticator = null) where T : new()
        {
            return Execute<T>(CreateRestRequest(resource, method, body, inlineAuthenticator));
        }

        public IRestRequest CreateRestRequest(string resource, Method method, object body, Authenticator inlineAuthenticator = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = options.JsonSerializer;
            request.AddAuthorizationBearerHeader(Eval(inlineAuthenticator));
            request.AddJsonBody(body);
            return request;
        }

        Authenticator Eval(Authenticator inlineAuthenticator)
        {
            if (ReferenceEquals(null, authenticator) && ReferenceEquals(null, inlineAuthenticator))
                throw new ArgumentNullException(nameof(authenticator), "Not a valid authenticator is specified. You can initialize a default Authenticator on Client.ctor(...) or specify explicitly the Authenticator with each call. On of the two is mandatory.");

            if (ReferenceEquals(null, inlineAuthenticator) == false)
                return inlineAuthenticator;

            if (authenticator.ExpiresIn < 120)
            {
                var flow = authenticator.GetAuthenticationFlow();

                if (flow == Authenticator.AuthenticationFlow.ClientCredentials)
                    authenticator = authenticator.GetClientCredentialsAuthenticatorAsync().Result;
                else if (flow == Authenticator.AuthenticationFlow.ResourceOwnerPassword)
                    authenticator = authenticator.GetResourceOwnerAuthenticatorAsync().Result;
                else
                    throw new NotImplementedException();
            }

            return authenticator;
        }

        public sealed class Options
        {
            public Options(Uri apiAddress)
            {
                if (ReferenceEquals(null, apiAddress) == true) throw new ArgumentNullException(nameof(apiAddress));

                ApiAddress = apiAddress;
                JsonSerializer = NewtonsoftJsonSerializer.Default();
            }

            public Options(Uri apiAddress, IJsonSerializer jsonSerializer) : this(apiAddress)
            {
                if (ReferenceEquals(null, jsonSerializer) == true) throw new ArgumentNullException(nameof(jsonSerializer));
                JsonSerializer = jsonSerializer;
            }

            public Uri ApiAddress { get; private set; }
            public IJsonSerializer JsonSerializer { get; private set; }
        }
    }
}
