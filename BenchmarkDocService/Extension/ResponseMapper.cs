using BenchmarkDocService.Models;

namespace BenchmarkDocService.Extension;

internal static class ResponseMapper
{
    private static Result ToFindResult(this string result) =>
        Enum.TryParse<Result>(result, true, out var mappedResult)
            ? mappedResult
            : Result.Error;

    internal static Result ToFindResult(this HttpResponseMessage responseMessage, Result noContentResult) =>
        responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent
            ? noContentResult
            : responseMessage.StatusCode.ToString().ToFindResult();
}