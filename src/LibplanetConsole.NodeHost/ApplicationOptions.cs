using JSSoft.Commands;

namespace LibplanetConsole.NodeHost;

internal record class ApplicationOptions
{
    [CommandProperty]
    public string EndPoint { get; init; } = string.Empty;

    [CommandProperty]
    public string PrivateKey { get; init; } = string.Empty;

    [CommandProperty("parent")]
    public int ParentProcessId { get; init; }

    [CommandPropertySwitch('a')]
    public bool AutoStart { get; init; } = false;

    public static ApplicationOptions Parse(string[] args)
    {
        var options = new ApplicationOptions();
        var commandSettings = new CommandSettings
        {
            AllowEmpty = true,
        };
        var parser = new CommandParser(options, commandSettings);
        parser.Parse(args);
        return options;
    }
}
