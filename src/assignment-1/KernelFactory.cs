using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Reflection;

namespace assignment_1
{
	internal class KernelFactory
	{
		private static List<string> skillsToLoad = new List<string>();

		internal static void Initialize(string[] skillsToLoad)
		{
			KernelFactory.skillsToLoad.AddRange(skillsToLoad);
		}

		internal static IKernel CreateForRequest(
			string serviceDeploymentId,
			string serviceCompletionEndpoint,
			string serviceKey,
			string serviceModelName,
			ILogger logger)
		{
			IKernel kernel = new KernelBuilder()
				.WithAzureTextCompletionService(
					serviceDeploymentId,
						serviceCompletionEndpoint,
						serviceKey,
						serviceId: serviceModelName)
				.WithAzureOpenAIChatCompletionService(
					serviceDeploymentId,
					serviceCompletionEndpoint,
					serviceKey,
					serviceId: serviceModelName)
				.Build();

			RegisterSemanticSkills(kernel, SkillsPath());
			return kernel;
		}

		private static void RegisterSemanticSkills(
			IKernel kernel,
			string skillsFolder)
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
					_ = kernel.ImportSemanticFunctionsFromDirectory(skillsFolder, currentFolder.Name);
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
