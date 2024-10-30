using BenchmarkDocService.Models;

namespace BenchmarkDocService.Client;

public interface IClient
{
    Task<ValueResponse<object>?> StreamAsync(object operation, CancellationToken cancellationToken = default);

    Task<ValueResponse<object>?> StreamAsync(IEnumerable<object> operations,
        CancellationToken cancellationToken = default);
}