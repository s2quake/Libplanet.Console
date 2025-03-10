using JSSoft.Terminals;
using LibplanetConsole.Common.Extensions;

namespace LibplanetConsole.Client.Executable.Tracers;

internal sealed class ClientEventTracer : ClientContentBase
{
    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        var message = $"BlockChain has been started.";
        System.Console.Out.WriteColoredLine(message, TerminalColorType.BrightGreen);
        return Task.CompletedTask;
    }

    protected override Task OnStopAsync(CancellationToken cancellationToken)
    {
        var message = $"BlockChain has been stopped.";
        System.Console.Out.WriteColoredLine(message, TerminalColorType.BrightGreen);
        return Task.CompletedTask;
    }
}
