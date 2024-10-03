using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace LibplanetConsole.Framework;

public abstract class ApplicationFramework : IAsyncDisposable, IServiceProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ManualResetEvent _closeEvent = new(false);
    private readonly SynchronizationContext _synchronizationContext;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILogger _logger;
    private bool _isDisposed;

    protected ApplicationFramework(IServiceProvider serviceProvider)
    {
        SynchronizationContext.SetSynchronizationContext(new());
        _logger = serviceProvider.GetRequiredService<ILogger>();
        _serviceProvider = serviceProvider;
        _synchronizationContext = SynchronizationContext.Current!;
    }

    public abstract ILogger Logger { get; }

    protected virtual bool CanClose => false;

    public static ILogger CreateLogger(
        Type applicationType, string logPath, string libraryLogPath)
    {
        var loggerConfiguration = new LoggerConfiguration();
        loggerConfiguration = loggerConfiguration.MinimumLevel.Debug();
        if (logPath != string.Empty)
        {
            loggerConfiguration = loggerConfiguration
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => IsApplicationLog(applicationType, e))
                    .WriteTo.File(logPath));
        }

        if (libraryLogPath != string.Empty)
        {
            loggerConfiguration = loggerConfiguration
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(e => IsApplicationLog(applicationType, e))
                    .WriteTo.File(libraryLogPath));
        }

        Log.Logger = loggerConfiguration.CreateLogger();
        var logger = Log.ForContext(applicationType);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception occurred.");
        };

        return logger;
    }

    public Task InvokeAsync(Action action, CancellationToken cancellationToken)
    {
        return Task.Run(Action, cancellationToken);

        void Action() => _synchronizationContext.Send((state) => action(), null);
    }

    public Task<T> InvokeAsync<T>(Func<T> func, CancellationToken cancellationToken)
    {
        return Task.Run(Func, cancellationToken);

        T Func()
        {
            T result = default!;
            _synchronizationContext.Send((state) => result = func(), null);
            return result;
        }
    }

    public void Cancel()
    {
        Logger.Debug("Application canceled.");
        _cancellationTokenSource.Cancel();
        _closeEvent.Set();
    }

    public async Task RunAsync()
    {
        ObjectDisposedException.ThrowIf(_isDisposed == true, this);

        Logger.Debug("Application running...");
        await OnRunAsync(_cancellationTokenSource.Token);
        Logger.Debug("Application waiting for close...");
        await Task.Run(() =>
        {
            while (CanClose != true && _closeEvent.WaitOne(1) != true)
            {
                // Wait for close.
            }
        });
        Logger.Debug("Application run completed.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed is false)
        {
            Logger.Debug("Application disposing...");
            await OnDisposeAsync();
            _cancellationTokenSource.Dispose();
            _isDisposed = true;
            GC.SuppressFinalize(this);
            Logger.Debug("Application disposed.");
        }
    }

    public abstract object? GetService(Type serviceType);

    protected virtual async Task OnRunAsync(CancellationToken cancellationToken)
    {
        var applicationServices = Sort(_serviceProvider.GetServices<IApplicationService>());

        for (var i = 0; i < applicationServices.Length; i++)
        {
            var serviceName = applicationServices[i].GetType().Name;
            _logger?.Debug("Application service initializing: {0}", serviceName);
            await applicationServices[i].InitializeAsync(cancellationToken);
            _logger?.Debug("Application service initialized: {0}", serviceName);
        }
    }

    protected virtual ValueTask OnDisposeAsync() => ValueTask.CompletedTask;

    private static bool IsApplicationLog(Type applicationType, LogEvent e)
    {
        if (e.Properties.TryGetValue("SourceContext", out var propertyValue) is false)
        {
            return false;
        }

        if (propertyValue is not ScalarValue scalarValue)
        {
            return false;
        }

        if (scalarValue.Value is not string value)
        {
            return false;
        }

        return value == applicationType.FullName;
    }

    private static IApplicationService[] Sort(IEnumerable<IApplicationService> items)
    {
        return DependencyUtility.TopologicalSort(items, GetDependencies).ToArray();

        IEnumerable<IApplicationService> GetDependencies(IApplicationService item)
        {
            return DependencyUtility.GetDependencies(item, items);
        }
    }
}
