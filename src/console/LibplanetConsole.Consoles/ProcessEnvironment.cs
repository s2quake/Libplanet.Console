using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LibplanetConsole.Common.Extensions;
using static System.Runtime.InteropServices.RuntimeInformation;

namespace LibplanetConsole.Consoles;

internal static class ProcessEnvironment
{
    private const string WorkspacePathVariableName = "LIBPLANET_CONSOLE_WORKSPACE_PATH";
    private const string NodePathVariableName = "LIBPLANET_CONSOLE_NODE_PATH";
    private const string ClientPathVariableName = "LIBPLANET_CONSOLE_CLIENT_PATH";
    private const string DotnetRootVariableName = "DOTNET_ROOT";
    private const string Framework = "net8.0";

#if DEBUG
    private const string Congiguration = "Debug";
#elif RELEASE
    private const string Congiguration = "Release";
#endif

    public static string WorkspacePath
    {
        get
        {
            if (Environment.GetEnvironmentVariable(WorkspacePathVariableName) is { } workspacePath)
            {
                if (Directory.Exists(workspacePath) != true)
                {
                    var message =
                        $"Directory '{workspacePath}' of environment variable " +
                        $"'{WorkspacePathVariableName}' does not exist.";
                    throw new InvalidOperationException(message);
                }

                return Regex.Replace(workspacePath, "[/\\\\]$", string.Empty);
            }
            else
            {
                return Environment.CurrentDirectory;
            }
        }
    }

    public static string NodePath
    {
        get
        {
            if (Environment.GetEnvironmentVariable(NodePathVariableName) is { } nodePath)
            {
                if (File.Exists(nodePath) != true)
                {
                    var message =
                        $"File '{nodePath}' of environment variable " +
                        $"'{NodePathVariableName}' does not exist.";
                    throw new FileNotFoundException(message);
                }

                return nodePath;
            }

            var actualNodePath = IsInProject ? NodePathInProject : NodePathInBin;
            if (File.Exists(actualNodePath) != true)
            {
                var message = $"File '{actualNodePath}' does not exist.";
                throw new FileNotFoundException(message);
            }

            return actualNodePath;
        }
    }

    public static string ClientPath
    {
        get
        {
            if (Environment.GetEnvironmentVariable(ClientPathVariableName) is { } clientPath)
            {
                if (File.Exists(clientPath) != true)
                {
                    var message =
                        $"File '{clientPath}' of environment variable " +
                        $"'{ClientPathVariableName}' does not exist.";
                    throw new FileNotFoundException(message);
                }

                return clientPath;
            }

            var actualClientPath = IsInProject ? ClientPathInProject : ClientPathInBin;
            if (File.Exists(actualClientPath) != true)
            {
                var message = $"File '{actualClientPath}' does not exist.";
                throw new FileNotFoundException(message);
            }

            return actualClientPath;
        }
    }

    public static string PublishExtension => IsWindows ? ".exe" : ".dll";

    public static string DotnetPath
    {
        get
        {
            var dotnetRoot = Environment.GetEnvironmentVariable(DotnetRootVariableName);
            if (dotnetRoot is not null)
            {
                var dotnetRootPath = GetDotnetPathFromDirectory(dotnetRoot);
                if (File.Exists(dotnetRootPath) == true)
                {
                    return dotnetRootPath;
                }

                throw new InvalidOperationException(
                    $"dotnet executable is not found in {DotnetRootVariableName}.");
            }

            var dotnetPath = GetDotnetPath();
            if (File.Exists(dotnetPath) == true)
            {
                return dotnetPath;
            }

            var dotnetRootInHomeDirectory = Path.Combine(HomeDirectory, ".dotnet");
            var dotnetPathInHomeDirectory = GetDotnetPathFromDirectory(dotnetRootInHomeDirectory);
            if (File.Exists(dotnetPathInHomeDirectory) == true)
            {
                return dotnetPathInHomeDirectory;
            }

            var message = $"dotnet executable is not found.\n" +
                          $"if you have installed dotnet, please set {DotnetRootVariableName} " +
                          $"environment variable to the directory containing dotnet executable.";
            throw new InvalidOperationException(message);
        }
    }

    public static string HomeDirectory
    {
        get
        {
            var isWindows = IsOSPlatform(OSPlatform.Windows);
            var variable = isWindows ? "USERPROFILE" : "HOME";
            var homeDirectory = Environment.GetEnvironmentVariable(variable);
            if (homeDirectory is not null)
            {
                return homeDirectory;
            }

            throw new InvalidOperationException($"Environment variable '{variable}' is not found.");
        }
    }

    public static bool IsOSX => IsOSPlatform(OSPlatform.OSX);

    public static bool IsWindows => IsOSPlatform(OSPlatform.Windows);

    public static bool IsLinux => IsOSPlatform(OSPlatform.Linux);

    public static bool IsArm64 => OSArchitecture == Architecture.Arm64;

    public static bool IsDotnetRuntime => Environment.ProcessPath == DotnetPath;

    private static bool IsInProject
    {
        get
        {
            var location = Assembly.GetExecutingAssembly().Location ??
                throw new InvalidOperationException("Executing assembly location is not found.");
            var directory = Path.GetDirectoryName(location) ??
                throw new InvalidOperationException(
                    $"Directory of the executing assembly location '{location}' is not found.");
            var expectedDirectory = $"{WorkspacePath}/src/console/" +
                                    $"LibplanetConsole.Consoles.Executable/" +
                                    $"bin/{Congiguration}/{Framework}";
            var d1 = Path.GetFullPath(expectedDirectory);
            var d2 = Path.GetFullPath(directory);
            if (IsOSPlatform(OSPlatform.Windows) == true)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(d1, d2);
            }

            return d1 == d2;
        }
    }

    private static string NodePathInProject
    {
        get
        {
            return Path.GetFullPath(
                $"{WorkspacePath}/src/node/LibplanetConsole.Nodes.Executable/bin/{Congiguration}/" +
                $"{Framework}/libplanet-node.dll");
        }
    }

    private static string NodePathInBin
    {
        get
        {
            if (Environment.ProcessPath is { } processPath)
            {
                var directoryName = Path.GetDirectoryName(processPath) ??
                    throw new InvalidOperationException(
                        $"Directory of the process path '{processPath}' is not found.");
                var extension = Path.GetExtension(processPath) ??
                    throw new InvalidOperationException(
                        $"Extension of the process path '{processPath}' is not found.");
                return Path.Combine(directoryName, $"libplanet-node{extension}");
            }

            throw new InvalidOperationException("Environment.ProcessPath is not found.");
        }
    }

    private static string ClientPathInProject
    {
        get
        {
            return Path.GetFullPath(
                $"{WorkspacePath}/src/client/LibplanetConsole.Clients.Executable/bin/" +
                $"{Congiguration}/{Framework}/libplanet-client.dll");
        }
    }

    private static string ClientPathInBin
    {
        get
        {
            if (Environment.ProcessPath is { } processPath)
            {
                var directoryName = Path.GetDirectoryName(processPath) ??
                    throw new InvalidOperationException(
                        $"Directory of the process path '{processPath}' is not found.");
                var extension = Path.GetExtension(processPath) ??
                    throw new InvalidOperationException(
                        $"Extension of the process path '{processPath}' is not found.");
                return Path.Combine(directoryName, $"libplanet-client{extension}");
            }

            throw new InvalidOperationException("Environment.ProcessPath is not found.");
        }
    }

    public static ImmutableArray<string> GetArguments(
            IEnumerable<IProcessArgumentProvider> argumentProviders, object obj)
    {
        var query = from argumentProvider in argumentProviders
                    where argumentProvider.CanSupport(obj.GetType())
                    from arguments in argumentProvider.GetArguments(obj)
                    select arguments;

        return [.. query];
    }

    public static ImmutableArray<string> GetArguments(
        IServiceProvider serviceProvider, object obj)
    {
        var argumentProviders = serviceProvider.GetService<IEnumerable<IProcessArgumentProvider>>();
        return GetArguments(argumentProviders, obj);
    }

    private static string GetDotnetPath()
    {
        var processStartInfo = new ProcessStartInfo();
        var process = new Process { StartInfo = processStartInfo };

        if (IsWindows == true)
        {
            processStartInfo.FileName = "powershell";
            processStartInfo.Arguments
                = "-Command 'Get-Command dotnet | Select-Object -ExpandProperty Source'";
        }
        else if (IsOSX == true || IsLinux == true)
        {
            processStartInfo.FileName = "which";
            processStartInfo.Arguments = "dotnet";
        }
        else
        {
            throw new NotSupportedException("Unsupported OS platform.");
        }

        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;
        process.Start();
        var error = process.StandardError.ReadToEnd();
        var @out = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("Failed to get dotnet path.\n" + error);
        }

        return @out.Trim();
    }

    private static string GetDotnetPathFromDirectory(string directory)
    {
        if (IsOSPlatform(OSPlatform.Windows) == true)
        {
            return Path.Combine(directory, "dotnet.exe");
        }

        return Path.Combine(directory, "dotnet");
    }
}
