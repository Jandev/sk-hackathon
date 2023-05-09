using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.Text.Json;

namespace assignment_1.Skills.my_skills
{
	/// <summary>
	/// Skill doing text operations.
	/// </summary>
	public class TextSkill
	{
		[SKFunction("Get the length of a specified text.")]
		[SKFunctionName("Length")]
		[SKFunctionInput(Description = "The text to get the length of.")]
		public Task<string> Length(string input, SKContext context)
		{
			if(string.IsNullOrEmpty(input))
			{
				context.Fail("No text to get the length of.");
			}

			return Task.FromResult(
				JsonSerializer.Serialize(input.Length));
		}
	}
}
