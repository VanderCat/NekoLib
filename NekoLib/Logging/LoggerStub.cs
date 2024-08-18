namespace NekoLib.Logging; 

internal class LoggerStub : ILogger{
    public void Log(LogLevel logLevel, string message) { }

    public void Log(LogLevel logLevel, string template, params object[] stuff) { }
}