namespace BenchmarkDocService.Handler;

public class DelegatingHandlerFactory
{
    private readonly Func<DelegatingHandler[]> _createDelegatingHandlers;

    public DelegatingHandlerFactory(Func<DelegatingHandler[]> createDelegatingHandlers)
    {
        _createDelegatingHandlers = createDelegatingHandlers;
    }

    public DelegatingHandler Create(HttpMessageHandler baseHandler)
    {
        var delegatingHandlers = _createDelegatingHandlers();
        delegatingHandlers.Aggregate((outer, inner) =>
        {
            outer.InnerHandler = inner;
            return inner;
        });

        delegatingHandlers.Last().InnerHandler = baseHandler;
        return delegatingHandlers.First();
    }
}