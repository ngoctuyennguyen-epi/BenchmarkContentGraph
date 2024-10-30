using System.Net;
using System.Text;
using System.Text.Json;
using BenchmarkDocService.Extension;
using BenchmarkDocService.Models;

namespace BenchmarkDocService.Client;

public partial class Client
{
    public async Task<ValueResponse<object>?> StreamAsync(object operation,
        CancellationToken cancellationToken = default)
    {
        return await StreamAsync(new List<object> { operation }, cancellationToken);
    }

    public async Task<ValueResponse<object>?> StreamAsync(IEnumerable<object> operations,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, StreamingServiceEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(operations), Encoding.UTF8, "application/x-ndjson")
            };

            requestMessage.Version = HttpVersion.Version30;
            using var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var result = responseMessage.ToFindResult(Result.Ok);

            if (responseMessage.StatusCode != HttpStatusCode.Created && responseMessage.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Error ---- " + (int)responseMessage.StatusCode + " ---- " + jsonString);
            }

            return new ValueResponse<object>
            {
                Result = result,
                Value = new(),
                Error = result != Result.Ok
                    ? new ResponseError
                    {
                        Status = (int)responseMessage.StatusCode,
                        Message = jsonString
                    }
                    : null
            };
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception ---- " + e.Message + e.StackTrace + e.InnerException?.Message +
                              e.InnerException?.StackTrace);
            return null;
        }
    }
}