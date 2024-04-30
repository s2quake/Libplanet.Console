using System.ComponentModel.Composition;
using JSSoft.Commands.Extensions;
using JSSoft.Terminals;

namespace LibplanetConsole.ClientHost;

[Export]
internal sealed class SystemTerminal : SystemTerminalBase
{
    private readonly CommandContext _commandContext;

    [ImportingConstructor]
    public SystemTerminal(IApplication application, CommandContext commandContext)
    {
        _commandContext = commandContext;
        _commandContext.Owner = application;
        Prompt = "libplanet-client $ ";
    }

    protected override string FormatPrompt(string prompt)
    {
        return prompt;
    }

    protected override string[] GetCompletion(string[] items, string find)
    {
        return _commandContext.GetCompletion(items, find);
    }

    protected override Task OnExecuteAsync(string command, CancellationToken cancellationToken)
    {
        return _commandContext.ExecuteAsync(command, cancellationToken);
    }

    protected override void OnInitialize(TextWriter @out, TextWriter error)
    {
        _commandContext.Out = @out;
        _commandContext.Error = error;
    }
}
