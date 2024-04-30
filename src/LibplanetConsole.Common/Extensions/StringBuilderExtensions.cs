using System.Text;

namespace LibplanetConsole.Common.Extensions;

public static class StringBuilderExtensions
{
    public static void AppendLines<T>(
        this StringBuilder @this, IEnumerable<T> items, Func<T, string> formatter)
    {
        foreach (var item in items)
        {
            @this.AppendLine(formatter(item));
        }
    }
}
