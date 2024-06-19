using System.ComponentModel.Composition;
using LibplanetConsole.Common.Extensions;

namespace LibplanetConsole.Consoles;

[Export(typeof(IProcessArgumentProvider))]
internal sealed class NodeProcessArgumentProvider : ProcessArgumentProviderBase<Node>
{
    protected override IEnumerable<string> GetArguments(Node obj)
    {
        var contents = obj.GetService<IEnumerable<INodeContent>>();
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
