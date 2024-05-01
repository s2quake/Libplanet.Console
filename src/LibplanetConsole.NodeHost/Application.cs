using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Net;
using JSSoft.Commands.Extensions;
using JSSoft.Communication;
using JSSoft.Communication.Extensions;
using JSSoft.Terminals;
using LibplanetConsole.Common.Actions;
using LibplanetConsole.Common.Extensions;
using LibplanetConsole.Frameworks;
using LibplanetConsole.GameServices;
using LibplanetConsole.NodeHost.Serializations;
using LibplanetConsole.NodeServices;

namespace LibplanetConsole.NodeHost;

internal sealed class Application : ApplicationBase, IApplication
{
    private readonly CompositionContainer _container;
    private readonly ApplicationOptions _options = new();
    private readonly Node _node;
    private readonly NodeServiceContext _nodeServiceContext;
    private readonly Process? _parentProcess;
    private SystemTerminal? _terminal;
    private Guid _closeToken;

    public Application(ApplicationOptions options)
    {
        _options = options;
        _node = new Node(options, [new AssemblyActionLoader(typeof(Player).Assembly)]);
        _container = new(new AssemblyCatalog(typeof(Application).Assembly));
        _container.ComposeExportedValue<IApplication>(this);
        _container.ComposeExportedValue<IServiceProvider>(this);
        _container.ComposeExportedValue(_options);
        _container.ComposeExportedValue(_node);
        _container.ComposeExportedValue<INode>(_node);
        _nodeServiceContext = _container.GetExportedValue<NodeServiceContext>()!;
        _node.BlockAppended += Node_BlockAppended;
        _node.Started += Node_Started;
        _node.Stopped += Node_Stopped;
        if (_options.ParentProcessId != 0 &&
            Process.GetProcessById(_options.ParentProcessId) is { } parentProcess)
        {
            _parentProcess = parentProcess;
        }
    }

    public EndPoint EndPoint => _nodeServiceContext.EndPoint;

    public ApplicationInfo Info => new()
    {
        EndPoint = EndPointUtility.ToString(EndPoint),
        NodeInfo = _node.Info,
    };

    protected override bool CanClose => _parentProcess?.HasExited == true;

    public override object? GetService(Type serviceType)
    {
        var contractName = AttributedModelServices.GetContractName(serviceType);
        return _container.GetExportedValue<object?>(contractName);
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
        _closeToken = await _nodeServiceContext.OpenAsync(cancellationToken: default);
        await base.OnStartAsync(cancellationToken);

        if (_options.ParentProcessId == 0)
        {
            var separator = new string('=', 80);
            var sw = new StringWriter();
            var commandContext = _container.GetExportedValue<CommandContext>()!;
            commandContext.Out = sw;
            await AutoStartAsync();
            sw.WriteLine(TerminalStringBuilder.GetString(separator, TerminalColorType.BrightGreen));
            await commandContext.ExecuteAsync(["--help"], cancellationToken: default);
            sw.WriteLine();
            await commandContext.ExecuteAsync(args: [], cancellationToken: default);
            sw.WriteLine(TerminalStringBuilder.GetString(separator, TerminalColorType.BrightGreen));
            commandContext.Out = Console.Out;
            Console.Write(sw.ToString());

            _terminal = _container.GetExportedValue<SystemTerminal>()!;
            await _terminal.StartAsync(cancellationToken);
        }
        else
        {
            await AutoStartAsync();
        }
    }

    protected override async ValueTask OnDisposeAsync()
    {
        await base.OnDisposeAsync();
        await _nodeServiceContext.ReleaseAsync(_closeToken);
        _node.BlockAppended -= Node_BlockAppended;
        _node.Started -= Node_Started;
        _node.Stopped -= Node_Stopped;
        _container.Dispose();
    }

    private async Task AutoStartAsync()
    {
        if (_options.AutoStart == true)
        {
            var nodeOptions = NodeOptionsUtility.GetNodeOptions(_node);
            await _node.StartAsync(nodeOptions, cancellationToken: default);
        }
    }

    private void Node_BlockAppended(object? sender, BlockEventArgs e)
    {
        var blockInfo = e.BlockInfo;
        var hash = blockInfo.Hash[0..8];
        var miner = blockInfo.Miner[0..8];
        var message = $"Block #{blockInfo.Index} '{hash}' Appended by '{miner}'";
        Console.WriteLine(
            TerminalStringBuilder.GetString(message, TerminalColorType.BrightGreen));
    }

    private void Node_Started(object? sender, EventArgs e)
    {
        var endPoint = EndPointUtility.ToString(_nodeServiceContext.EndPoint);
        var message = $"BlockChain has been started.: {endPoint}";
        Console.WriteLine(
            TerminalStringBuilder.GetString(message, TerminalColorType.BrightGreen));
        Console.Out.WriteLineAsJson(_node.Info);
    }

    private void Node_Stopped(object? sender, EventArgs e)
    {
        var endPoint = EndPointUtility.ToString(_nodeServiceContext.EndPoint);
        var message = $"BlockChain has been stopped.: {endPoint}";
        Console.WriteLine(
            TerminalStringBuilder.GetString(message, TerminalColorType.BrightGreen));
    }
}
