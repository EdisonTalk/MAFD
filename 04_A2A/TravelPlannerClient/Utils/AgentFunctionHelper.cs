using A2A;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.RegularExpressions;

namespace TravelPlannerClient.Utils;

public class AgentFunctionHelper
{
    public static IEnumerable<AIFunction> CreateFunctionTools(AIAgent a2aAgent, AgentCard agentCard)
    {
        foreach (var skill in agentCard.Skills)
        {
            AIFunctionFactoryOptions options = new()
            {
                Name = Sanitize(skill.Id),
                Description = $$"""
                {
                    "description": "{{skill.Description}}",
                    "tags": "[{{string.Join(", ", skill.Tags ?? [])}}]",
                    "examples": "[{{string.Join(", ", skill.Examples ?? [])}}]",
                    "inputModes": "[{{string.Join(", ", skill.InputModes ?? [])}}]",
                    "outputModes": "[{{string.Join(", ", skill.OutputModes ?? [])}}]"
                }
                """,
            };

            yield return AIFunctionFactory.Create(RunAgentAsync, options);
        }

        async Task<string> RunAgentAsync(string input, CancellationToken cancellationToken)
        {
            var response = await a2aAgent.RunAsync(input, cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Text;
        }
    }

    private static readonly Regex InvalidNameCharsRegex = new Regex("[^0-9A-Za-z]+", RegexOptions.Compiled);

    public static string Sanitize(string name)
    {
        return InvalidNameCharsRegex.Replace(name, "_");
    }
}
