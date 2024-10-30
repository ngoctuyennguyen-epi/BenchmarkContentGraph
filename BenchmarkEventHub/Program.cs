using System.Diagnostics;
using System.Net;
using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace BenchmarkEventHub;

class Program
{
    private static HttpClient _httpClient;
    private static EventHubProducerClient _eventHubProducerClient;
    
    static async Task Main(string[] args)
    {
        var eventHubName = "***";
        var sasKeyName = "***";
        var namespaceName = "***";
        var sasKey = "***";
        var connectionString = $"Endpoint=sb://{namespaceName}.servicebus.windows.net/;SharedAccessKeyName={sasKeyName};SharedAccessKey={sasKey};EntityPath={eventHubName}";
        var eventHubProducerClientOptions = new EventHubProducerClientOptions
        {
            ConnectionOptions =
            {
                TransportType = EventHubsTransportType.AmqpWebSockets
            }
        };
        
        _eventHubProducerClient = new EventHubProducerClient(connectionString, eventHubProducerClientOptions);
        
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
        
        var tasks = Enumerable.Range(1, 100).ToList();
        
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 100
        };

        // var batches = tasks.Chunk(5);

        await Parallel.ForEachAsync(tasks, options, async (i, token) =>
        {
            var subTasks = new List<Task>();
            for (var j = 0; j < 5000; j++)
            {
                subTasks.Add(SendDataAsyncEventHub());
            }
            
            // await SendDataAsyncEventHub();

            await Task.WhenAll(subTasks);
        });
        
        Console.WriteLine("Finished benchmark event hub");
    }
    
    static async Task SendDataAsync()
    {
        var timer = new Stopwatch();
        timer.Start();

        var eventHubName = "***";
        var sasKeyName = "***";
        var namespaceName = "***";
        var sasKey = "***";
        var resourceUri = $"https://{namespaceName}.servicebus.windows.net/{eventHubName}";
        var sasToken = SasTokenHelper.GenerateSasToken(resourceUri, sasKeyName, sasKey);

        var eventHubUrl = $"{resourceUri}/messages";

        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.RequestUri = new Uri(eventHubUrl);
        httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", sasToken);
        httpRequestMessage.Headers.TryAddWithoutValidation("BrokerProperties", $"{{\"PartitionKey\":\"72cb7d76983144c4ada22a063d1a0dda\"}}");
        httpRequestMessage.Headers.TryAddWithoutValidation("TenantId", "72cb7d76983144c4ada22a063d1a0dda");
        httpRequestMessage.Headers.TryAddWithoutValidation("X-Correlation-ID", "a123456");

        
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
    
    static async Task SendDataAsyncEventHub()
    {
        try
        {
            var timer = new Stopwatch();
            timer.Start();
            ThreadPool.GetAvailableThreads(out var worker, out var io);
            Console.WriteLine($"Worker threads {worker} --- IO threads {io} --- PendingWorkItemCount {ThreadPool.PendingWorkItemCount}");
            var eventDataBatch = await _eventHubProducerClient.CreateBatchAsync();
            var eventData = new EventData(Encoding.UTF8.GetBytes(Constants.Data));
            eventData.Properties.Add("TenantId", "72cb7d76983144c4ada22a063d1a0dda");
            
            eventDataBatch.TryAdd(eventData);
            await _eventHubProducerClient.SendAsync(eventDataBatch);

            timer.Stop();
            
            var timeTaken = timer.Elapsed.Milliseconds;

            // Console.WriteLine($"Time Taken: {timeTaken}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            // throw;
        }

    }
}