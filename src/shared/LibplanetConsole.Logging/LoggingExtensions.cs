using Serilog;

namespace LibplanetConsole.Logging;

internal static class LoggingExtensions
{
    public static IServiceCollection AddLogging(
        this IServiceCollection @this, string logPath, string name, params LoggingFilter[] filters)
    {
        if (logPath == string.Empty)
        {
            throw new ArgumentException("Log path must be specified.", nameof(logPath));
        }

        if (File.Exists(logPath) is true)
        {
            throw new ArgumentException("Log path must be a directory.", nameof(logPath));
        }

        if (name == string.Empty)
        {
            throw new ArgumentException("Name must be specified.", nameof(name));
        }

        var loggerConfiguration = new LoggerConfiguration();
        var logFilename = Path.Combine(logPath, name);
        loggerConfiguration = loggerConfiguration.MinimumLevel.Debug();
        loggerConfiguration = loggerConfiguration
            .WriteTo.Logger(lc => lc.WriteTo.File(logFilename));

        foreach (var filter in filters)
        {
            var filename = Path.Combine(logPath, filter.Name);
            loggerConfiguration = loggerConfiguration
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(filter.Filter)
                    .WriteTo.File(filename));
        }

        Log.Logger = loggerConfiguration.CreateLogger();
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception occurred.");
        };

        @this.AddSingleton<ILoggerFactory, LoggerFactory>();
        @this.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        return @this;
    }

    public static IServiceCollection AddLogging(
        this IServiceCollection @this, params LoggingFilter[] filters)
    {
        var loggerConfiguration = new LoggerConfiguration();
        loggerConfiguration = loggerConfiguration.MinimumLevel.Debug();

        foreach (var filter in filters)
        {
            loggerConfiguration = loggerConfiguration
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(filter.Filter)
                    .WriteTo.Trace());
        }

        Log.Logger = loggerConfiguration.CreateLogger();
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception occurred.");
        };

        @this.AddSingleton<ILoggerFactory, LoggerFactory>();
        @this.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        return @this;
    }
}
