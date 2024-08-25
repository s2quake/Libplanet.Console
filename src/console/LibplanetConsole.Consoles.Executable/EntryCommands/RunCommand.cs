using System.Diagnostics;
using JSSoft.Commands;
using LibplanetConsole.Frameworks;

namespace LibplanetConsole.Consoles.Executable.EntryCommands;

[CommandSummary("Run the Libplanet console.")]
internal sealed class RunCommand : CommandAsyncBase, ICustomCommandDescriptor
{
    CommandMemberDescriptorCollection ICustomCommandDescriptor.Members
    {
        get
        {
            if (ApplicationSettingsParser.Instance is ICustomCommandDescriptor customDescriptor)
            {
                return customDescriptor.Members;
            }

            throw new UnreachableException();
        }
    }

    object ICustomCommandDescriptor.GetMemberOwner(CommandMemberDescriptor memberDescriptor)
    {
        if (ApplicationSettingsParser.Instance is ICustomCommandDescriptor customDescriptor)
        {
            return customDescriptor.GetMemberOwner(memberDescriptor);
        }

        throw new UnreachableException();
    }

    protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settings = ApplicationSettingsParser.Peek<ApplicationSettings>();
            var @out = Console.Out;
            await using var application = new Application(settings);
            await @out.WriteLineAsync();
            await application.RunAsync();
            await @out.WriteLineAsync("\u001b0");
        }
        catch (CommandParsingException e)
        {
            e.Print(Console.Out);
            Environment.Exit(1);
        }
    }
}
