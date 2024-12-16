using System.ComponentModel;
using JSSoft.Commands;
using LibplanetConsole.Common.Extensions;

namespace LibplanetConsole.Console.Delegation.Commands;

[CommandSummary("Delegation Commands.")]
[Category("Delegation")]
internal sealed class DelegationCommand(
    IConsole console, IDelegation delegation) : CommandMethodBase
{
    [CommandMethod]
    public async Task StakeAsync(long ncg, CancellationToken cancellationToken)
    {
        await delegation.StakeAsync(ncg, cancellationToken);
    }

    [CommandMethod]
    public async Task InfoAsync(string address = "", CancellationToken cancellationToken = default)
    {
        var delegatorAddress = address == string.Empty ? console.Address : new Address(address);
        var info = await delegation.GetDelegatorInfoAsync(delegatorAddress, cancellationToken);
        await Out.WriteLineAsJsonAsync(info, cancellationToken);
    }
}
