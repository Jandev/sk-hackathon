using assignment_1.Summarize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using assignment_1;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(s =>
	{
		s.AddOptions<Settings.OpenAi>()
			.Configure<IConfiguration>((settings, configuration) =>
			{
				configuration.GetSection(nameof(Settings.OpenAi)).Bind(settings);
			});
		s.AddTransient<IInvoker<WebsiteRequest>, Website>();
		s.AddTransient<IInvoker<TextRequest>, Text>();
		s.AddTransient<IInvoker<NaturalLanguageQueryRequest>, NaturalLanguageQuery>();

		s.AddSingleton(typeof(IKernel),
			s =>
			{
				var openAiOptions = s.GetRequiredService<IOptions<Settings.OpenAi>>();
				var logger = s.GetRequiredService<ILogger<IKernel>>();
				var openAiSettings = openAiOptions.Value;

				var skillCollectionToLoad = new string[] { "website" };
				KernelFactory.Initialize(skillCollectionToLoad);
				var kernel = KernelFactory.CreateForRequest(
					openAiSettings.ServiceDeploymentId,
					openAiSettings.ServiceCompletionEndpoint,
					openAiSettings.ServiceKey,
					openAiSettings.ServiceModelName,
					logger);

				return kernel;
			});
	})
	.ConfigureHostConfiguration(c =>
	{
		c.AddUserSecrets<Program>();
	})
	.Build();

host.Run();
