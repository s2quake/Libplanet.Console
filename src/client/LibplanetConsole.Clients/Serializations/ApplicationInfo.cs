using LibplanetConsole.Common;

namespace LibplanetConsole.Clients.Serializations;

public readonly record struct ApplicationInfo
{
    public required AppEndPoint EndPoint { get; init; }

    public AppEndPoint? NodeEndPoint { get; init; }

    public required string LogPath { get; init; }
}
