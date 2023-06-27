# Question

> "Based on solely the above information. Please write a summary in a polite way."

The prompt also contains the nearest matching data.
It tries to read and write files.

## Plan

```json
{
  "state": [{ "Key": "INPUT", "Value": "" }],
  "steps": [
    {
      "state": [{ "Key": "INPUT", "Value": "" }],
      "steps": [],
      "parameters": [{ "Key": "INPUT", "Value": "Jan\u0027s Information" }],
      "outputs": ["INPUT_TEXT"],
      "next_step_index": 0,
      "name": "ReadAsync",
      "skill_name": "_GLOBAL_FUNCTIONS_",
      "description": "Read a file"
    },
    {
      "state": [{ "Key": "INPUT", "Value": "" }],
      "steps": [],
      "parameters": [
        { "Key": "url", "Value": "$INPUT_TEXT" },
        { "Key": "INPUT", "Value": "" }
      ],
      "outputs": ["SUMMARY_TEXT"],
      "next_step_index": 0,
      "name": "summarize",
      "skill_name": "website",
      "description": "Create a summary of a specified URL."
    },
    {
      "state": [{ "Key": "INPUT", "Value": "" }],
      "steps": [],
      "parameters": [{ "Key": "INPUT", "Value": "$SUMMARY_TEXT" }],
      "outputs": ["TRIMMED_TEXT"],
      "next_step_index": 0,
      "name": "TrimStart",
      "skill_name": "_GLOBAL_FUNCTIONS_",
      "description": "Trim whitespace from the start of a string."
    },
    {
      "state": [{ "Key": "INPUT", "Value": "" }],
      "steps": [],
      "parameters": [
        { "Key": "input2", "Value": "$TRIMMED_TEXT" },
        {
          "Key": "INPUT",
          "Value": "Jan is a Software Engineer at Microsoft, working on a new Copilot solution in the Manufacturing space. He has been a bartender for quite a few years and has been awarded with the title Microsoft Azure MVP for all the community work he did and still does. He has also worked at a couple of companies in his professional career, such as Clockwork, Leones, NetSourcing, Get There, Macaw and 4Dotnet, where he has gained a lot of experience in software and solution design."
        }
      ],
      "outputs": ["FINAL_TEXT"],
      "next_step_index": 0,
      "name": "Concat",
      "skill_name": "_GLOBAL_FUNCTIONS_",
      "description": "Concat two strings into one."
    },
    {
      "state": [{ "Key": "INPUT", "Value": "" }],
      "steps": [],
      "parameters": [
        { "Key": "path", "Value": "Summary.txt" },
        { "Key": "content", "Value": "$FINAL_TEXT" },
        { "Key": "INPUT", "Value": "" }
      ],
      "outputs": [],
      "next_step_index": 0,
      "name": "WriteAsync",
      "skill_name": "_GLOBAL_FUNCTIONS_",
      "description": "Write a file"
    }
  ],
  "parameters": [{ "Key": "INPUT", "Value": "" }],
  "outputs": [],
  "next_step_index": 0,
  "name": "",
  "skill_name": "Microsoft.SemanticKernel.Planning.Plan",
  "description": "Are there any noteworthy things about Jan?\r\nHe\u0027s been a bartender for quite a few years, so can probably pour you a good drink. He has also been awarded with the title Microsoft Azure MVP for all the community work he did and still does.\r\nWhere does Jan work right now?\r\nAt this moment, Jan is working as a Software Engineer at Microsoft. Along with his team members, he\u0027s working on a new Copilot solution in the Manufacturing space.\r\nWhere did Jan work in the past?\r\nJan has been employed at a couple of companies in his professional career. He has started working at a company called Clockwork, which was a brand of Ordina. The focus was creating software for pocket pc\u0027s. He later moved on working at Leones, later rebranded to NetSourcing, where he worked on developing software for healthcare and got experience with SharePoint development. Jan also worked a small time at Get There, as a consultant, but moved on rather quickly to Macaw. At Macaw he has learned a lot on writing Clean Code, software and solution design, and much more. After quite a few years he moved to 4Dotnet, where he has worked for a long time getting a lot experience designing and implementing solutions with Microsoft Azure.\r\n\r\n\r\n\r\n\r\nBased on solely the above information. Please write a summary in a polite way.\r\n"
}
```
