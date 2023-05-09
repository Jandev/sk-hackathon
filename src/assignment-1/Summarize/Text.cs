using assignment_1.Skills.my_skills;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Orchestration;

namespace assignment_1.Summarize
{
	internal class Text : IInvoker<TextRequest>
	{
		private Settings.OpenAi openAiSettings;
		private ILogger<Text> logger;

		public Text(
			IOptions<Settings.OpenAi> options,
			ILogger<Text> logger)
		{
			this.openAiSettings = options.Value;
			this.logger = logger;
		}

		public async Task<string> Invoke(TextRequest request)
		{
			const string skill = "TextSkill";
			const string lengthFunctionName = "Length";

			KernelFactory.Initialize(new[] { skill });
			var kernel = KernelFactory.CreateForRequest(
				openAiSettings.ServiceDeploymentId,
				openAiSettings.ServiceCompletionEndpoint,
				openAiSettings.ServiceKey,
				openAiSettings.ServiceModelName,
				logger);
			kernel.ImportSkill(new TextSkill(), "TextSkill");
			
			var lengthFunction = kernel.Skills.GetFunction(
				skill, 
				lengthFunctionName);
			var contextVariables = new ContextVariables(request.Text);

			var result = await kernel.RunAsync(contextVariables, lengthFunction);

			if (result.ErrorOccurred)
			{
				this.logger.LogError(result.LastErrorDescription);
				throw new Exception(result.LastErrorDescription);
			}

			return result.Result.Trim();
		}
	}

	public record TextRequest(string Text);
}
