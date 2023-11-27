using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace assignment_1.Skills.my_skills
{
	/// <summary>
	/// Skill doing text operations.
	/// </summary>
	public class TextSkill
	{
		[SKFunction, Description("Get the length of a specified text.")]
		public Task<string> Length(
			[Description("The text to calculate the length from.")]
			string input)
		{
			return Task.FromResult(
				JsonSerializer.Serialize(input.Length));
		}
	}
}
