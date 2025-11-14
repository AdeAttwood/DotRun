# DotRun

DotRun is a lightweight task runner for .NET, designed for [single-file C# scripts](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/). Define tasks, dependencies, and automation workflows in plain C# — no extra setup or configuration required.

Run your scripts directly with the .NET CLI and keep your build automation simple, readable, and portable.

## Getting Started

Create a `build.cs` script:

```csharp
#:package DotRun@0.0.1

using DotRun;

var builder = new DotRunBuilder();

// Register a task to run
builder.RunCommand("Build", "dotnet", "build");

// Run all the tasks
await builder.RunAsync();
```

Run it:

```bash
dotnet run ./scripts/build.cs
```

No installation. No bootstrapping. Just `.cs` scripts that work anywhere the .NET SDK is installed.

## Key Features

* **Single-file scripts** — everything lives in one `.cs` file.
* **No setup required** — write your script, run it, done.
* **Fluent C# API** — define tasks, dependencies, shell commands, and async operations.
* **Artifact management** — create logs, hashes, or version files as part of your workflow.
* **Portable** — works wherever .NET is installed, perfect for small projects, CI/CD scripts, or experiments.
* **Parallel execution with DAGs** — tasks run concurrently when possible, respecting dependencies in a directed acyclic graph (DAG), so builds and workflows complete faster without conflicts.

## Docs

### Tasks

The core of DotRun is the `Task`. Use `builder.Task(string name)` to create a new task with a unique ID. Each task requires an action, defined via the `Run` method, which accepts a lambda taking a `Context` object. The `Context` provides access to shell commands, data sharing, and file operations.

Tasks are executed asynchronously and can run in parallel where dependencies allow.

Example:

```csharp
var listFiles = builder
    .Task("ListFiles")
    .Run(async c => {
        var output = await c.Shell("ls", "-al");
        Console.WriteLine(output);
    });
```

### Context API

The `Context` class is passed to each task's action and offers the following methods:

- `Shell(string command, string args)`: Executes a shell command asynchronously and returns its standard output as a string. Throws an exception if the command fails (non-zero exit code).
- `WriteFile(string path, string content)`: Writes the given content to a file at the specified path.
- `Data`: A `ConcurrentDictionary<string, object>` for sharing data between tasks.

### Sharing Data Between Tasks

Tasks can store and retrieve data using the `Context.Data` dictionary. This is useful for passing results from one task to another, such as hashes, versions, or computed values.

Data is stored by key and can be accessed across tasks. Ensure type safety when casting retrieved values.

Example:

```csharp
var getCommitHash = builder
    .Task("GetCommitHash")
    .Run(async c => {
        var hash = await c.Shell("git", "rev-parse", "HEAD");
        c.Data["CommitHash"] = hash.Trim();
    });

builder.Task("SaveHashFile")
    .DependsOn(getCommitHash)
    .Run(c => {
        var hash = (string)c.Data["CommitHash"];
        c.WriteFile("./artifacts/commit-hash.txt", hash);
    });
```

### Defining Dependencies

Tasks can depend on other tasks using `DependsOn(TaskItem)` or `DependsOn(string taskName)`. Dependent tasks will not start until their dependencies complete. Independent tasks run in parallel, forming a Directed Acyclic Graph (DAG) for efficient execution.

Dependencies ensure correct order, such as restoring packages before building.

Example:

```csharp
var showInfo = builder.RunCommand("Info", "dotnet", "--info");
var restore = builder.RunCommand("Restore", "dotnet", "restore");
var build = builder.RunCommand("Build", "dotnet", "build").DependsOn(restore);
var test = builder.RunCommand("Test", "dotnet", "test").DependsOn(build);
```

In this example, `Info` can run immediately, `Restore` depends on nothing, `Build` waits for `Restore`, and `Test` waits for `Build`.

### Run Command Alias

`RunCommand` is a convenience method equivalent to creating a task that runs a shell command. The following are identical:

```csharp
builder.RunCommand("Build", "dotnet", "build");
builder.Task("Build").Run(c => c.Shell("dotnet", "build"));
```

Use `RunCommand` for simple shell invocations; use `Task` and `Run` for more complex logic involving multiple commands, data manipulation, or custom actions.

### Parallel Execution

DotRun executes tasks in a DAG, allowing parallel execution of independent tasks. This speeds up workflows by running non-dependent tasks concurrently.

For example, if you have tasks A, B, and C where C depends on both A and B, A and B will run in parallel, and C will start only after both complete.

Ensure your tasks are stateless or properly synchronized if they share resources.

### Error Handling

If a shell command fails (exits with non-zero code), `Context.Shell` throws an exception with details including stdout and stderr. Exceptions are propagated to the runner, which catches them, displays error details in the console, and continues executing other tasks.

Tasks can include try-catch blocks for custom error handling within the task action.

Example:

```csharp
builder.Task("SafeCommand")
    .Run(async c => {
        try {
            await c.Shell("some-command", "args");
        } catch (Exception ex) {
            Console.WriteLine($"Command failed: {ex.Message}");
            // Handle error, perhaps continue or set flags
        }
    });
```
