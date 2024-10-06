using System.Diagnostics;
using System.Net;
using System.Text;

namespace BenchmarkEventHub;

class Program
{
    private static HttpClient _httpClient;
    
    static async Task Main(string[] args)
    {
        var socketsHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(240),
            MaxConnectionsPerServer = int.MaxValue,
            UseCookies = false,
            EnableMultipleHttp2Connections = true,
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(240),
        };

        _httpClient = new HttpClient(socketsHandler)
        {
            Timeout = TimeSpan.FromSeconds(100),
            DefaultRequestVersion = HttpVersion.Version20
        };
        
        Console.WriteLine("Started benchmark event hub");
        
        var tasks = new List<int>();
        for (var i = 0; i < 100; i++)
        {
            tasks.Add(i);
        }
        
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 10
        };

        // var batches = tasks.Chunk(5);

        await Parallel.ForEachAsync(tasks, options, async (i, token) =>
        {
            var subTasks = new List<Task>();
            for (var j = 0; j < 1000; j++)
            {
                subTasks.Add(SendDataAsync());
            }

            await Task.WhenAll(subTasks);
        });
        
        Console.WriteLine("Finished benchmark event hub");
    }
    
    static async Task SendDataAsync()
    {
        var timer = new Stopwatch();
        timer.Start();

        var eventHubName = "{event-hub-name}";
        var sasKeyName = "{key-name}";
        var namespaceName = "{namespace-name}";
        var sasKey = "{key}";
        var resourceUri = $"https://{namespaceName}.servicebus.windows.net/{eventHubName}";
        var sasToken = SasTokenHelper.GenerateSasToken(resourceUri, sasKeyName, sasKey);

        var eventHubUrl = $"{resourceUri}/messages";

        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.RequestUri = new Uri(eventHubUrl);
        httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", sasToken);
        
        var data = Constants.Data;
        HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");

        try
        {
            httpRequestMessage.Content = content;
            httpRequestMessage.Method = HttpMethod.Post;
            httpRequestMessage.Version = HttpVersion.Version20;

            var response = await _httpClient.SendAsync(httpRequestMessage, default);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to send message: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception message: {e.Message}");
        }

        timer.Stop();
        
        var timeTaken = timer.Elapsed.Milliseconds;
        Console.WriteLine($"Time Taken: {timeTaken}");
    }
}