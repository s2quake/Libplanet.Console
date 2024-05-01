using System.Security.Cryptography;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Crypto;
using Libplanet.Types.Blocks;
using Libplanet.Types.Tx;
using LibplanetConsole.Games.Actions;
using LibplanetConsole.Games.Serializations;

namespace LibplanetConsole.Games;

public record class GamePlayRecord(
    Block Block, ITransaction Transaction, IValue Action, int Offset)
{
    public static IEnumerable<GamePlayRecord> GetGamePlayRecords(
        BlockChain blockChain, Address userAddress)
    {
        for (var i = 0; i < blockChain.Count; i++)
        {
            if (GetGamePlayRecord(blockChain[i], userAddress) is { } gamePlayRecord)
            {
                yield return gamePlayRecord;
            }
        }
    }

    public static GamePlayRecord? GetGamePlayRecord(Block block, Address userAddress)
    {
        for (var i = 0; i < block.Transactions.Count; i++)
        {
            var transaction = block.Transactions[i];
            for (var j = 0; j < transaction.Actions.Count; j++)
            {
                var action = transaction.Actions[j];
                if (IsGamePlayAction(action) == true && GetUserAddress(action) == userAddress)
                {
                    return new(block, transaction, action, j);
                }
            }
        }

        return null;
    }

    public int GetSeed()
    {
        var block = Block;
        var transaction = Transaction;
        var offset = Offset;
        var preEvaluationHashBytes = block.PreEvaluationHash.ToByteArray();
        var signature = transaction.Signature;
        return ActionEvaluator.GenerateRandomSeed(preEvaluationHashBytes, signature, offset);
    }

    public StageInfo GetStageInfo()
    {
        if (Action is Dictionary values)
        {
            return new StageInfo((Dictionary)values[nameof(GamePlayAction.StageInfo)]);
        }

        throw new NotSupportedException();
    }

    private static byte[] ComputeHash(byte[] bytes)
    {
        using var sha = SHA1.Create();
        return sha.ComputeHash(bytes);
    }

    private static bool IsGamePlayAction(IValue value)
    {
        return value is Dictionary values && values["type_id"] is Text text && text == "game-play";
    }

    private static Address GetUserAddress(IValue value)
    {
        if (value is Dictionary values && values[nameof(GamePlayAction.UserAddress)] is { } data)
        {
            return new Address(data);
        }

        return new PrivateKey().Address;
    }
}
