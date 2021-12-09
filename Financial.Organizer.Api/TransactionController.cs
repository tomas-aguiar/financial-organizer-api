using System;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Registry;

namespace Financial.Organizer.Api
{
    [ApiController]
    [Route("api/v1/transaction")]
    public class TransactionController : Controller
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;
        private readonly HttpClient _client;

        public TransactionController(IReadOnlyPolicyRegistry<string> policyRegistry,
            IHttpClientFactory httpClientFactory)
        {
            _policy = policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>(PolicyKey);
            _client = httpClientFactory.CreateClient(ClientName);
        }

        private const string PolicyKey = "PollyCache";
        private const string ClientName = "clientName";
        private const string GetUri = "https://getUri.com";

        [HttpGet]
        public HttpResponseMessage Get(string id = "first")
        {
            // var response = _policy.ExecuteAsync(async ctx => 
            //     await _client.GetAsync(new Uri(GetUri + id)), new Context("some-key"));
            var response = _policy.ExecuteAsync(async ctx => 
                await _client.GetAsync(new Uri(GetUri + id)), new Context(id));
            return response.Result;
        }
    }
}