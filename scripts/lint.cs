#:project ../src/DotRun/DotRun.csproj

using DotRun;

var builder = new DotRunBuilder();

var restore = builder.RunCommand("Restore", "dotnet", "restore");

builder.RunCommand(
    "Format",
    "dotnet",
    "format  --verify-no-changes --no-restore"
)
.DependsOn(restore);

builder.RunCommand(
    "Build",
    "dotnet",
    "build /warnaserror --no-restore"
)
.DependsOn(restore);

await builder.RunAsync();
