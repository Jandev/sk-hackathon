using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Orchestration;
using System.Text.RegularExpressions;

namespace assignment_1.Summarize
{
	internal class Website : IInvoker<WebsiteRequest>
	{
		private readonly IHttpClientFactory httpClientFactory;
		private readonly ILogger<Website> logger;
		private Settings.OpenAi openAiSettings;

		public Website(
			IHttpClientFactory httpClientFactory,
			IOptions<Settings.OpenAi> options,
			ILogger<Website> logger)
		{
			this.openAiSettings = options.Value;
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		public async Task<string> Invoke(WebsiteRequest request)
		{
			const string skill = "website";
			const string summaryFunctionName = "summarize";

			KernelFactory.Initialize(new[] { skill });
			var kernel = KernelFactory.CreateForRequest(
				openAiSettings.ServiceDeploymentId,
				openAiSettings.ServiceCompletionEndpoint,
				openAiSettings.ServiceKey,
				openAiSettings.ServiceModelName,
				logger);
			string strippedContent;

			bool beingOldFasioned = true;
			if (beingOldFasioned)
			{
				strippedContent = await GetContentViaRegularCodeFlow(request);
			}
			else
			{
				strippedContent = await GetContentViaModernWays(request, kernel);
			}

			var summarizeFunction = kernel.Skills.GetFunction(skill, summaryFunctionName);
			var contextVariables = new ContextVariables();
			contextVariables.Set("input", strippedContent);

			var result = await kernel.RunAsync(contextVariables, summarizeFunction);

			if (result.ErrorOccurred)
			{
				this.logger.LogError(result.LastErrorDescription);
				throw new Exception(result.LastErrorDescription);
			}

			return result.Result.Trim();
		}

		private async Task<string> GetContentViaRegularCodeFlow(WebsiteRequest request)
		{
			var content = await DownloadContent(request.Url.ToString());
			Match match = Regex.Match(content, "<body[^>]*>(.*?)</body>", RegexOptions.Singleline);
			string strippedContent;
			if (match.Success)
			{
				string foundContent = match.Groups[1].Value;
				strippedContent = foundContent.Substring(0, foundContent.Length > 1800 ? 1800 : foundContent.Length);
			}
			else
			{
				throw new Exception();
			}

			return strippedContent;
		}

		private async Task<string> DownloadContent(string url)
		{
			using var client = this.httpClientFactory.CreateClient();
			var response = await client.GetAsync(url);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStringAsync();
			}
			else
			{
				throw new Exception($"Failed to download content from {url}. Status code: {response.StatusCode}");
			}
		}

		private async Task<string> GetContentViaModernWays(WebsiteRequest request, Microsoft.SemanticKernel.IKernel kernel)
		{
			const string siteSkill = "SiteContentSkill";
			const string getBodyFunctionName = "GetBody";
			kernel.ImportSkill(new Skills.my_skills.DownloadContent(this.httpClientFactory), siteSkill);
			var getBodyFunction = kernel.Skills.GetFunction(
					siteSkill,
					getBodyFunctionName);

			var getBodyContextVariables = new ContextVariables(request.Url.ToString());
			var getBodyContext = await kernel.RunAsync(getBodyContextVariables, getBodyFunction);
			var strippedContent = getBodyContext.Result.Trim();
			return strippedContent;
		}
	}

	public record WebsiteRequest(Uri Url);
}
