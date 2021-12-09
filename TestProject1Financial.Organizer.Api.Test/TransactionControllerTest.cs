using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Financial.Organizer.Api;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Contrib.HttpClient;
using Moq.Protected;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Registry;
using Xunit;

namespace TestProject1Financial.Organizer.Api.Test
{
    public class TransactionControllerTest
    {
        private readonly TransactionController _sut;
        private readonly Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public TransactionControllerTest()
        {
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _client = CreateHttpClientMock();
            _mockClientFactory.Setup(f => f.CreateClient(ClientName))
                .Returns(_client);
            var mockPolicyRegistry = CreatePolicyRegistry();
            _sut = new TransactionController(mockPolicyRegistry, _mockClientFactory.Object);
        }
        
        private const string ClientName = "clientName";
        private const string GetUri = "https://getUri.com";
        private const string PollyCache = "PollyCache";
        
        [Fact]
        public void ShouldCreateClient()
        {
            _mockClientFactory.Verify(f => f.CreateClient(ClientName));
        }

        [Fact]
        public void ShouldGetResponseFromClient_WhenItsTheFirstTime()
        {
            _handlerMock.SetupRequest(HttpMethod.Get, new Uri(GetUri)).ReturnsResponse(HttpStatusCode.Accepted);

            _sut.Get();
            
            _handlerMock.VerifyRequest(HttpMethod.Get, new Uri(GetUri), Times.Exactly(1));
        }
        
        [Fact]
        public void ShouldGetResponseFromCache_WhenItsNotTheFirstTime()
        {
            _handlerMock.SetupRequest(HttpMethod.Get, new Uri(GetUri)).ReturnsResponse(HttpStatusCode.Accepted);

            _sut.Get();
            _sut.Get();
            
            _handlerMock.VerifyRequest(HttpMethod.Get, new Uri(GetUri), Times.Exactly(1));
        }
        
        [Fact]
        public void ShouldGetResponseFromClient_WhenItsNotTheSameArgument()
        {
            const string second = "second";
            _handlerMock.SetupRequest(HttpMethod.Get, new Uri(GetUri + "first"))
                .ReturnsResponse(HttpStatusCode.Accepted);
            _handlerMock.SetupRequest(HttpMethod.Get, new Uri(GetUri + second))
                .ReturnsResponse(HttpStatusCode.Accepted);
            
            _sut.Get();
            _sut.Get(second);
            
            _handlerMock.VerifyRequest(HttpMethod.Get, new Uri(GetUri + "first"), Times.Exactly(1));
            _handlerMock.VerifyRequest(HttpMethod.Get, new Uri(GetUri + second), Times.Exactly(1));
        }

        private HttpClient CreateHttpClientMock()
        {
            _handlerMock = new Mock<HttpMessageHandler>();

            return _handlerMock.CreateClient();
        }

        private IReadOnlyPolicyRegistry<string> CreatePolicyRegistry()
        {
            var registry = new PolicyRegistry
            {
                {
                    PollyCache, Policy.CacheAsync(
                        new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions()))
                            .AsyncFor<HttpResponseMessage>(),
                        TimeSpan.FromMinutes(5))
                }
            };

            return registry;
        }
    }
}