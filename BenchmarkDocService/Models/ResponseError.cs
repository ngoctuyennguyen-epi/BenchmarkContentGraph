namespace BenchmarkDocService.Models;

public class ResponseError
{
    /// <summary>
    /// The error status.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// The error information.
    /// </summary>
    public string Message { get; set; }
}