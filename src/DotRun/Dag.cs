namespace DotRun;

public class DagNode
{
    public string Id { get; }
    public Func<Context, Task> Action { get; }

    public DagNode(string id, Func<Context, Task> action)
    {
        Id = id;
        Action = action;
    }

    public override string ToString() => Id;
}

public class Dag
{
    private readonly Dictionary<string, DagNode> _nodes = new();
    private readonly Dictionary<string, HashSet<string>> _edges = new();

    public IEnumerable<DagNode> Nodes => _nodes.Values;

    public void AddNode(DagNode node)
    {
        _nodes[node.Id] = node;
        _edges.TryAdd(node.Id, new HashSet<string>());
    }

    public void AddEdge(string fromId, string toId)
    {
        if (!_nodes.ContainsKey(fromId) || !_nodes.ContainsKey(toId))
        {
            throw new InvalidOperationException("Both nodes must exist before adding an edge.");
        }

        _edges[fromId].Add(toId);
    }

    public IEnumerable<DagNode> GetRootNodes()
    {
        var dependents = _edges.Values.SelectMany(e => e).ToHashSet();
        return _nodes.Values.Where(n => !dependents.Contains(n.Id));
    }

    public IEnumerable<DagNode> GetChildren(DagNode node)
    {
        if (_edges.TryGetValue(node.Id, out var children))
            return children.Select(id => _nodes[id]);
        return Enumerable.Empty<DagNode>();
    }

    public IReadOnlyDictionary<string, HashSet<string>> Edges => _edges;
}
