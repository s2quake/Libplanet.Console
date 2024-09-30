using System.ComponentModel;
using JSSoft.Commands;
using Libplanet.Common;
using LibplanetConsole.Common;
using LibplanetConsole.Common.DataAnnotations;
using LibplanetConsole.Common.Extensions;
using LibplanetConsole.Common.IO;
using LibplanetConsole.DataAnnotations;

namespace LibplanetConsole.Node.Executable.EntryCommands;

[CommandSummary("Create a new repository to run the node")]
internal sealed class InitializeCommand : CommandBase
{
    public InitializeCommand()
        : base("init")
    {
    }

    [CommandPropertyRequired]
    [CommandSummary("The directory path used to initialize a repository.")]
    [Path(Type = PathType.Directory, ExistsType = PathExistsType.NotExistOrEmpty)]
    public string RepositoryPath { get; set; } = string.Empty;

    [CommandProperty]
    [CommandSummary("Indicates the private key of the node. " +
                    "If omitted, a random private key is used.")]
    [PrivateKey]
    public string PrivateKey { get; init; } = string.Empty;

    [CommandProperty]
    [CommandSummary("The endpoint of the node. " +
                    "If omitted, a random endpoint is used.")]
    [EndPoint]
    public string EndPoint { get; set; } = string.Empty;

    [CommandProperty]
    [CommandSummary("The directory path to store the block. " +
                    "If omitted, the 'store' directory is used.")]
    [Path(
        Type = PathType.Directory, ExistsType = PathExistsType.NotExistOrEmpty, AllowEmpty = true)]
    public string StorePath { get; set; } = string.Empty;

    [CommandProperty]
    [CommandSummary("The file path to store the application logs." +
                    "If omitted, the 'app.log' file is used.")]
    [Path(Type = PathType.File, ExistsType = PathExistsType.NotExistOrEmpty, AllowEmpty = true)]
    public string LogPath { get; set; } = string.Empty;

    [CommandProperty]
    [CommandSummary("The file path to store logs other than application logs." +
                    "If omitted, the 'library.log' file is used.")]
    [Path(Type = PathType.File, ExistsType = PathExistsType.NotExistOrEmpty, AllowEmpty = true)]
    public string LibraryLogPath { get; set; } = string.Empty;

    [CommandProperty]
    [CommandSummary("The file path of the genesis." +
                    "If omitted, the 'genesis' file is used.")]
    [Path(Type = PathType.File, AllowEmpty = true)]
    public string GenesisPath { get; set; } = string.Empty;

    [CommandPropertySwitch("single-node")]
    [CommandSummary("If set, the repository is created in a format suitable for a single node.")]
    public bool IsSingleNode { get; set; }

    [CommandProperty]
    [CommandSummary("The private key of the genesis block. " +
                    "if omitted, a random private key is used.\n" +
                    "Requires the '--single-node' option to be set.")]
    [CommandPropertyDependency(nameof(IsSingleNode))]
    [PrivateKey]
    [Category("Genesis")]
    public string GenesisKey { get; set; } = string.Empty;

    [CommandProperty("timestamp")]
    [CommandSummary("The timestamp of the genesis block. ex) \"2021-01-01T00:00:00Z\"\n" +
                    "Requires the '--single-node' option to be set.")]
    [Category("Genesis")]
    [CommandPropertyDependency(nameof(IsSingleNode))]
    public DateTimeOffset DateTimeOffset { get; set; }

    [CommandPropertySwitch("quiet", 'q')]
    [CommandSummary("If set, the command does not output any information.")]
    public bool Quiet { get; set; }

    [CommandProperty("module-path")]
    [CommandSummary("Indicates the path or the name of the assembly that provides " +
                    "the IActionProvider.\n" +
                    "Requires the '--single-node' option to be set.")]
    [Category("Genesis")]
    [CommandPropertyDependency(nameof(IsSingleNode))]
    public string ActionProviderModulePath { get; set; } = string.Empty;

    [CommandProperty("module-type")]
    [CommandSummary("Indicates the type name of the IActionProvider.\n" +
                    "Requires the '--single-node' option to be set.")]
    [CommandExample("--module-type 'LibplanetModule.SimpleActionProvider, LibplanetModule'")]
    [Category("Genesis")]
    [CommandPropertyDependency(nameof(IsSingleNode))]
    public string ActionProviderType { get; set; } = string.Empty;

    protected override void OnExecute()
    {
        var outputPath = Path.GetFullPath(RepositoryPath);
        var endPoint = AppEndPoint.ParseOrNext(EndPoint);
        var privateKey = PrivateKeyUtility.ParseOrRandom(PrivateKey);
        var storePath = Path.Combine(outputPath, StorePath.Fallback("store"));
        var logPath = Path.Combine(outputPath, LogPath.Fallback("app.log"));
        var libraryLogPath = Path.Combine(outputPath, LibraryLogPath.Fallback("library.log"));
        var genesisPath = Path.Combine(outputPath, GenesisPath.Fallback("genesis"));
        var repository = new Repository
        {
            EndPoint = endPoint,
            PrivateKey = privateKey,
            StorePath = storePath,
            LogPath = logPath,
            LibraryLogPath = libraryLogPath,
            GenesisPath = genesisPath,
            SeedEndPoint = IsSingleNode is true ? endPoint : null,
            ActionProviderModulePath = ActionProviderModulePath,
            ActionProviderType = ActionProviderType,
        };
        dynamic info = repository.Save(outputPath);
        using var writer = new ConditionalTextWriter(Out)
        {
            Condition = Quiet is false,
        };

        if (IsSingleNode is true)
        {
            var genesisOptions = new GenesisOptions
            {
                GenesisKey = PrivateKeyUtility.ParseOrRandom(GenesisKey),
                Validators = [privateKey.PublicKey],
                Timestamp = DateTimeOffset != DateTimeOffset.MinValue
                    ? DateTimeOffset : DateTimeOffset.UtcNow,
                ActionProviderModulePath = ActionProviderModulePath,
                ActionProviderType = ActionProviderType,
            };

            var genesisBlock = BlockUtility.CreateGenesisBlock(genesisOptions);
            var genesis = BlockUtility.SerializeBlock(genesisBlock);
            var genesisString = ByteUtil.Hex(genesis);
            File.WriteAllLines(genesisPath, [genesisString]);
            info.GenesisArguments = new
            {
                GenesisKey = PrivateKeyUtility.ToString(genesisOptions.GenesisKey),
                Validators = genesisOptions.Validators.Select(
                    item => item.ToHex(compress: false)),
                genesisOptions.Timestamp,
                genesisOptions.ActionProviderModulePath,
                genesisOptions.ActionProviderType,
            };
            info.Genesis = genesisString;
        }

        TextWriterExtensions.WriteLineAsJson(writer, info);
    }
}
