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
		private readonly IInvoker<WebsiteRequest> websiteSummaryInvoker;
		private readonly IInvoker<Summarize.TextRequest> textInvoker;

		public WebTriggers(
			IInvoker<WebsiteRequest> websiteSummaryInvoker,
			IInvoker<Summarize.TextRequest> repositorySummaryInvoker,
			ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<WebTriggers>();
			this.websiteSummaryInvoker = websiteSummaryInvoker;
			this.textInvoker = repositorySummaryInvoker;
		}

		[Function(nameof(Summarize))]
		public async Task<HttpResponseData> Summarize(
			[HttpTrigger(AuthorizationLevel.Function, "post")]
			HttpRequestData req,
			CancellationToken hostCancellationToken)
		{
			using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken);

			var data = await JsonSerializer.DeserializeAsync<WebsiteSummaryRequest>(
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
				var summary = await this.websiteSummaryInvoker.Invoke(websiteRequest);

				var response = req.CreateResponse(HttpStatusCode.OK);

				await response.WriteAsJsonAsync(new WebsiteSummaryResponse
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

		[Function(nameof(TextOperation))]
		public async Task<HttpResponseData> TextOperation(
			[HttpTrigger(AuthorizationLevel.Function, "post")]
			HttpRequestData req,
			CancellationToken hostCancellationToken
			)
		{
			using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken);

			var data = await JsonSerializer.DeserializeAsync<TextSummaryRequest>(
				new StreamReader(req.Body).BaseStream,
				options: new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				},
				cancellationToken: cancellationSource.Token);
			if (data == null || string.IsNullOrWhiteSpace(data.Text))
			{
				throw new ArgumentException(nameof(data), "Input not in the correct format.");
			}
			var repositoryRequest = new TextRequest(data.Text);
			try
			{
				var result = await this.textInvoker.Invoke(repositoryRequest);

				var response = req.CreateResponse(HttpStatusCode.OK);

				await response.WriteAsJsonAsync(
					new TextSummaryResponse
				{
					Ask = repositoryRequest.Text,
					Length = int.Parse(result)
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

		public class WebsiteSummaryRequest
		{
			public string Url { get; set; } = string.Empty;
		}
		public class WebsiteSummaryResponse
		{
			public string Ask { get; set; } = string.Empty;
			public string Summary { get; set; } = string.Empty;
		}

		public class TextSummaryRequest
		{
			public string Text { get; set; } = string.Empty;
		}
		public class TextSummaryResponse
		{
			public string Ask { get; set; } = string.Empty;
			public int Length { get; set; } = 0;
		}
	}
}
