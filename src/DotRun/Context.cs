using System.Collections.Concurrent;
using System.Diagnostics;

namespace DotRun;

public class Context
{
    public ConcurrentDictionary<string, object> Data { get; } = new();

    public async Task<string> Shell(string command, string args)
    {
        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = args
            }
        };

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

        var output = (await outputTask).Trim();
        var error = (await errorTask).Trim();

        if (process.ExitCode != 0)
        {
            var stdoutLabel = string.IsNullOrWhiteSpace(output) ? "(EMPTY)" : output;
            var stderrLabel = string.IsNullOrWhiteSpace(error) ? "(EMPTY)" : error;

            throw new Exception($"""
            Command '{command} {args}' failed with exit code {process.ExitCode}

            STDOUT:

            {stdoutLabel}

            STDERR:

            {stderrLabel}
            """);
        }

        return output;
    }

    public void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }
}
