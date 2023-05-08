using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.TemplateEngine;
using System.Reflection;

namespace assignment_1
{
	internal class KernelFactory
	{
		private static List<string> skillsToLoad = new List<string>();

		private static bool isInitialized = false;
		internal static void Initialize(string[] skillsToLoad)
		{
			KernelFactory.skillsToLoad.AddRange(skillsToLoad);
			isInitialized = true;
		}

		internal static IKernel CreateForRequest(
			string serviceDeploymentId,
			string serviceCompletionEndpoint,
			string serviceKey,
			string serviceModelName,
			ILogger logger)
		{
			if (!isInitialized)
			{
				throw new InvalidOperationException($"Builder needs to be initialized first using ${nameof(KernelFactory.Initialize)}.");
			}
			KernelBuilder builder = Kernel.Builder;
			builder = ConfigureKernelBuilder(
				serviceDeploymentId,
				serviceCompletionEndpoint,
				serviceKey,
				serviceModelName,
				builder);
			return CompleteKernelSetup(builder, logger);
		}

		private static KernelBuilder ConfigureKernelBuilder(
			string serviceDeploymentId,
			string serviceCompletionEndpoint,
			string serviceKey,
			string serviceModelName,
			KernelBuilder builder
			)
		{
			builder = builder
				.Configure(c =>
				{
					c.AddAzureTextCompletionService(
						serviceDeploymentId,
						serviceDeploymentId,
						serviceCompletionEndpoint,
						serviceKey);
				});

			return builder;
		}

		private static IKernel CompleteKernelSetup(
			KernelBuilder builder,
			ILogger logger)
		{
			IKernel kernel = builder.Build();

			RegisterSemanticSkills(kernel, SkillsPath(), logger);

			return kernel;
		}

		private static void RegisterSemanticSkills(
			IKernel kernel,
			string skillsFolder,
			ILogger logger)
		{
			foreach (string skPromptPath in Directory.EnumerateFiles(skillsFolder, "*.txt", SearchOption.AllDirectories))
			{
				FileInfo fInfo = new(skPromptPath);
				DirectoryInfo? currentFolder = fInfo.Directory;

				while (currentFolder?.Parent?.FullName != skillsFolder)
				{
					currentFolder = currentFolder?.Parent;
				}

				if (ShouldLoad(currentFolder.Name, skillsToLoad))
				{
					try
					{
						_ = kernel.ImportSemanticSkillFromDirectory(skillsFolder, currentFolder.Name);
					}
					catch (TemplateException e)
					{
						logger.LogWarning("Could not load skill from {0} with error: {1}", currentFolder.Name, e.Message);
					}
				}
			}
		}

		private static string SkillsPath()
		{
			string skillsPath = "Skills";

			bool SearchPath(string pathToFind, out string result, int maxAttempts = 10)
			{
				var currDir = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
				bool found;
				do
				{
					result = Path.Join(currDir, pathToFind);
					found = Directory.Exists(result);
					currDir = Path.GetFullPath(Path.Combine(currDir, ".."));
				} while (maxAttempts-- > 0 && !found);

				return found;
			}

			if (!SearchPath(skillsPath, out string path))
			{
				throw new ApplicationException("Skills directory not found.");
			}

			return path;
		}

		private static bool ShouldLoad(string skillName, IEnumerable<string> skillsToLoad)
		{
			return skillsToLoad.Contains(skillName, StringComparer.InvariantCultureIgnoreCase) != false;
		}
	}
}
