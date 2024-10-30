using System.Net;

namespace BenchmarkDocService.Models;

public class ServiceException : HttpRequestException
{
    protected ServiceException(string message) : base(message)
    {
    }

    public ServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ServiceHttpException : ServiceException
{
    public HttpStatusCode HttpStatusCode;

    public ServiceHttpException(HttpStatusCode httpStatusCode, string message) : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }

    public ServiceHttpException(HttpStatusCode httpStatusCode, string message, Exception innerException) : base(message,
        innerException)
    {
        HttpStatusCode = httpStatusCode;
    }
}