using System.ComponentModel.Composition;
using LibplanetConsole.Common;

namespace LibplanetConsole.Nodes;

[Export(typeof(IInfoProvider))]
internal sealed class ApplicationInfoProvider : IInfoProvider
{
    public bool CanSupport(Type type) => typeof(ApplicationBase).IsAssignableFrom(type);

    public IEnumerable<(string Name, object? Value)> GetInfos(object obj)
    {
        if (obj is ApplicationBase application)
        {
            return InfoUtility.EnumerateValues(application.Info);
        }

        throw new NotSupportedException("The object is not supported.");
    }
}
