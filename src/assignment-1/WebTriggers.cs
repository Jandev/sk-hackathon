using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace assignment_1
{
    public class WebTriggers
    {
        private readonly ILogger _logger;

        public WebTriggers(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebTriggers>();
        }

        [Function(nameof(Summarize))]
        public HttpResponseData Summarize(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
