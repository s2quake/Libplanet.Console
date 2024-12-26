#if LIBPLANET_NODE
namespace LibplanetConsole.Node;

public readonly partial record struct BlockInfo
{
    public BlockInfo(Block block)
        : this(block.Index, block.Hash, block.Miner)
    {
    }
}
#endif // LIBPLANET_NODE
