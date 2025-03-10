using LibplanetConsole.Common;

namespace LibplanetConsole.Console.Executable;

internal sealed class NodeRepositoryProcess : NodeProcessBase
{
    public required PrivateKey PrivateKey { get; init; }

    public required int Port { get; init; }

    public string HubUrl { get; init; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public required string GenesisPath { get; set; } = string.Empty;

    public required string AppProtocolVersionPath { get; set; } = string.Empty;

    public string ActionProviderModulePath { get; set; } = string.Empty;

    public string ActionProviderType { get; set; } = string.Empty;

    public int BlocksyncPort { get; set; }

    public int ConsensusPort { get; set; }

    public override string[] Arguments
    {
        get
        {
            if (GenesisPath == string.Empty)
            {
                throw new InvalidOperationException("GenesisPath must be set.");
            }

            if (AppProtocolVersionPath == string.Empty)
            {
                throw new InvalidOperationException("AppProtocolVersionPath must be set.");
            }

            var argumentList = new List<string>
            {
                "init",
                OutputPath,
                "--private-key",
                PrivateKeyUtility.ToString(PrivateKey),
                "--port",
                $"{Port}",
                "--genesis-path",
                GenesisPath,
                "--apv-path",
                AppProtocolVersionPath,
            };
            if (HubUrl != string.Empty)
            {
                argumentList.Add("--hub-url");
                argumentList.Add(HubUrl);
            }

            if (ActionProviderModulePath != string.Empty)
            {
                argumentList.Add("--module-path");
                argumentList.Add(ActionProviderModulePath);
            }

            if (ActionProviderType != string.Empty)
            {
                argumentList.Add("--module-type");
                argumentList.Add(ActionProviderType);
            }

            if (BlocksyncPort is not 0)
            {
                argumentList.Add("--blocksync-port");
                argumentList.Add($"{BlocksyncPort}");
            }

            if (ConsensusPort is not 0)
            {
                argumentList.Add("--consensus-port");
                argumentList.Add($"{ConsensusPort}");
            }

            return [.. argumentList];
        }
    }
}
