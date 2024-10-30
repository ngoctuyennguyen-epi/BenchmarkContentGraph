namespace BenchmarkDocService.Models;

public class ValueResponse<T> : Response
{
    public T Value { get; set; }

    public static implicit operator T(ValueResponse<T> valueResponse) => valueResponse.Value;
}