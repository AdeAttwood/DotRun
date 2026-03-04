namespace DotRun.Test;

public class ContextTest
{
    [Fact]
    public async Task Shell_CapturesLargeStdout()
    {
        var context = new Context();
        var (command, args, expected) = BuildLargeStdoutCommand();

        var output = await context.Shell(command, args);

        Assert.Contains(expected, output);
    }

    [Fact]
    public async Task Shell_CapturesLargeStderrOnFailure()
    {
        var context = new Context();
        var (command, args, expected) = BuildLargeStderrCommand();

        var ex = await Assert.ThrowsAsync<Exception>(() => context.Shell(command, args));

        Assert.Contains(expected, ex.Message);
    }

    [Fact]
    public async Task Shell_CapturesStdoutAndStderrTogether()
    {
        var context = new Context();
        var (command, args, stdoutExpected, stderrExpected) = BuildLargeBothStreamsCommand();

        var ex = await Assert.ThrowsAsync<Exception>(() => context.Shell(command, args));

        Assert.Contains(stdoutExpected, ex.Message);
        Assert.Contains(stderrExpected, ex.Message);
    }

    private static (string command, string args, string expected) BuildLargeStdoutCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            return (
                "powershell",
                "-NoProfile -Command \"1..5000 | ForEach-Object { Write-Output ('stdout-line-' + $_) }\"",
                "stdout-line-5000");
        }

        return (
            "bash",
            "-c \"for i in $(seq 1 5000); do echo stdout-line-$i; done\"",
            "stdout-line-5000");
    }

    private static (string command, string args, string expected) BuildLargeStderrCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            return (
                "powershell",
                "-NoProfile -Command \"1..5000 | ForEach-Object { [Console]::Error.WriteLine('stderr-line-' + $_) }; exit 1\"",
                "stderr-line-5000");
        }

        return (
            "bash",
            "-c \"for i in $(seq 1 5000); do echo stderr-line-$i 1>&2; done; exit 1\"",
            "stderr-line-5000");
    }

    private static (string command, string args, string stdoutExpected, string stderrExpected)
        BuildLargeBothStreamsCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            return (
                "powershell",
                "-NoProfile -Command \"1..5000 | ForEach-Object { Write-Output ('stdout-line-' + $_) }; 1..5000 | ForEach-Object { [Console]::Error.WriteLine('stderr-line-' + $_) }; exit 1\"",
                "stdout-line-5000",
                "stderr-line-5000");
        }

        return (
            "bash",
            "-c \"for i in $(seq 1 5000); do echo stdout-line-$i; done; for i in $(seq 1 5000); do echo stderr-line-$i 1>&2; done; exit 1\"",
            "stdout-line-5000",
            "stderr-line-5000");
    }
}
