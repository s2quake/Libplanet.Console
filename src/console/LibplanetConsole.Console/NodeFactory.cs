using System.Collections.Concurrent;
using System.Diagnostics;
using LibplanetConsole.Common;

namespace LibplanetConsole.Console;

internal static class NodeFactory
{
    private static readonly ConcurrentDictionary<IServiceProvider, Descriptor> _valueByKey = [];
    private static readonly ConcurrentDictionary<Node, AsyncServiceScope> _scopeByNode = [];

    public static Node Create(IServiceProvider serviceProvider, object? key)
    {
        if (_valueByKey.Remove(serviceProvider, out var descriptor) is true)
        {
            var nodeOptions = descriptor.NodeOptions;
            var node = new Node(serviceProvider, nodeOptions);
            _scopeByNode.AddOrUpdate(node, descriptor.ServiceScope, (k, v) => v);
            return node;
        }

        throw new UnreachableException("This should not be called.");
    }

    public static async ValueTask DisposeScopeAsync(Node node)
    {
        if (_scopeByNode.Remove(node, out var serviceScope) is true)
        {
            await node.DisposeAsync();
            await serviceScope.DisposeAsync();
        }
    }

    public static Node CreateNew(IServiceProvider serviceProvider, NodeOptions nodeOptions)
    {
        var serviceScope = serviceProvider.CreateAsyncScope();
        _valueByKey.AddOrUpdate(
            serviceScope.ServiceProvider,
            new Descriptor
            {
                NodeOptions = nodeOptions,
                ServiceScope = serviceScope,
            },
            (k, v) => v);

        var scopedServiceProvider = serviceScope.ServiceProvider;
        var key = INode.Key;
        var node = scopedServiceProvider.GetRequiredKeyedService<Node>(key);
        node.Contents = GetNodeContents(scopedServiceProvider, key);
        return node;
    }

    private static INodeContent[] GetNodeContents(IServiceProvider serviceProvider, string key)
    {
        var contents = serviceProvider.GetKeyedServices<INodeContent>(key)
            .OrderBy(item => item.Order);
        return [.. DependencyUtility.TopologicalSort(contents, content => content.Dependencies)];
    }

    private sealed record class Descriptor
    {
        public required NodeOptions NodeOptions { get; init; }

        public required AsyncServiceScope ServiceScope { get; init; }
    }
}
