using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;
using System.Diagnostics;

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

		public async Task<string> Invoke(NaturalLanguageQueryRequest request)
		{
			var configuration = new SequentialPlannerConfig();
			var planner = new SequentialPlanner(kernel, configuration);

			var plan = await planner.CreatePlanAsync(request.Query);
			this.logger.LogInformation("Original plan: {plan}", plan.ToJson());

			var executedPlan = await ExecutePlanAsync(kernel, plan, request.Query);

			logger.LogDebug(plan.ToJson());

			return string.Join(",", executedPlan.Outputs);
		}

		private async Task<Plan> ExecutePlanAsync(
			IKernel kernel,
			Plan plan,
			string input = "",
			int maxSteps = 10)
		{
			Stopwatch sw = new();
			sw.Start();

			// loop until complete or at most N steps
			try
			{
				for (int step = 1; plan.HasNextStep && step < maxSteps; step++)
				{
					if (string.IsNullOrEmpty(input))
					{
						await plan.InvokeNextStepAsync(kernel.CreateNewContext());
						// or await kernel.StepAsync(plan);
					}
					else
					{
						plan = await kernel.StepAsync(input, plan);
					}

					if (!plan.HasNextStep)
					{
						this.logger.LogDebug($"Step {step} - COMPLETE!");
						this.logger.LogDebug(plan.State.ToString());
						break;
					}

					this.logger.LogDebug($"Step {step} - Results so far:");
					this.logger.LogDebug(plan.State.ToString());
				}
			}
			catch (KernelException e)
			{
				this.logger.LogDebug("Step - Execution failed:");
				this.logger.LogDebug(e.Message);
			}

			sw.Stop();
			this.logger.LogDebug($"Execution complete in {sw.ElapsedMilliseconds} ms!");
			return plan;
		}
	}

	public class NaturalLanguageQueryRequest
	{
		public string Query { get; set; } = string.Empty;
	}
}
