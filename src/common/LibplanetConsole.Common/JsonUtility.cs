using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using LibplanetConsole.Common.IO;

namespace LibplanetConsole.Common;

public static class JsonUtility
{
    private static readonly bool IsJQSupported = SupportsJQ();
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { ExcludeEmptyStrings },
        },
    };

    public static string Serialize(object value)
        => JsonSerializer.Serialize(value, SerializerOptions);

    public static string Serialize(object value, bool isColorized)
    {
        var json = Serialize(value);
        if (isColorized == true)
        {
            return ToColorizedString(json);
        }

        return json;
    }

    public static string ToColorizedString(string json)
    {
        if (IsJQSupported == true)
        {
            using var tempFile = TempFile.WriteAllText(json);
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = $"jq";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = $". -C \"{tempFile.FileName}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }

        return json;
    }

    public static T Deserialize<T>(string value)
    {
        if (JsonSerializer.Deserialize<T>(value, SerializerOptions) is T t)
        {
            return t;
        }

        throw new ArgumentException("Cannot deserialize the object.", nameof(value));
    }

    private static bool SupportsJQ()
    {
        try
        {
            if (OperatingSystem.IsMacOS() == true && Console.IsOutputRedirected is false)
            {
                var process = new Process();
                process.StartInfo.FileName = $"which";
                process.StartInfo.Arguments = "jq";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += (s, e) => { };
                process.ErrorDataReceived += (s, e) => { };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    private static void ExcludeEmptyStrings(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var property in jsonTypeInfo.Properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    property.ShouldSerialize = ShouldSerialize;
                }
            }
        }

        static bool ShouldSerialize(object obj, object? value)
            => value is string @string && string.IsNullOrEmpty(@string) is false;
    }
}
