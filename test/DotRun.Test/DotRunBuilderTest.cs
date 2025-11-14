namespace DotRun.Test;

public class DotRunBuilderTest
{
    [Fact]
    public void Task_AddsTaskItemWithGivenName()
    {
        var builder = new DotRunBuilder();

        var task = builder.Task("test");

        Assert.Equal("test", task.Name);
        Assert.Single(builder.Tasks);
        Assert.Equal("test", builder.Tasks.Single().Name);
    }

    [Fact]
    public void RunCommand_CreatesTaskWithShellAction()
    {
        var builder = new DotRunBuilder();

        var task = builder.RunCommand("test", "echo", "hello");

        Assert.Equal("test", task.Name);
        Assert.NotNull(task.Action);
    }

    [Fact]
    public async Task RunAsync_ExecutesTasks()
    {
        var builder = new DotRunBuilder();
        var executed = false;

        builder.Task("test").Run(ctx => { executed = true; return Task.CompletedTask; });

        await builder.RunAsync();

        Assert.True(executed);
    }

    [Fact]
    public async Task RunAsync_ExecutesTasksInOrder()
    {
        var builder = new DotRunBuilder();
        var order = new List<string>();

        builder.Task("a").Run(ctx => { order.Add("a"); return Task.CompletedTask; });
        builder.Task("b").DependsOn("a").Run(ctx => { order.Add("b"); return Task.CompletedTask; });

        await builder.RunAsync();

        Assert.Equal(new[] { "a", "b" }, order);
    }
}
