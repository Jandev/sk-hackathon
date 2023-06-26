using System.Net;
using System.Text;
using System.Text.Json;
using assignment_1.Summarize;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using static Google.Apis.CustomSearchAPI.v1.Data.Search.QueriesData;

namespace assignment_1
{
	public class WebTriggers
	{
		private readonly ILogger _logger;
		private readonly IInvoker<WebsiteRequest> websiteSummaryInvoker;
		private readonly IInvoker<Summarize.TextRequest> textInvoker;
		private readonly IInvoker<NaturalLanguageQueryRequest> naturalLangaugeQueryInvoker;
		private readonly Embeddings.Index index;

		public WebTriggers(
			IInvoker<WebsiteRequest> websiteSummaryInvoker,
			IInvoker<Summarize.TextRequest> repositorySummaryInvoker,
			IInvoker<Summarize.NaturalLanguageQueryRequest> naturalLanguageQueryInvoker,
			assignment_1.Embeddings.Index index,
			ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<WebTriggers>();
			this.websiteSummaryInvoker = websiteSummaryInvoker;
			this.textInvoker = repositorySummaryInvoker;
			this.naturalLangaugeQueryInvoker = naturalLanguageQueryInvoker;
			this.index = index;
		}

		[Function(nameof(Summarize))]
		public async Task<HttpResponseData> Summarize(
			[HttpTrigger(AuthorizationLevel.Function, "post")]
			HttpRequestData req,
			CancellationToken hostCancellationToken)
		{
			using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken);

			var data = await GetRequestData<WebsiteSummaryRequest>(req, cancellationSource);
			if (data == null || string.IsNullOrWhiteSpace(data.Url))
			{
				throw new ArgumentException(nameof(data), "Input not in the correct format.");
			}
			var websiteRequest = new WebsiteRequest(new Uri(data.Url));
			try
			{
				var summary = await this.websiteSummaryInvoker.Invoke(websiteRequest);

				var response = new WebsiteSummaryResponse
				{
					Ask = websiteRequest.Url.ToString(),
					Summary = summary
				};
				return await CreateValidResponse(req, response);
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

			var data = await GetRequestData<TextSummaryRequest>(req, cancellationSource);
			if (data == null || string.IsNullOrWhiteSpace(data.Text))
			{
				throw new ArgumentException(nameof(data), "Input not in the correct format.");
			}
			var repositoryRequest = new TextRequest(data.Text);
			try
			{
				var result = await this.textInvoker.Invoke(repositoryRequest);

				var response = new TextSummaryResponse
				{
					Ask = repositoryRequest.Text,
					Length = int.Parse(result)
				};
				return await CreateValidResponse(req, response);
			}
			catch (Exception ex)
			{
				var response = req.CreateResponse(HttpStatusCode.InternalServerError);
				response.WriteString(ex.Message);
				return response;
			}
		}

		[Function(nameof(Query))]
		public async Task<HttpResponseData> Query(
			[HttpTrigger(AuthorizationLevel.Function, "post")]
			HttpRequestData requestData,
			CancellationToken cancellationToken
			)
		{
			using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			var data = await GetRequestData<QueryRequest>(requestData, cancellationSource);
			if (data == null || string.IsNullOrWhiteSpace(data.Ask))
			{
				throw new ArgumentException(nameof(data), "Input not in the correct format.");
			}

			try
			{
				var request = new NaturalLanguageQueryRequest { Query = data.Ask };
				var result = await this.naturalLangaugeQueryInvoker.Invoke(request);
				var responseObject = new QueryResponse
				{
					Response = result,
				};

				return await CreateValidResponse(requestData, result);
			}
			catch (Exception ex)
			{
				var response = requestData.CreateResponse(HttpStatusCode.InternalServerError);
				response.WriteString(ex.Message);
				return response;
			}
		}

		[Function(nameof(CreateEmbeddingsIndex))]
		public async Task<HttpResponseData> CreateEmbeddingsIndex(
			[HttpTrigger(AuthorizationLevel.Function, "post")]
			HttpRequestData requestData,
			CancellationToken cancellationToken
			)
		{
			using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			try
			{
				await this.index.Build();

				return await CreateValidResponse(requestData, "Created");
			}
			catch (Exception ex)
			{
				var response = requestData.CreateResponse(HttpStatusCode.InternalServerError);
				response.WriteString(ex.Message);
				return response;
			}
		}

		[Function(nameof(SearchEmbeddings))]
		public async Task<HttpResponseData> SearchEmbeddings(
			[HttpTrigger(AuthorizationLevel.Function, "post")]
			HttpRequestData requestData,
			CancellationToken cancellationToken
			)
		{
			using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			var data = await GetRequestData<QueryRequest>(requestData, cancellationSource);
			if (data == null || string.IsNullOrWhiteSpace(data.Ask))
			{
				throw new ArgumentException(nameof(data), "Input not in the correct format.");
			}

			try
			{
				var nearestContentCollection = await this.index.GetNearestContent(data.Ask);

				// Create the prompt.
				var sb = new StringBuilder();
				sb.AppendLine($"In the below blocks is additional information of the request '{data.Ask}'.");
				sb.AppendLine("---");
				foreach ( var item in nearestContentCollection)
				{
					sb.AppendLine(item.Query);
					sb.AppendLine(item.Content);
					sb.AppendLine("---");
				}
				sb.AppendLine();
				sb.AppendLine("Create a concise text for the above request and additional information.");
				var response = await this.naturalLangaugeQueryInvoker.Invoke(new NaturalLanguageQueryRequest { Query = sb.ToString() });

				return await CreateValidResponse(requestData, response);
			}
			catch (Exception ex)
			{
				var response = requestData.CreateResponse(HttpStatusCode.InternalServerError);
				response.WriteString(ex.Message);
				return response;
			}
		}


		private static async Task<HttpResponseData> CreateValidResponse<TResponse>(HttpRequestData requestData, TResponse responseObject)
		{
			var response = requestData.CreateResponse(HttpStatusCode.OK);
			await response.WriteAsJsonAsync(responseObject);
			return response;
		}

		private static async Task<TRequest> GetRequestData<TRequest>(HttpRequestData requestData, CancellationTokenSource cancellationSource)
			where TRequest : class, new()
		{
			var request = await JsonSerializer.DeserializeAsync<TRequest>(
							new StreamReader(requestData.Body).BaseStream,
							options: new JsonSerializerOptions
							{
								PropertyNamingPolicy = JsonNamingPolicy.CamelCase
							},
							cancellationToken: cancellationSource.Token)
				?? new TRequest();

			return request;
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

		public class QueryRequest
		{
			public string Ask { get; set; } = string.Empty;
		}
		public class QueryResponse
		{
			public string Response { get; set; } = string.Empty;
		}
	}
}
