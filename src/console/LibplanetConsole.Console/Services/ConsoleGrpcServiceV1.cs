using Grpc.Core;
using LibplanetConsole.Console.Grpc;
using LibplanetConsole.Grpc;
using LibplanetConsole.Node;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace LibplanetConsole.Console.Services;

public sealed class ConsoleGrpcServiceV1(
    IServer server,
    IApplicationOptions options,
    INodeCollection nodes,
    IClientCollection clients)
    : ConsoleGrpcService.ConsoleGrpcServiceBase
{
    private Uri? _seedUrl;

    public override Task<GetNodeSettingsResponse> GetNodeSettings(
        GetNodeSettingsRequest request, ServerCallContext context)
    {
        var genesis = BlockUtility.SerializeBlock(options.GenesisBlock);
        return Task.FromResult(new GetNodeSettingsResponse
        {
            AppProtocolVersion = options.AppProtocolVersion,
            Genesis = TypeUtility.ToGrpc(genesis),
            ProcessId = Environment.ProcessId,
            SeedUrl = GetSeedUrl().ToString(),
        });
    }

    public override Task<GetClientSettingsResponse> GetClientSettings(
        GetClientSettingsRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetClientSettingsResponse
        {
            ProcessId = Environment.ProcessId,
            NodeUrl = RandomNodeUrl().ToString(),
        });
    }

    public override async Task<AttachNodeResponse> AttachNode(
        AttachNodeRequest request, ServerCallContext context)
    {
        var attachOptions = new AttachOptions
        {
            Address = TypeUtility.ToAddress(request.Address),
            Url = new Uri(request.Url),
            ProcessId = request.ProcessId,
        };
        await nodes.AttachAsync(attachOptions, context.CancellationToken);
        return new AttachNodeResponse();
    }

    public override async Task<AttachClientResponse> AttachClient(
        AttachClientRequest request, ServerCallContext context)
    {
        var attachOptions = new AttachOptions
        {
            Address = TypeUtility.ToAddress(request.Address),
            Url = new Uri(request.Url),
            ProcessId = request.ProcessId,
        };
        await clients.AttachAsync(attachOptions, context.CancellationToken);
        return new AttachClientResponse();
    }

    private Uri GetSeedUrl()
    {
        if (_seedUrl is null)
        {
            var addressesFeature = server.Features.Get<IServerAddressesFeature>()
                ?? throw new InvalidOperationException("ServerAddressesFeature is not available.");
            var address = addressesFeature.Addresses.First();
            _seedUrl = new Uri(address);
        }

        return _seedUrl;
    }

    private Uri RandomNodeUrl()
    {
        var nodeIndex = Random.Shared.Next(nodes.Count);
        var node = nodes[nodeIndex];
        return node.Url;
    }
}
