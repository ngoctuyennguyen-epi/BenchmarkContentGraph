using BenchmarkDocService.Config;

namespace BenchmarkDocService;

static class Program
{
    static async Task Main(string[] args)
    {
        var requestCount = 0;
        const int duration = 15; // Minutes
        var endTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(duration));

        var configurationSettings = new ConfigurationSettings
        {
            Endpoint = "***",
            AppKey = "***",
            Secret = "***",
            ClusterSecret = "***"
        };

        var client = new Client.Client(configurationSettings.Endpoint, configurationSettings.AppKey,
            configurationSettings.Secret, configurationSettings.ClusterSecret);

        var operation = new
        {
            Id = "1",
            LanguageRouting = "en",
            Document = Constants.Constants.Data
        };

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        Console.WriteLine($"Starting API calls for {duration} minute(s)...");

        await Parallel.ForEachAsync(Enumerable.Range(0, 10_000_000), options, async (i, token) =>
        {
            if (DateTime.UtcNow >= endTime) return;
            var subTasks = Enumerable.Range(0, 300)
                .Select(_ =>
                {
                    Interlocked.Increment(ref requestCount);
                    return client.StreamAsync(operation, token);
                })
                .ToArray();

            await Task.WhenAll(subTasks);
        });

        Console.WriteLine($"Total API calls made: {requestCount}");
    }
}