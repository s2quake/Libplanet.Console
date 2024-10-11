using JSSoft.Commands;
using LibplanetConsole.Common;
using LibplanetConsole.Logging;
using LibplanetConsole.Node.Evidence;
using LibplanetConsole.Node.Executable.Commands;
using LibplanetConsole.Node.Executable.Tracers;
using LibplanetConsole.Node.Explorer;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace LibplanetConsole.Node.Executable;

internal sealed class Application
{
    private readonly WebApplicationBuilder _builder = WebApplication.CreateBuilder();

    public Application(ApplicationOptions options, object[] instances)
    {
        var (_, port) = EndPointUtility.GetHostAndPort(options.EndPoint);
        var services = _builder.Services;
        foreach (var instance in instances)
        {
            services.AddSingleton(instance.GetType(), instance);
        }

        _builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(port, o => o.Protocols = HttpProtocols.Http2);
        });

        services.AddLogging(options.LogPath, options.LibraryLogPath);

        services.AddSingleton<CommandContext>();
        services.AddSingleton<SystemTerminal>();

        services.AddSingleton<HelpCommand>()
                .AddSingleton<ICommand>(s => s.GetRequiredService<HelpCommand>());
        services.AddSingleton<VersionCommand>()
                .AddSingleton<ICommand>(s => s.GetRequiredService<VersionCommand>());

        services.AddNode(options);
        services.AddExplorer(_builder.Configuration);

        services.AddGrpc();
        services.AddGrpcReflection();

        services.AddHostedService<BlockChainEventTracer>();
        services.AddHostedService<NodeEventTracer>();
        services.AddHostedService<SystemTerminalHostedService>();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var app = _builder.Build();

        app.UseNode();
        app.UseExplorer();
        app.MapGet("/", () => "Libplanet-Node");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGrpcReflectionService().AllowAnonymous();

        await Console.Out.WriteLineAsync();
        await app.RunAsync(cancellationToken);
    }
}
