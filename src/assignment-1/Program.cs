using assignment_1.Summarize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using assignment_1;
using Microsoft.Extensions.Configuration;

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
	})
	.ConfigureHostConfiguration(c =>
	{
		c.AddUserSecrets<Program>();
	})
	.Build();

host.Run();
