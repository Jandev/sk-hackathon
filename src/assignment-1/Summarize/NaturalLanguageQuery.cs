using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planners.Handlebars;
using System.Text.Json;

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
			//var configuration = new SequentialPlannerConfig();
			// Remove the functions to read/write files, located in the `_GLOBAL_SKILLS_`.
			//configuration.ExcludedFunctions.Add("Write");
			//configuration.ExcludedFunctions.Add("Read");
			//var planner = new SequentialPlanner(kernel, configuration);
			var configuration = new HandlebarsPlannerConfig();
			configuration.ExcludedFunctions.Add("Write");
			configuration.ExcludedFunctions.Add("Read");
			var planner = new HandlebarsPlanner(kernel, configuration);

			var plan = await planner.CreatePlanAsync(request.Query, cancellationToken);
			this.logger.LogInformation("Original plan: {plan}", plan);

			var result = plan.Invoke(kernel.CreateNewContext(), new Dictionary<string, object?>(), cancellationToken);
			return result.GetValue<string>();
			//var executedPlan = await kernel.RunAsync(/*request.Query*/ctx, cancellationToken, plan);
			//return executedPlan.GetValue<string>() ?? string.Empty;
		}
	}

	public class NaturalLanguageQueryRequest
	{
		public string Query { get; set; } = string.Empty;
	}
}
