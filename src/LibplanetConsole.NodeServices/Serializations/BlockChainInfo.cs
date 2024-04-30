using Libplanet.Blockchain;

namespace LibplanetConsole.NodeServices.Serializations;

public record class BlockChainInfo
{
    public BlockChainInfo(BlockChain blockChain)
    {
        var blocks = new BlockInfo[blockChain.Count];
        for (var i = 0; i < blockChain.Count; i++)
        {
            blocks[i] = new BlockInfo(blockChain[i]);
        }

        GenesisHash = $"{blockChain.Genesis.Hash}";
    }

    public BlockChainInfo()
    {
    }

    public string GenesisHash { get; init; } = string.Empty;
}
