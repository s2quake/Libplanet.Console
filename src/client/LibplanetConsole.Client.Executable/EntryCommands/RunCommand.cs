using System.ComponentModel;
using JSSoft.Commands;
using LibplanetConsole.Common;
using LibplanetConsole.Common.DataAnnotations;
using LibplanetConsole.DataAnnotations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace LibplanetConsole.Client.Executable.EntryCommands;

[CommandSummary("Runs the libplanet-client")]
internal sealed class RunCommand
    : CommandAsyncBase, IConfigureOptions<ApplicationOptions>
{
    [CommandProperty]
    [CommandSummary("Specifies the port on which the client will run.")]
    [NonNegative]
    public int Port { get; init; }

    [CommandProperty]
    [CommandSummary("Specifies the private key of the client.")]
    [PrivateKey]
    public string PrivateKey { get; init; } = string.Empty;

    [CommandProperty("parent")]
    [CommandSummary("Reserved option used by libplanet-console")]
    [Category]
    public int ParentProcessId { get; init; }

    [CommandProperty]
    [CommandSummary("Specifies the hub url to connect")]
    [Uri(AllowEmpty = true)]
    public string HubUrl { get; init; } = string.Empty;

    [CommandProperty]
    [CommandSummary("Specifies the file path to save logs")]
    [Path(Type = PathType.Directory, AllowEmpty = true)]
    [DefaultValue("")]
    public string LogPath { get; set; } = string.Empty;

    [CommandPropertySwitch("no-repl")]
    [CommandSummary("If set, the application starts without REPL")]
    public bool NoREPL { get; init; }

    void IConfigureOptions<ApplicationOptions>.Configure(ApplicationOptions options)
    {
        var port = Port;
        var privateKey = PrivateKeyUtility.ParseOrRandom(PrivateKey);
        options.Port = port;
        options.PrivateKey = PrivateKeyUtility.ToString(privateKey);
        options.ParentProcessId = ParentProcessId;
        options.HubUrl = HubUrl;
        options.LogPath = GetFullPath(LogPath);
        options.NoREPL = NoREPL;

        static string GetFullPath(string path)
            => path != string.Empty ? Path.GetFullPath(path) : path;
    }

    protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var builder = CreateBuilder(this);
            var application = new Application(builder);
            var port = Port is 0 ? PortUtility.NextPort() : Port;
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(port, o => o.Protocols = HttpProtocols.Http2);
                options.ListenLocalhost(port + 1, o => o.Protocols = HttpProtocols.Http1AndHttp2);
            });

            await application.RunAsync(cancellationToken);
        }
        catch (CommandParsingException e)
        {
            e.Print(System.Console.Out);
            Environment.Exit(1);
        }
    }

    private static WebApplicationBuilder CreateBuilder(
        IConfigureOptions<ApplicationOptions> configureOptions)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(configureOptions);
        return builder;
    }
}
