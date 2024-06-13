using System.Net;
using LibplanetConsole.Explorer.Serializations;

namespace LibplanetConsole.Consoles.Explorer;

public interface IExplorerNodeContent
{
    event EventHandler? Started;

    event EventHandler? Stopped;

    EndPoint EndPoint { get; set; }

    ExplorerInfo Info { get; }

    bool IsRunning { get; }

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
