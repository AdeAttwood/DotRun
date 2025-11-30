namespace DotRun;

public class DotRunBuilder
{
    private readonly List<TaskItem> _tasks = new();

    public IReadOnlyList<TaskItem> Tasks => _tasks.AsReadOnly();

    public TaskItem Task(string name)
    {
        var task = new TaskItem { Name = name };
        _tasks.Add(task);

        return task;
    }

    public TaskItem RunCommand(string name, string command, string args)
    {
        return this.Task(name).Run(c => c.Shell(command, args));
    }

    public async Task RunAsync()
    {
        var dag = new Dag();
        var ctx = new Context();

        foreach (var task in _tasks)
        {
            if (task.Action == null) continue;
            dag.AddNode(new DagNode(task.Name, task.Action));

            foreach (var child in task.Dependencies)
            {
                dag.AddEdge(child, task.Name);
            }
        }

        var executor = new DagExecutor(dag, ctx);
        await executor.ExecuteAsync();
    }
}
