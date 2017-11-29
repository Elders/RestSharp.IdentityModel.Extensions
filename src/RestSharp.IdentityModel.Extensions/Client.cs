using System;
using System.Collections.Generic;
using System.Reflection;

namespace RestSharp
{
    public class RestSharpIdentityModelClient
    {
        Options options;
        Authenticator authenticator;
        IRestClient client;

        public RestSharpIdentityModelClient(Options options, Authenticator authenticator = null)
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
            if (ReferenceEquals(null, request) == true) throw new ArgumentNullException(nameof(request));

            return client.Execute<T>(request);
        }

        public IRestResponse<T> Execute<T>(string resource, Method method, object body, Authenticator inlineAuthenticator = null) where T : new()
        {
            if (string.IsNullOrEmpty(resource) == true) throw new ArgumentNullException(nameof(resource));
            if (ReferenceEquals(null, method) == true) throw new ArgumentNullException(nameof(method));

            return Execute<T>(CreateRestRequest(resource, method, body, inlineAuthenticator));
        }

        public IRestResponse<T> Execute<T>(string resource, Method method, object body, List<Parameter> parameters, Authenticator inlineAuthenticator = null) where T : new()
        {
            if (string.IsNullOrEmpty(resource) == true) throw new ArgumentNullException(nameof(resource));
            if (ReferenceEquals(null, parameters) == true) throw new ArgumentNullException(nameof(parameters));
            if (ReferenceEquals(null, method) == true) throw new ArgumentNullException(nameof(method));

            return Execute<T>(CreateRestRequest(resource, method, body, parameters, inlineAuthenticator));
        }

        public IRestResponse<T> ExecuteGet<T>(string resource, object body, List<Parameter> parameters, Authenticator inlineAuthenticator = null) where T : new()
        {
            if (string.IsNullOrEmpty(resource) == true) throw new ArgumentNullException(nameof(resource));
            if (ReferenceEquals(null, parameters) == true) throw new ArgumentNullException(nameof(parameters));

            var parametersFromBody = ExtractParameters(body);
            return Execute<T>(CreateRestRequest(resource, Method.GET, body, parameters, inlineAuthenticator));
        }

        List<Parameter> ExtractParameters(object body)
        {
            var paramethers = new List<Parameter>();

            if (ReferenceEquals(null, body) == true)
                return paramethers;

            var props = new List<PropertyInfo>(body.GetType().GetProperties());
            foreach (var prop in props)
            {
                object propValue = prop.GetValue(body, null);
                var parameter = new Parameter
                {
                    Name = prop.Name,
                    Value = propValue,
                    Type = ParameterType.QueryString
                };
                paramethers.Add(parameter);
            }

            return paramethers;
        }

        public IRestRequest CreateRestRequest(string resource, Method method, object body, Authenticator inlineAuthenticator = null)
        {
            if (string.IsNullOrEmpty(resource) == true) throw new ArgumentNullException(nameof(resource));
            if (ReferenceEquals(null, method) == true) throw new ArgumentNullException(nameof(method));

            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = options.JsonSerializer;
            request.AddAuthorizationBearerHeader(Eval(inlineAuthenticator));
            request.AddJsonBody(body);
            return request;
        }

        public IRestRequest CreateRestRequest(string resource, Method method, object body, List<Parameter> parameters, Authenticator inlineAuthenticator = null)
        {
            if (string.IsNullOrEmpty(resource) == true) throw new ArgumentNullException(nameof(resource));
            if (ReferenceEquals(null, parameters) == true) throw new ArgumentNullException(nameof(parameters));
            if (ReferenceEquals(null, method) == true) throw new ArgumentNullException(nameof(method));

            var request = CreateRestRequest(resource, method, body, inlineAuthenticator);
            foreach (var par in parameters)
            {
                request.AddParameter(par);
            }
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
