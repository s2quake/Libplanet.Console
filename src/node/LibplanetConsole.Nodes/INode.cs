using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Net;
using Libplanet.Types.Tx;
using LibplanetConsole.Common;
using LibplanetConsole.Nodes.Serializations;

namespace LibplanetConsole.Nodes;

public interface INode : IVerifier
{
    event EventHandler<BlockEventArgs>? BlockAppended;

    event EventHandler? Started;

    event EventHandler? Stopped;

    NodeInfo Info { get; }

    NodeOptions NodeOptions { get; }

    bool IsRunning { get; }

    BlockChain BlockChain { get; }

    Swarm Swarm { get; }

    AppPublicKey PublicKey { get; }

    AppAddress Address => PublicKey.Address;

    AppPeer BlocksyncSeedPeer { get; }

    AppPeer ConsensusSeedPeer { get; }

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);

    Task<TxId> AddTransactionAsync(IAction[] values, CancellationToken cancellationToken);

    Task AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken);

    long GetNextNonce(AppAddress address);
}
