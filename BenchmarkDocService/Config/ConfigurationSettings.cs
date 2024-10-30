namespace BenchmarkDocService.Config;

public class ConfigurationSettings
{
    // HMAC key of the customer's product instance in Episerver Turnstile
    public string AppKey { get; set; }

    // HMAC secret key of the customer's product instance in Episerver Turnstile
    public string Secret { get; set; }

    // Endpoint of the REST services of the Episerver Product that is using Episerver Turnstile's middleware for authentication
    public string Endpoint { get; set; }
    
    // Using when send the request to the cluster directly
    public string ClusterSecret { get; set; }
}