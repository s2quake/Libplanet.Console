using System.ComponentModel.Composition;
using JSSoft.Commands;

namespace LibplanetConsole.ConsoleHost.Commands;

[Export(typeof(ICommand))]
[Export(typeof(VersionCommand))]
internal sealed class VersionCommand : VersionCommandBase
{
}
