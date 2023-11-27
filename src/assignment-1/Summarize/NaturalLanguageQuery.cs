using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;

namespace assignment_1.Summarize
{
	internal class NaturalLanguageQuery : IInvoker<NaturalLanguageQueryRequest>
	{
		private readonly IKernel kernel;
		private readonly ILogger<NaturalLanguageQuery> logger;

		public NaturalLanguageQuery(
			IKernel kernel,
			ILogger<NaturalLanguageQuery> logger
			)
		{
			this.kernel = kernel;
			this.logger = logger;
		}

		public async Task<string> Invoke(
			NaturalLanguageQueryRequest request,
			CancellationToken cancellationToken)
		{
			var configuration = new SequentialPlannerConfig();
			// Remove the functions to read/write files, located in the `_GLOBAL_SKILLS_`.
			configuration.ExcludedFunctions.Add("Write");
			configuration.ExcludedFunctions.Add("Read");
			var planner = new SequentialPlanner(kernel, configuration);

			var plan = await planner.CreatePlanAsync(request.Query, cancellationToken);
			this.logger.LogInformation("Original plan: {plan}", plan.ToJson());

			var executedPlan = await kernel.RunAsync(plan);
			return executedPlan.GetValue<string>() ?? string.Empty;
		}
	}

	public class NaturalLanguageQueryRequest
	{
		public string Query { get; set; } = string.Empty;
	}
}
