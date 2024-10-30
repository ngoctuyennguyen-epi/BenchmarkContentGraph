using System.Net.Security;
using BenchmarkDocService.Handler;
using EPiServer.Turnstile.Contracts.Hmac;

namespace BenchmarkDocService.Client;

public partial class Client : IClient
{
    private static string UserAgent => $"EPiServer-Find-NET-API/{typeof(Client).Assembly.GetName().Version}";
    private const string StreamingServiceEndpoint = "_stream";

    protected virtual HttpClient _httpClient { get; }

    public Client()
    {
    }

    public Client(string baseAddress, string key, string secret, string clusterSecret) : this()
    {
        var delegatingHandlers = new List<DelegatingHandler>();
        if (!string.IsNullOrWhiteSpace(secret))
        {
            delegatingHandlers.Add(new HmacMessageHandler(
                new DefaultHmacDeclarationFactory(new Sha256HmacAlgorithm(Convert.FromBase64String(secret))),
                key));
        }

        delegatingHandlers.Add(new ServiceExceptionHandler());

        var delegatingHandlerFactory = new DelegatingHandlerFactory(() => delegatingHandlers.ToArray());

        baseAddress = SafeUrl(baseAddress);
        _httpClient = CreateHttpClient(baseAddress, key, clusterSecret, delegatingHandlerFactory);
    }

    private static HttpClient CreateHttpClient(
        string baseAddress,
        string key,
        string clusterSecret,
        DelegatingHandlerFactory delegatingHandlerFactory)
    {
        var sslOptions = new SslClientAuthenticationOptions
        {
            // Leave certs unvalidated for debugging
            RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true,
        };
        var socketsHttpHandler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            SslOptions = sslOptions
        };

        return new HttpClient(delegatingHandlerFactory.Create(socketsHttpHandler))
        {
            BaseAddress = new Uri(baseAddress),
            DefaultRequestHeaders =
            {
                { "User-Agent", UserAgent },
                { "Authorization", $"epi-single {key}" },
                { "X-Cluster-Secret", clusterSecret }
            }
        };
    }

    private static string SafeUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.AbsoluteUri;
        }
        catch (Exception ex)
        {
            throw new ArgumentException(ex.Message, ex);
        }
    }
}