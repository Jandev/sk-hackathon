using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Orchestration;
using System;

namespace assignment_1.Summarize
{
	internal class Website : IInvoker<WebsiteRequest>
	{
		private readonly ILogger<Website> logger;
		private Settings.OpenAi openAiSettings;

		public Website(IOptions<Settings.OpenAi> options,
			ILogger<Website> logger)
		{
			this.openAiSettings = options.Value;
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
			
			var summarizeFunction = kernel.Skills.GetFunction(skill, summaryFunctionName);
			var contextVariables = new ContextVariables();
			contextVariables.Set("url", request.Url.ToString());

			var result = await kernel.RunAsync(contextVariables, summarizeFunction);

			if (result.ErrorOccurred)
			{
				this.logger.LogError(result.LastErrorDescription);
				throw new Exception(result.LastErrorDescription);
			}

			return result.Result.Trim();
		}
	}

	public record WebsiteRequest(Uri Url);
}
