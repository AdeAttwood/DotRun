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

var build = builder.RunCommand(
    "Build",
    "dotnet",
    "build /warnaserror --no-restore"
)
.DependsOn(restore);

builder.RunCommand(
    "Test",
    "dotnet",
    "test --no-build --no-restore"
)
.DependsOn(build);

await builder.RunAsync();
