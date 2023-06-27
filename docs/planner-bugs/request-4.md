# Question

> "Create a summary from the above text."

The prompt also contains the nearest matching data.

Creates a plan with the wrong input parameter.

## Plan

```json
{
  "state": [{ "Key": "INPUT", "Value": "" }],
  "steps": [
    {
      "state": [{ "Key": "INPUT", "Value": "" }],
      "steps": [],
      "parameters": [{ "Key": "INPUT", "Value": "INPUT_TEXT" }],
      "outputs": ["TEXT_CONTENT"],
      "next_step_index": 0,
      "name": "ReadAsync",
      "skill_name": "_GLOBAL_FUNCTIONS_",
      "description": "Read a file"
    },
    {
      "state": [{ "Key": "INPUT", "Value": "" }],
      "steps": [],
      "parameters": [
        { "Key": "url", "Value": "" },
        { "Key": "INPUT", "Value": "$TEXT_CONTENT" }
      ],
      "outputs": ["RESULT__SUMMARY"],
      "next_step_index": 0,
      "name": "summarize",
      "skill_name": "website",
      "description": "Create a summary of a specified URL."
    }
  ],
  "parameters": [{ "Key": "INPUT", "Value": "" }],
  "outputs": ["RESULT__SUMMARY"],
  "next_step_index": 0,
  "name": "",
  "skill_name": "Microsoft.SemanticKernel.Planning.Plan",
  "description": "Are there any noteworthy things about Jan?\r\nHe\u0027s been a bartender for quite a few years, so can probably pour you a good drink. He has also been awarded with the title Microsoft Azure MVP for all the community work he did and still does.\r\nWhere does Jan work right now?\r\nAt this moment, Jan is working as a Software Engineer at Microsoft. Along with his team members, he\u0027s working on a new Copilot solution in the Manufacturing space.\r\nWhere did Jan work in the past?\r\nJan has been employed at a couple of companies in his professional career. He has started working at a company called Clockwork, which was a brand of Ordina. The focus was creating software for pocket pc\u0027s. He later moved on working at Leones, later rebranded to NetSourcing, where he worked on developing software for healthcare and got experience with SharePoint development. Jan also worked a small time at Get There, as a consultant, but moved on rather quickly to Macaw. At Macaw he has learned a lot on writing Clean Code, software and solution design, and much more. After quite a few years he moved to 4Dotnet, where he has worked for a long time getting a lot experience designing and implementing solutions with Microsoft Azure.\r\n\r\n\r\n\r\n\r\n\r\nCreate a summary from the above text.\r\n"
}
```
