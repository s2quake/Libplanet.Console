#if LIBPLANET_NODE || LIBPLANET_CONSOLE
using Libplanet.Types.Evidence;
using LibplanetConsole.Evidence.Grpc;

namespace LibplanetConsole.Evidence;

public readonly record struct EvidenceInfo
{
    public string Type { get; init; }

    public string Id { get; init; }

    public Address TargetAddress { get; init; }

    public long Height { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public static explicit operator EvidenceInfo(EvidenceBase evidence) => new()
    {
        Type = evidence.GetType().Name,
        Id = evidence.Id.ToString(),
        Height = evidence.Height,
        TargetAddress = evidence.TargetAddress,
        Timestamp = evidence.Timestamp,
    };

    public static implicit operator EvidenceInfo(EvidenceInfoProto evidenceInfo) => new()
    {
        Type = evidenceInfo.Type,
        Id = evidenceInfo.Id,
        TargetAddress = new Address(evidenceInfo.TargetAddress),
        Height = evidenceInfo.Height,
        Timestamp = evidenceInfo.Timestamp.ToDateTimeOffset(),
    };

    public static implicit operator EvidenceInfoProto(EvidenceInfo evidenceInfo) => new()
    {
        Type = evidenceInfo.Type,
        Id = evidenceInfo.Id,
        TargetAddress = evidenceInfo.TargetAddress.ToHex(),
        Height = evidenceInfo.Height,
        Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(
                evidenceInfo.Timestamp),
    };
}
#endif // LIBPLANET_NODE || LIBPLANET_CONSOLE
