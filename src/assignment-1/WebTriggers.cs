using System.Net;
using System.Text.Json;
using assignment_1.Summarize;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace assignment_1
{
	public class WebTriggers
	{
		private readonly ILogger _logger;
		private readonly IInvoker<WebsiteRequest> invoker;

		public WebTriggers(
			IInvoker<WebsiteRequest> invoker,
			ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<WebTriggers>();
			this.invoker = invoker;
		}

		[Function(nameof(Summarize))]
		public async Task<HttpResponseData> Summarize(
			[HttpTrigger(AuthorizationLevel.Function, "post")]
			HttpRequestData req,
			CancellationToken hostCancellationToken)
		{
			using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken);

			var data = await JsonSerializer.DeserializeAsync<Request>(
				new StreamReader(req.Body).BaseStream,
				options: new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				},
				cancellationToken: cancellationSource.Token);
			if(data == null || string.IsNullOrWhiteSpace(data.Url))
			{
				throw new ArgumentException(nameof(data), "Input not in the correct format.");
			}
			var websiteRequest = new WebsiteRequest(new Uri(data.Url));
			try
			{
				var summary = await this.invoker.Invoke(websiteRequest);

				var response = req.CreateResponse(HttpStatusCode.OK);

				await response.WriteAsJsonAsync(new Response
				{
					Ask = websiteRequest.Url.ToString(),
					Summary = summary
				});
				return response;
			}
			catch (Exception ex)
			{
				var response = req.CreateResponse(HttpStatusCode.InternalServerError);
				response.WriteString(ex.Message);
				return response;
			}
		}

		public class Request
		{
			public string Url { get; set; } = string.Empty;
		}
		public class Response
		{
			public string Ask { get; set; } = string.Empty;
			public string Summary { get; set; } = string.Empty;
		}
	}
}
