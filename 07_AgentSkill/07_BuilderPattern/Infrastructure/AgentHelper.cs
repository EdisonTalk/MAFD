using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace AgentSkillDemo.Infrastructure;

internal class AgentHelper
{
    public static async Task<AIContext> GetAgentSkillsContextAsync(IChatClient chatClient, AgentSkillsProvider provider)
    {
        // 创建一个临时 Agent 来获取技能上下文
        var agent = chatClient
            .AsAIAgent();
        var inputContext = new AIContext
        {
            Instructions = "你是一个专业的跨境物流运营助手，负责帮助用户处理跨境物流相关的事务。"
        };

        var invokingContext = new AIContextProvider.InvokingContext(agent, session: null, inputContext);

        var providedContext = await provider.InvokingAsync(invokingContext);

        return providedContext;
    }

    /// <summary>
    /// Runs a skill script as a local subprocess.
    /// </summary>
    public static async Task<object?> RunScriptInSubProcessAsync(
        AgentFileSkill skill,
        AgentFileSkillScript script,
        AIFunctionArguments arguments,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(script.FullPath))
        {
            return $"Error: Script file not found: {script.FullPath}";
        }

        string extension = Path.GetExtension(script.FullPath);
        string? interpreter = extension switch
        {
            ".py" => "python3",
            ".js" => "node",
            ".sh" => "bash",
            ".ps1" => "pwsh",
            _ => null,
        };

        var startInfo = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(script.FullPath) ?? ".",
        };

        if (interpreter is not null)
        {
            startInfo.FileName = interpreter;
            startInfo.ArgumentList.Add(script.FullPath);
        }
        else
        {
            startInfo.FileName = script.FullPath;
        }

        if (arguments is not null)
        {
            foreach (var (key, value) in arguments)
            {
                if (value is bool boolValue)
                {
                    if (boolValue)
                    {
                        startInfo.ArgumentList.Add(NormalizeKey(key));
                    }
                }
                else if (value is not null)
                {
                    startInfo.ArgumentList.Add(NormalizeKey(key));
                    startInfo.ArgumentList.Add(value.ToString()!);
                }
            }
        }

        Process? process = null;
        try
        {
            process = Process.Start(startInfo);
            if (process is null)
            {
                return $"Error: Failed to start process for script '{script.Name}'.";
            }

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            string output = await outputTask.ConfigureAwait(false);
            string error = await errorTask.ConfigureAwait(false);

            if (!string.IsNullOrEmpty(error))
            {
                output += $"\nStderr:\n{error}";
            }

            if (process.ExitCode != 0)
            {
                output += $"\nScript exited with code {process.ExitCode}";
            }

            return string.IsNullOrEmpty(output) ? "(no output)" : output.Trim();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Kill the process on cancellation to avoid leaving orphaned subprocesses.
            process?.Kill(entireProcessTree: true);
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return $"Error: Failed to execute script '{script.Name}': {ex.Message}";
        }
        finally
        {
            process?.Dispose();
        }
    }

    /// <summary>
    /// Normalizes a parameter key to a consistent --flag format.
    /// Models may return keys with or without leading dashes (e.g., "value" vs "--value").
    /// </summary>
    private static string NormalizeKey(string key) => "--" + key.TrimStart('-');
}