namespace LibplanetConsole.Client;

public sealed record class ApplicationOptions
{
    public ApplicationOptions(EndPoint endPoint, PrivateKey privateKey)
    {
        EndPoint = endPoint;
        PrivateKey = privateKey;
    }

    public EndPoint EndPoint { get; }

    public PrivateKey PrivateKey { get; }

    public int ParentProcessId { get; init; }

    public EndPoint? NodeEndPoint { get; init; }

    public string LogPath { get; init; } = string.Empty;

    public bool NoREPL { get; init; }

    public object[] Components { get; init; } = [];
}
