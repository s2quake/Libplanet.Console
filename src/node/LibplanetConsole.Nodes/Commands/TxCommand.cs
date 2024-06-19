using System.ComponentModel.Composition;
using JSSoft.Commands;
using LibplanetConsole.Common;
using LibplanetConsole.Common.Actions;

namespace LibplanetConsole.Nodes.Commands;

[Export(typeof(ICommand))]
[method: ImportingConstructor]
[CommandSummary("Adds a transaction to store simple string.")]
internal sealed class TxCommand(INode node) : CommandAsyncBase
{
    [CommandPropertyRequired]
    public string Text { get; set; } = string.Empty;

    protected override async Task OnExecuteAsync(
        CancellationToken cancellationToken, IProgress<ProgressInfo> progress)
    {
        var action = new StringAction
        {
            Value = Text,
        };
        await node.AddTransactionAsync([action], cancellationToken);
        await Out.WriteLineAsync($"{(ShortAddress)node.Address}: {Text}");
    }
}
