using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NekoLib.Extra;

public static class Logging {
    private static ILoggerFactory? _factory;
    
    public static void Init(Action<ILoggingBuilder> config) {
        _factory = LoggerFactory.Create(config);
    }
    
    public static ILogger GetFor(string name) {
        if (_factory is null) 
            return NullLogger.Instance;
        return _factory.CreateLogger(name);
    }
}