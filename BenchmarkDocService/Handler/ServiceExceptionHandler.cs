using System.Net;
using BenchmarkDocService.Models;

namespace BenchmarkDocService.Handler;

public class ServiceExceptionHandler: DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ServiceHttpException(response.StatusCode, "Invalid credentials. Try verifying your credentials.");
            }

            return response;
        }
        catch (ServiceException serviceException)
        {
            throw serviceException;
        }
        catch (Exception ex)
        {
            throw new ServiceException(ex.Message, ex);
        }
    }
}