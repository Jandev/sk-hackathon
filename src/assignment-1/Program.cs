using assignment_1.Summarize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using assignment_1;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.CoreSkills;
using System.Reflection;

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

				AddSemanticSkills();
				AddNativeSkills();

				return kernel;

				void AddSemanticSkills()
				{
					logger.LogInformation("Importing semantic skills");
					string skillsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Skills");
					kernel.ImportSemanticSkillFromDirectory(skillsPath, "website");
				}

				void AddNativeSkills()
				{
					logger.LogInformation("Importing native skills");
					kernel.ImportSkill(new FileIOSkill());
					kernel.ImportSkill(new TextSkill());
				}
			});
	})
	.ConfigureHostConfiguration(c =>
	{
		c.AddUserSecrets<Program>();
	})
	.Build();

host.Run();
