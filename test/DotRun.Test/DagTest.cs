namespace DotRun.Test;

public class DagTest
{
    [Fact]
    public void AddNode_AddsNodeToNodes()
    {
        var dag = new Dag();
        var node = new DagNode("A", (ctx) => Task.CompletedTask);

        dag.AddNode(node);

        Assert.Contains(node, dag.Nodes);
    }

    [Fact]
    public void AddEdge_AddsEdgeBetweenExistingNodes()
    {
        var dag = new Dag();
        var nodeA = new DagNode("A", (ctx) => Task.CompletedTask);
        var nodeB = new DagNode("B", (ctx) => Task.CompletedTask);
        dag.AddNode(nodeA);
        dag.AddNode(nodeB);

        dag.AddEdge("A", "B");

        Assert.Contains("B", dag.Edges["A"]);
    }

    [Fact]
    public void AddEdge_ThrowsException_WhenFromNodeDoesNotExist()
    {
        var dag = new Dag();
        var nodeB = new DagNode("B", (ctx) => Task.CompletedTask);
        dag.AddNode(nodeB);

        var exception = Assert.Throws<InvalidOperationException>(() => dag.AddEdge("A", "B"));
        Assert.Equal("Both nodes must exist before adding an edge.", exception.Message);
    }

    [Fact]
    public void AddEdge_ThrowsException_WhenToNodeDoesNotExist()
    {
        var dag = new Dag();
        var nodeA = new DagNode("A", (ctx) => Task.CompletedTask);
        dag.AddNode(nodeA);

        var exception = Assert.Throws<InvalidOperationException>(() => dag.AddEdge("A", "B"));
        Assert.Equal("Both nodes must exist before adding an edge.", exception.Message);
    }

    [Fact]
    public void GetRootNodes_ReturnsNodesWithNoIncomingEdges()
    {
        var dag = new Dag();
        var nodeA = new DagNode("A", (ctx) => Task.CompletedTask);
        var nodeB = new DagNode("B", (ctx) => Task.CompletedTask);
        var nodeC = new DagNode("C", (ctx) => Task.CompletedTask);
        dag.AddNode(nodeA);
        dag.AddNode(nodeB);
        dag.AddNode(nodeC);
        dag.AddEdge("A", "B");

        var roots = dag.GetRootNodes().ToList();

        Assert.Contains(nodeA, roots);
        Assert.Contains(nodeC, roots);
        Assert.DoesNotContain(nodeB, roots);
    }

    [Fact]
    public void GetChildren_ReturnsDirectChildren()
    {
        var dag = new Dag();
        var nodeA = new DagNode("A", (ctx) => Task.CompletedTask);
        var nodeB = new DagNode("B", (ctx) => Task.CompletedTask);
        var nodeC = new DagNode("C", (ctx) => Task.CompletedTask);
        dag.AddNode(nodeA);
        dag.AddNode(nodeB);
        dag.AddNode(nodeC);
        dag.AddEdge("A", "B");
        dag.AddEdge("A", "C");

        var children = dag.GetChildren(nodeA).ToList();

        Assert.Contains(nodeB, children);
        Assert.Contains(nodeC, children);
    }

    [Fact]
    public void GetChildren_ReturnsEmpty_WhenNoChildren()
    {
        var dag = new Dag();
        var nodeA = new DagNode("A", (ctx) => Task.CompletedTask);
        dag.AddNode(nodeA);

        var children = dag.GetChildren(nodeA);

        Assert.Empty(children);
    }

    [Fact]
    public void DagNode_ToString_ReturnsId()
    {
        var node = new DagNode("test", (ctx) => Task.CompletedTask);

        Assert.Equal("test", node.ToString());
    }
}
