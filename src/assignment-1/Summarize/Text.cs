using assignment_1.Skills.my_skills;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
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

		public async Task<string> Invoke(
			TextRequest request,
			CancellationToken cancellationToken)
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
			kernel.ImportFunctions(new TextSkill(), "TextSkill");
			
			var lengthFunction = kernel.Functions.GetFunction(
				skill, 
				lengthFunctionName);
			var contextVariables = new ContextVariables(request.Text);

			var result = await kernel.RunAsync(contextVariables, cancellationToken, lengthFunction);
			return result.GetValue<string>();
		}
	}

	public record TextRequest(string Text);
}
