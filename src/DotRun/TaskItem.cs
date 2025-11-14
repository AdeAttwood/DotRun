namespace DotRun;

public class TaskItem
{
    public required string Name { get; set; }

    public List<string> Dependencies { get; set; } = new();

    public Func<Context, Task>? Action { get; set; }

    public TaskItem DependsOn(TaskItem task)
    {
        this.DependsOn(task.Name);

        return this;
    }

    public TaskItem DependsOn(string taskName)
    {
        this.Dependencies.Add(taskName);

        return this;
    }

    public TaskItem Run(Func<Context, Task> action)
    {
        this.Action = action;

        return this;
    }

    public async Task Run(Context c)
    {
        if (Action == null)
        {
            throw new Exception($"Unable to run {this.Name}, there is no Action");
        }

        await this.Action(c);
    }
}
