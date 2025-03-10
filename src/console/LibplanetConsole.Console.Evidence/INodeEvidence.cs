using Libplanet.Types.Evidence;
using LibplanetConsole.Evidence;

namespace LibplanetConsole.Console.Evidence;

internal interface INodeEvidence
{
    Task<EvidenceId> AddEvidenceAsync(CancellationToken cancellationToken);

    Task<EvidenceInfo[]> GetEvidenceAsync(long height, CancellationToken cancellationToken);

    Task<EvidenceInfo> GetEvidenceAsync(EvidenceId evidenceId, CancellationToken cancellationToken);

    Task<EvidenceInfo[]> GetPendingEvidenceAsync(CancellationToken cancellationToken);

    Task<EvidenceInfo> GetPendingEvidenceAsync(
        EvidenceId evidenceId, CancellationToken cancellationToken);

    Task ViolateAsync(string type, CancellationToken cancellationToken);
}
