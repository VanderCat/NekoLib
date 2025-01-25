using Microsoft.Extensions.Logging;

namespace NekoLib.Extra;

[ProviderAlias("NekoLibConsole")]
public sealed class ConsoleLoggerProvider(ConsoleLoggerConfiguration config) : ILoggerProvider {
    private ConsoleLogger? _logger;

    public ILogger CreateLogger(string categoryName) {
        if (_logger is null) {
            return _logger = new ConsoleLogger(()=>config);
        }

        return _logger;
    }

    private ConsoleLoggerConfiguration GetCurrentConfig() => config;

    public void Dispose() { }
}

public sealed class ConsoleLoggerConfiguration {
    public int EventId { get; set; }

    public LogLevel MinLogLevel = LogLevel.Information;
}

public sealed class ConsoleLogger(Func<ConsoleLoggerConfiguration> getCurrentConfig) : ILogger {
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (!IsEnabled(logLevel))
            return;
        Console.Log(formatter(state, exception));
    }
    public bool IsEnabled(LogLevel logLevel) {
        return logLevel >= getCurrentConfig().MinLogLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}

public static class ConsoleLoggerSinkExtension {
    public static ILoggingBuilder AddNekoLibConsole(this ILoggingBuilder builder, ConsoleLoggerConfiguration config) {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.AddProvider(new ConsoleLoggerProvider(config));

        return builder;
    }

    public static ILoggingBuilder AddNekoLibConsole(this ILoggingBuilder builder) => builder.AddNekoLibConsole(new());
}