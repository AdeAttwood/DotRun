namespace DotRun.Test;

public class TaskItemTest
{
    [Fact]
    public void DependsOn_TaskItem_AddsDependencyAndReturnsSelf()
    {
        var task1 = new TaskItem { Name = "task1" };
        var task2 = new TaskItem { Name = "task2" };

        var result = task2.DependsOn(task1);

        Assert.Equal(task2, result);
        Assert.Contains("task1", task2.Dependencies);
    }

    [Fact]
    public void DependsOn_String_AddsDependencyAndReturnsSelf()
    {
        var task = new TaskItem { Name = "task" };

        var result = task.DependsOn("dep");

        Assert.Equal(task, result);
        Assert.Contains("dep", task.Dependencies);
    }

    [Fact]
    public void Run_SetsActionAndReturnsSelf()
    {
        var task = new TaskItem { Name = "task" };
        var action = (Context ctx) => Task.CompletedTask;

        var result = task.Run(action);

        Assert.Equal(task, result);
        Assert.Equal(action, task.Action);
    }

    [Fact]
    public async Task Run_ExecutesAction_WhenActionIsSet()
    {
        var task = new TaskItem { Name = "task" };
        var executed = false;
        var context = new Context();

        task.Run(ctx => { executed = true; return Task.CompletedTask; });

        await task.Run(context);

        Assert.True(executed);
    }

    [Fact]
    public async Task Run_ThrowsException_WhenActionIsNull()
    {
        var task = new TaskItem { Name = "task" };
        var context = new Context();

        var exception = await Assert.ThrowsAsync<Exception>(() => task.Run(context));

        Assert.Equal("Unable to run task, there is no Action", exception.Message);
    }
}
