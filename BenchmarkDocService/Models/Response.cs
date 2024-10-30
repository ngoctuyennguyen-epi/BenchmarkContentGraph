namespace BenchmarkDocService.Models;

public class Response
{
    public Result Result { get; set; }

    public ResponseError? Error { get; set; }
}