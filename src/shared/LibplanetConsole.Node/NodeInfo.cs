using Libplanet.Crypto;
using LibplanetConsole.Common;

namespace LibplanetConsole.Node;

public readonly record struct NodeInfo
{
    public required int ProcessId { get; init; }

    public string AppProtocolVersion { get; init; }

    public string SwarmEndPoint { get; init; }

    public string ConsensusEndPoint { get; init; }

    public Address Address { get; init; }

    public AppHash GenesisHash { get; init; }

    public AppHash TipHash { get; init; }

    public bool IsRunning { get; init; }

    public AppPeer[] Peers { get; init; }

    public static NodeInfo Empty { get; } = new NodeInfo
    {
        ProcessId = -1,
        AppProtocolVersion = string.Empty,
        SwarmEndPoint = string.Empty,
        ConsensusEndPoint = string.Empty,
        Peers = [],
    };
}
