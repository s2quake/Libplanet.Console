using System.ComponentModel.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace LibplanetConsole.Console;

[Export(typeof(IProcessArgumentProvider))]
internal sealed class NodeProcessArgumentProvider : ProcessArgumentProviderBase<Node>
{
    protected override IEnumerable<string> GetArguments(Node obj)
    {
        var contents = obj.GetRequiredService<IEnumerable<INodeContent>>();
        foreach (var content in contents)
        {
            var args = ProcessEnvironment.GetArguments(serviceProvider: obj, obj: content);
            foreach (var arg in args)
            {
                yield return arg;
            }
        }
    }
}
