using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.Text.RegularExpressions;

namespace assignment_1.Skills.my_skills
{
	/// <summary>
	/// Skill site actions
	/// </summary>
	internal class DownloadContent
	{
		private readonly IHttpClientFactory httpClientFactory;

		public DownloadContent(IHttpClientFactory httpClientFactory)
		{
			this.httpClientFactory = httpClientFactory;
		}

		[SKFunction("Retrieve the body from a specified website URL.")]
		[SKFunctionName("GetBody")]
		[SKFunctionInput(Description = "Retrieves the content of the body from an HTML page.")]
		public async Task<string> GetBody(string input, SKContext context)
		{
			if (string.IsNullOrEmpty(input))
			{
				context.Fail("No input specified to download the body from.");
			}

			var content = await GetContent(input);

			return content;

		}

		private async Task<string> GetContent(string url)
		{
			var content = await ReadContent(url);
			Match match = Regex.Match(content, "<body[^>]*>(.*?)</body>", RegexOptions.Singleline);
			string strippedContent;
			if (match.Success)
			{
				string foundContent = match.Groups[1].Value;
				strippedContent = foundContent.Substring(0, foundContent.Length > 1800 ? 1800 : foundContent.Length);
			}
			else
			{
				throw new Exception();
			}

			return strippedContent;
		}

		private async Task<string> ReadContent(string url)
		{
			using var client = this.httpClientFactory.CreateClient();
			var response = await client.GetAsync(url);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStringAsync();
			}
			else
			{
				throw new Exception($"Failed to download content from {url}. Status code: {response.StatusCode}");
			}
		}
	}
}
