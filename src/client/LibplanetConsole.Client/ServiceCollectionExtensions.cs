using JSSoft.Commands;
using LibplanetConsole.Alias;
using LibplanetConsole.Alias.Commands;
using LibplanetConsole.BlockChain;
using LibplanetConsole.Client.Commands;
using LibplanetConsole.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LibplanetConsole.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClient(
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
        @this.AddSingleton<Client>()
            .AddSingleton<IClient>(s => s.GetRequiredService<Client>());
        @this.AddSingleton<ClientBlockChain>()
            .AddSingleton<IBlockChain>(s => s.GetRequiredService<ClientBlockChain>())
            .AddSingleton<IClientContent>(s => s.GetRequiredService<ClientBlockChain>());
        @this.AddHostedService<ClientHostedService>();
        @this.AddSingleton<IInfoProvider, ApplicationInfoProvider>();
        @this.AddSingleton<IInfoProvider, ClientInfoProvider>();
        @this.AddSingleton<AliasCollection>()
            .AddSingleton<IAliasCollection>(s => s.GetRequiredService<AliasCollection>())
            .AddSingleton<IClientContent>(s => s.GetRequiredService<AliasCollection>());

        @this.AddSingleton<ICommand, ExitCommand>();
        @this.AddSingleton<ICommand, InfoCommand>();
        @this.AddSingleton<ICommand, KeyCommand>();
        @this.AddSingleton<ICommand, StartCommand>();
        @this.AddSingleton<ICommand, StopCommand>();
        @this.AddSingleton<ICommand, TxCommand>();
        @this.AddAliasCommands();
        return @this;
    }
}
