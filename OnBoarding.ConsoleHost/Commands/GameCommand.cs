using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;
using Bencodex.Types;
using JSSoft.Library.Commands;
using Libplanet.Action;
using Libplanet.Types.Blocks;
using Libplanet.Types.Tx;
using OnBoarding.ConsoleHost.Actions;
using OnBoarding.ConsoleHost.Extensions;
using OnBoarding.ConsoleHost.Games;
using OnBoarding.ConsoleHost.Games.Serializations;

namespace OnBoarding.ConsoleHost.Commands;

[Export(typeof(ICommand))]
[CommandSummary("Play, replay, and List games.")]
sealed class GameCommand : CommandMethodBase
{
    private readonly Application _application;
    private readonly SwarmHostCollection _swarmHosts;

    [ImportingConstructor]
    public GameCommand(Application application)
    {
        _application = application;
        _swarmHosts = application.GetService<SwarmHostCollection>()!;
    }

    [CommandProperty(InitValue = 10)]
    public int Tick { get; set; }

    [CommandMethod]
    [CommandMethodStaticProperty(typeof(IndexProperties), nameof(IndexProperties.SwarmIndex))]
    [CommandMethodStaticProperty(typeof(IndexProperties), nameof(IndexProperties.UserIndex))]
    public void Play()
    {
        var swarmHost = _application.GetSwarmHost(IndexProperties.SwarmIndex);
        var user = _application.GetUser(IndexProperties.UserIndex);
        var playerInfo = user.GetPlayerInfo(swarmHost);
        var stageInfo = new StageInfo
        {
            Address = new(),
            Player = playerInfo,
            Monsters = MonsterInfo.Create(10),
        };
        var stageAction = new StageAction
        {
            StageInfo = stageInfo,
        };
        swarmHost.StageTransaction(user, new IAction[] { stageAction });
        // BlockChainUtils.AppendNew(swarmHost.BlockChain, user, validators, new IAction[] { stageAction });
        Out.WriteLine("Game Finished.");
    }

    [CommandMethod]
    [CommandMethodStaticProperty(typeof(IndexProperties), nameof(IndexProperties.SwarmIndex))]
    [CommandMethodStaticProperty(typeof(IndexProperties), nameof(IndexProperties.UserIndex))]
    [CommandMethodStaticProperty(typeof(IndexProperties), nameof(IndexProperties.BlockIndex))]
    [CommandMethodProperty(nameof(Tick))]
    public async Task Replay(CancellationToken cancellationToken)
    {
        var tick = Tick;
        var block = _application.GetBlock(IndexProperties.SwarmIndex, IndexProperties.BlockIndex);
        var (stageInfo, seed) = GetStageInfo(block);
        var stage = new Stage(stageInfo, seed, Out);
        await stage.PlayAsync(tick, cancellationToken);
        var playerInfo = (PlayerInfo)stage.Player;
        Out.WriteLineAsJson(playerInfo);
    }

    [CommandMethod]
    [CommandMethodStaticProperty(typeof(IndexProperties), nameof(IndexProperties.SwarmIndex))]
    [CommandMethodStaticProperty(typeof(IndexProperties), nameof(IndexProperties.UserIndex))]
    public void List()
    {
        var blockChain = _application.GetBlockChain(IndexProperties.SwarmIndex);
        var sb = new StringBuilder();
        for (var i = 0; i < blockChain.Count; i++)
        {
            var block = blockChain[i];
            if (IsStageInfo(block) == true)
            {
                sb.AppendLine($"Block #{i}");
            }
        }
        Out.Write(sb.ToString());
    }

    private static bool IsStageInfo(Block block)
    {
        return block
                .Transactions
                .SelectMany(x => x.Actions)
                .Any(IsStageAction);
    }

    private static bool IsStageAction(IValue value)
    {
        return value is Dictionary values && values["type_id"] is Text text && text == "stage";
    }

    private static (ITransaction transaction, IValue action, int offset) GetStageInfoAction(Block block)
    {
        for (var i = 0; i < block.Transactions.Count; i++)
        {
            var transaction = block.Transactions[i];
            for (var j = 0; j < transaction.Actions.Count; j++)
            {
                var action = transaction.Actions[j];
                if (IsStageAction(action) == true)
                {
                    return (transaction, action, j);
                }
            }
        }
        throw new ArgumentException($"'Block #{block.Index}' does not have {nameof(StageInfo)}.");
    }

    private static (StageInfo stageInfo, int seed) GetStageInfo(Block block)
    {
        var (transaction, action, offset) = GetStageInfoAction(block);
        var values = (Dictionary)action;
        var stageInfo = new StageInfo((Dictionary)values["StageInfo"]);
        var preEvaluationHashBytes = block.PreEvaluationHash.ToByteArray();
        var signature = transaction.Signature;
        var hashedSignature = ComputeHash(signature);
        var seed = ActionEvaluator.GenerateRandomSeed(preEvaluationHashBytes, hashedSignature, signature, offset);
        return (stageInfo, seed);

        static byte[] ComputeHash(byte[] bytes)
        {
            using var hasher = SHA1.Create();
            return hasher.ComputeHash(bytes);
        }
    }
}
