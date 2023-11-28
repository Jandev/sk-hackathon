using Microsoft.SemanticKernel;
using System.ComponentModel;
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

		[SKFunction, Description("Retrieve the body from a specified website URL.")]
		public async Task<string> GetBody(
			[Description("The URL to of the website to download the body from.")]
			string urlToLoad)
		{
			var content = await GetContent(urlToLoad);

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
				return "No information found.";
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
