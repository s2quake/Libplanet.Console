using System.ComponentModel.Composition;
using Libplanet.Crypto;
using LibplanetConsole.NodeServices;

namespace LibplanetConsole.NodeHost;

[Export]
[Export(typeof(INode))]
[method: ImportingConstructor]
internal sealed class Node(ApplicationOptions options)
    : NodeBase(GetPrivateKey(options)), INode
{
    public static PrivateKey GetPrivateKey(ApplicationOptions options)
    {
        if (options.PrivateKey == string.Empty)
        {
            return new PrivateKey();
        }

        return new PrivateKey(options.PrivateKey);
    }
}
