using LibplanetConsole.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LibplanetConsole.Console;

public interface IClient : IAddressable, IAsyncDisposable, IKeyedServiceProvider, ISigner
{
    const string Key = nameof(IClient);

    event EventHandler? Attached;

    event EventHandler? Detached;

    event EventHandler? Started;

    event EventHandler? Stopped;

    event EventHandler? Disposed;

    int ProcessId { get; }

    bool IsAttached { get; }

    bool IsRunning { get; }

    EndPoint EndPoint { get; }

    ClientInfo Info { get; }

    PublicKey PublicKey { get; }

    Task StartProcessAsync(ProcessOptions options, CancellationToken cancellationToken);

    Task StopProcessAsync(CancellationToken cancellationToken);

    Task AttachAsync(CancellationToken cancellationToken);

    Task DetachAsync(CancellationToken cancellationToken);

    Task StartAsync(INode node, CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
