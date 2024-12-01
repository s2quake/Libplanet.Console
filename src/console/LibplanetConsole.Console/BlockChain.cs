using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibplanetConsole.Console;

internal sealed class BlockChain : IBlockChain, IDisposable, IConsole
{
    private readonly NodeCollection _nodes;
    private readonly PrivateKey _privateKey;
    private readonly BlockHash _genesisHash;
    private readonly ILogger<BlockChain> _logger;
    private IBlockChain? _blockChain;
    private Node? _node;
    private bool _isDisposed;

    public BlockChain(NodeCollection nodes, IApplicationOptions options, ILogger<BlockChain> logger)
    {
        _nodes = nodes;
        _privateKey = options.PrivateKey;
        _genesisHash = options.GenesisBlock.Hash;
        _logger = logger;
        UpdateCurrent(_nodes.Current);
        _nodes.CurrentChanged += Nodes_CurrentChanged;
    }

    public event EventHandler<BlockEventArgs>? BlockAppended;

    public event EventHandler? Started;

    public event EventHandler? Stopped;

    public BlockInfo Tip { get; private set; } = BlockInfo.Empty;

    public bool IsRunning { get; private set; }

    void IDisposable.Dispose()
    {
        if (_isDisposed is false)
        {
            _nodes.CurrentChanged -= Nodes_CurrentChanged;
            UpdateCurrent(null);

            _isDisposed = true;
        }
    }

    public async Task<TxId> SendTransactionAsync(
        IAction[] actions, CancellationToken cancellationToken)
    {
        if (IsRunning is false || _blockChain is null || _node is null)
        {
            throw new InvalidOperationException("BlockChain is not running.");
        }

        var privateKey = _privateKey;
        var genesisHash = _genesisHash;
        var nonce = await _blockChain.GetNextNonceAsync(privateKey.Address, cancellationToken);
        var values = actions.Select(item => item.PlainValue).ToArray();
        var transaction = Transaction.Create(
            nonce: nonce,
            privateKey: privateKey,
            genesisHash: genesisHash,
            actions: new TxActionList(values));

        await _node.SendTransactionAsync(transaction, cancellationToken);
        return transaction.Id;
    }

    Task<long> IBlockChain.GetNextNonceAsync(
        Address address, CancellationToken cancellationToken)
    {
        if (IsRunning is false || _blockChain is null)
        {
            throw new InvalidOperationException("BlockChain is not running.");
        }

        return _blockChain.GetNextNonceAsync(address, cancellationToken);
    }

    Task<IValue> IBlockChain.GetStateAsync(
        long height,
        Address accountAddress,
        Address address,
        CancellationToken cancellationToken)
    {
        if (IsRunning is false || _blockChain is null)
        {
            throw new InvalidOperationException("BlockChain is not running.");
        }

        return _blockChain.GetStateAsync(height, accountAddress, address, cancellationToken);
    }

    Task<IValue> IBlockChain.GetStateAsync(
        BlockHash blockHash,
        Address accountAddress,
        Address address,
        CancellationToken cancellationToken)
    {
        if (IsRunning is false || _blockChain is null)
        {
            throw new InvalidOperationException("BlockChain is not running.");
        }

        return _blockChain.GetStateAsync(blockHash, accountAddress, address, cancellationToken);
    }

    Task<IValue> IBlockChain.GetStateAsync(
        HashDigest<SHA256> stateRootHash,
        Address accountAddress,
        Address address,
        CancellationToken cancellationToken)
    {
        if (IsRunning is false || _blockChain is null)
        {
            throw new InvalidOperationException("BlockChain is not running.");
        }

        return _blockChain.GetStateAsync(
            stateRootHash, accountAddress, address, cancellationToken);
    }

    Task<BlockHash> IBlockChain.GetBlockHashAsync(
        long height, CancellationToken cancellationToken)
    {
        if (IsRunning is false || _blockChain is null)
        {
            throw new InvalidOperationException("BlockChain is not running.");
        }

        return _blockChain.GetBlockHashAsync(height, cancellationToken);
    }

    Task<T> IBlockChain.GetActionAsync<T>(
        TxId txId, int actionIndex, CancellationToken cancellationToken)
    {
        if (IsRunning is false || _blockChain is null)
        {
            throw new InvalidOperationException("BlockChain is not running.");
        }

        return _blockChain.GetActionAsync<T>(txId, actionIndex, cancellationToken);
    }

    private void UpdateCurrent(Node? node)
    {
        if (_blockChain is not null)
        {
            _blockChain.Started -= BlockChain_Started;
            _blockChain.Stopped -= BlockChain_Stopped;
            _blockChain.BlockAppended -= BlockChain_BlockAppended;
            if (_blockChain.IsRunning is false)
            {
                Tip = BlockInfo.Empty;
                IsRunning = false;
                _logger.LogDebug("BlockChain is stopped.");
                Stopped?.Invoke(this, EventArgs.Empty);
            }
        }

        _node = node;
        _blockChain = node?.GetKeyedService<IBlockChain>(INode.Key);

        if (_blockChain is not null)
        {
            if (_blockChain.IsRunning is true)
            {
                Tip = _blockChain.Tip;
                IsRunning = true;
                _logger.LogDebug("BlockChain is started.");
                Started?.Invoke(this, EventArgs.Empty);
            }

            _blockChain.Started += BlockChain_Started;
            _blockChain.Stopped += BlockChain_Stopped;
            _blockChain.BlockAppended += BlockChain_BlockAppended;
        }
    }

    private void Nodes_CurrentChanged(object? sender, EventArgs e)
        => UpdateCurrent(_nodes.Current);

    private void BlockChain_BlockAppended(object? sender, BlockEventArgs e)
    {
        Tip = e.BlockInfo;
        BlockAppended?.Invoke(sender, e);
    }

    private void BlockChain_Started(object? sender, EventArgs e)
    {
        if (sender is IBlockChain blockChain && blockChain == _blockChain)
        {
            Tip = _blockChain.Tip;
            IsRunning = true;
            _logger.LogDebug("BlockChain is started.");
            Started?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            throw new UnreachableException("The sender is not an instance of IBlockChain.");
        }
    }

    private void BlockChain_Stopped(object? sender, EventArgs e)
    {
        Tip = BlockInfo.Empty;
        IsRunning = false;
        _logger.LogDebug("BlockChain is stopped.");
        Stopped?.Invoke(this, EventArgs.Empty);
    }
}
