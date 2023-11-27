using assignment_1.Summarize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using assignment_1;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using Azure.AI.OpenAI;
using Azure;
using Azure.Search.Documents.Indexes;
using assignment_1.Skills.my_skills;
using Microsoft.SemanticKernel.Plugins.Core;

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(s =>
	{
		AddOptions(s);

		s.AddTransient<IInvoker<WebsiteRequest>, Website>();
		s.AddTransient<IInvoker<TextRequest>, Text>();
		s.AddTransient<IInvoker<NaturalLanguageQueryRequest>, NaturalLanguageQuery>();

		RegisterAIServices(s);
	})
	.ConfigureHostConfiguration(c =>
	{
		c.AddUserSecrets<Program>();
	})
	.Build();

host.Run();

static void AddOptions(IServiceCollection s)
{
	s.AddOptions<Settings.OpenAi>()
				.Configure<IConfiguration>((settings, configuration) =>
				{
					configuration.GetSection(nameof(Settings.OpenAi)).Bind(settings);
				});
	s.AddOptions<Settings.CognitiveSearch>()
		.Configure<IConfiguration>((settings, configuration) =>
		{
			configuration.GetSection(nameof(Settings.CognitiveSearch)).Bind(settings);
		});
}

static void RegisterAIServices(IServiceCollection s)
{
	s.AddSingleton(
		typeof(IKernel),
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

			AddSemanticSkills();
			AddNativeSkills();

			return kernel;

			void AddSemanticSkills()
			{
				logger.LogInformation("Importing semantic skills");
				string skillsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Skills");
				kernel.ImportSemanticFunctionsFromDirectory(skillsPath, "website");
			}

			void AddNativeSkills()
			{
				logger.LogInformation("Importing native skills");
				kernel.ImportFunctions(new FileIOPlugin());
				kernel.ImportFunctions(new Microsoft.SemanticKernel.Plugins.Core.TextPlugin());
				kernel.ImportFunctions(new DownloadContent(s.GetRequiredService<IHttpClientFactory>()), "MySkills");
			}
		});
	s.AddSingleton(
		typeof(OpenAIClient),
		s =>
		{
			var openAiOptions = s.GetRequiredService<IOptions<Settings.OpenAi>>();
			var openAiSettings = openAiOptions.Value;
			var credential = new AzureKeyCredential(openAiSettings.ServiceKey);
			var openAIClient = new OpenAIClient(new Uri(openAiSettings.ServiceCompletionEndpoint), credential);
			return openAIClient;
		});
	s.AddSingleton(
		typeof(SearchIndexClient),
		s =>
		{
			var cognitiveSearchOptions = s.GetRequiredService<IOptions<Settings.CognitiveSearch>>();
			var cognitiveSearchSettings = cognitiveSearchOptions.Value;
			var searchCredential = new AzureKeyCredential(cognitiveSearchSettings.Key);
			var indexClient = new SearchIndexClient(
				new Uri(cognitiveSearchSettings.Endpoint),
				searchCredential);
			return indexClient;
		});
	s.AddTransient<assignment_1.Embeddings.Index>();
	s.AddHttpClient();
}