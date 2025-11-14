using System.Collections.Concurrent;
using System.Diagnostics;

namespace DotRun;

public class Context
{
    public ConcurrentDictionary<string, object> Data { get; } = new();

    public Task<string> Shell(string command, string args)
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
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"""
            Command '{command} {args}' failed with exit code {process.ExitCode}

            STDOUT:

            {process.StandardOutput.ReadToEnd().Trim() ?? "(EMPTY)"}

            STDERR:

            {process.StandardError.ReadToEnd().Trim() ?? "(EMPTY)"}
            """);
        }

        return Task.FromResult(process.StandardOutput.ReadToEnd().Trim());
    }

    public void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }
}
