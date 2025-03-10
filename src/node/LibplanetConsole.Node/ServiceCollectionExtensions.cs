using JSSoft.Commands;
using LibplanetConsole.Alias;
using LibplanetConsole.Alias.Commands;
using LibplanetConsole.BlockChain;
using LibplanetConsole.Common;
using LibplanetConsole.Node.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LibplanetConsole.Node;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNode(
        this IServiceCollection @this, IConfiguration configuration)
    {
        var synchronizationContext = SynchronizationContext.Current ?? new();
        SynchronizationContext.SetSynchronizationContext(synchronizationContext);

        @this.AddOptions<ApplicationOptions>()
            .Bind(configuration.GetSection(ApplicationOptions.Position))
            .ValidateDataAnnotations();
        @this.AddSingleton<IApplicationOptions>(
            s => s.GetRequiredService<IOptions<ApplicationOptions>>().Value);

        @this.AddSingleton(synchronizationContext);
        @this.AddSingleton<Node>()
            .AddSingleton<INode>(s => s.GetRequiredService<Node>())
            .AddSingleton<IBlockChain>(s => s.GetRequiredService<Node>());
        @this.AddSingleton<AliasCollection>()
            .AddSingleton<IAliasCollection>(s => s.GetRequiredService<AliasCollection>())
            .AddSingleton<INodeContent>(s => s.GetRequiredService<AliasCollection>());
        @this.AddSingleton<IInfoProvider, ApplicationInfoProvider>();
        @this.AddSingleton<IInfoProvider, NodeInfoProvider>();

        @this.AddHostedService<NodeHostedService>();

        @this.AddSingleton<ICommand, ExitCommand>();
        @this.AddSingleton<ICommand, InfoCommand>();
        @this.AddSingleton<ICommand, KeyCommand>();
        @this.AddSingleton<ICommand, StartCommand>();
        @this.AddSingleton<ICommand, StopCommand>();
        @this.AddSingleton<ICommand, TxCommand>();
        @this.AddSingleton<ICommand, NonceCommand>();
        @this.AddAliasCommands();

        return @this;
    }
}
