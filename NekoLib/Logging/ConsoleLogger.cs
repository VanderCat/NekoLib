namespace NekoLib.Logging; 

public class ConsoleLogger : ILogger {
    public void Log(LogLevel logLevel, string message) {
        Console.WriteLine($"NekoLib: [{logLevel}] {message}");
    }

    public void Log(LogLevel logLevel, string template, params object[] stuff) {
        Log(logLevel, string.Format(template, stuff));
    }
}