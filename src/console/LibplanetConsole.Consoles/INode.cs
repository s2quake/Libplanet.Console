using System.Net;
using Libplanet.Action;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using LibplanetConsole.Common;
using LibplanetConsole.Nodes.Serializations;

namespace LibplanetConsole.Consoles;

public interface INode : IAddressable, IAsyncDisposable, IServiceProvider, ISigner
{
    event EventHandler<BlockEventArgs>? BlockAppended;

    event EventHandler? Attached;

    event EventHandler? Detached;

    event EventHandler? Started;

    event EventHandler? Stopped;

    event EventHandler? Disposed;

    bool IsAttached { get; }

    bool IsRunning { get; }

    EndPoint EndPoint { get; }

    NodeInfo Info { get; }

    NodeOptions NodeOptions { get; }

    PublicKey PublicKey { get; }

    Task AttachAsync(CancellationToken cancellationToken);

    Task DetachAsync(CancellationToken cancellationToken);

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);

    Task<TxId> SendTransactionAsync(IAction[] actions, CancellationToken cancellationToken);

    Task<long> GetNextNonceAsync(Address address, CancellationToken cancellationToken);
}
