using System.Collections.Concurrent;

using Spectre.Console;

namespace DotRun;

public class DagExecutor
{
    private readonly Dag _dag;
    private readonly Context _context;

    public DagExecutor(Dag dag, Context context)
    {
        _dag = dag;
        _context = context;
    }

    public async Task ExecuteAsync()
    {
        var indegree = new Dictionary<string, int>();
        foreach (var node in _dag.Nodes)
        {
            indegree[node.Id] = 0;
        }

        foreach (var edge in _dag.Edges)
        {
            foreach (var child in edge.Value)
            {
                indegree[child]++;
            }
        }

        var readyQueue = new ConcurrentQueue<DagNode>(_dag.GetRootNodes());
        var completed = new ConcurrentDictionary<string, bool>();
        var errors = new List<string>();

        await AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn { Alignment = Justify.Left },
                new SpinnerColumn(),
                new ElapsedTimeColumn()
            })
            .StartAsync(async ctx =>
            {
                var runningTasks = new List<Task>();
                var taskMap = new ConcurrentDictionary<string, ProgressTask>();

                while (!readyQueue.IsEmpty || runningTasks.Count > 0)
                {
                    while (readyQueue.TryDequeue(out var node))
                    {
                        var progressTask = ctx.AddTask(node.Id, autoStart: false);
                        taskMap[node.Id] = progressTask;

                        var t = Task.Run(async () =>
                        {
                            progressTask.StartTask();

                            try
                            {
                                await node.Action(_context);

                                progressTask.StopTask();
                                progressTask.Description = $"[green]{node.Id}[/]";

                                completed[node.Id] = true;

                                foreach (var child in _dag.GetChildren(node))
                                {
                                    if (--indegree[child.Id] == 0)
                                    {
                                        readyQueue.Enqueue(child);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                progressTask.StopTask();
                                progressTask.Description = $"[red]{node.Id}[/]";
                                completed[node.Id] = true;

                                errors.Add($"""
                                [red]ERROR: {node.Id}[/]

                                {e.Message}
                                """);
                            }
                        });

                        runningTasks.Add(t);
                    }

                    var finished = await Task.WhenAny(runningTasks);
                    runningTasks.Remove(finished);
                }
            });

        foreach (var error in errors)
        {
            Console.WriteLine(error.Trim() + "\n\n");
        }
    }
}
