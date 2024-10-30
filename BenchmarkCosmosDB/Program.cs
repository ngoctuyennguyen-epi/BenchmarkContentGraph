using Microsoft.Azure.Cosmos;

namespace BenchmarkCosmosDB;

static class Program
{
    private static CosmosClient _client;
    private static Database _database;
    private static Container _container;
    static async Task Main(string[] args)
    {
        _client = new(
            connectionString: "***"
        );
        
        _database = _client.GetDatabase("Find");
        _container = _database.GetContainer("Testing");

        
        
        var tasks = new List<int>();
        for (var i = 0; i < 1; i++)
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
            for (var j = 0; j < 1; j++)
            {
                var item = new Product(
                    Id: Guid.NewGuid().ToString(),
                    Category: "gear-surf-surfboards",
                    Name: "Yamba Surfboard",
                    Quantity: 12,
                    Price: 850.00m,
                    Clearance: false,
                    TenantId: "36fae3ba9ba84bdfb833b26397793a03"
                );
                subTasks.Add(SendDataAsync(item));
            }

            await Task.WhenAll(subTasks);
        });

       
        Console.WriteLine("Hello, World!");
    }

    static async Task SendDataAsync(Product data)
    {
        try
        {
            var response = await _container.UpsertItemAsync(
                item: data,
                partitionKey: new PartitionKey("36fae3ba9ba84bdfb833b26397793a03")
            );
            
            Console.WriteLine(response.StatusCode);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private record Product(
        string Id,
        string Category,
        string Name,
        int Quantity,
        decimal Price,
        bool Clearance,
        string TenantId
    );
}