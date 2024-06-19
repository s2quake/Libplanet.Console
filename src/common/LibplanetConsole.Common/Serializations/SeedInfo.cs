using Libplanet.Crypto;

namespace LibplanetConsole.Common.Serializations;

public readonly record struct SeedInfo
{
    public static SeedInfo Empty { get; } = default;

    public GenesisInfo GenesisInfo { get; init; }

    public string BlocksyncSeedPeer { get; init; }

    public string ConsensusSeedPeer { get; init; }

    public SeedInfo Encrypt(PublicKey publicKey) => this with
    {
        GenesisInfo = GenesisInfo.Encrypt(publicKey),
    };

    public SeedInfo Decrypt(PrivateKey privateKey) => this with
    {
        GenesisInfo = GenesisInfo.Decrypt(privateKey),
    };
}
