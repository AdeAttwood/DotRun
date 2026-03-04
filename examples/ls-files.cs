#:project ../src/DotRun/DotRun.csproj

using DotRun;
var builder = new DotRunBuilder();

builder
    .Task("GitFiles")
    .Run(async c =>
    {
        var output = await c.Shell("git", "ls-files -z");
        var files = output.Split("\0");

        if (!files.Any(f => f == "README.md"))
        {
            throw new Exception("File does not exist");
        }
    });

await builder.RunAsync();
