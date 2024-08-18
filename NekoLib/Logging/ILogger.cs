using NekoLib.Annotations;

namespace NekoLib.Logging; 

/// <summary>
/// This interface is used for internal logging inside NekoLib. It is expected for you to hook up your
/// logging system there. By default it does not printing anything, you can use ConsoleLogger, if you feel lazy
/// enough. You can use this for you projects instead of something like serilog, but keep in mind this is a barebones
/// implementation of a Logger interface, and you are advised to use something different instead.
/// </summary>
public interface ILogger {
    /// <summary>
    /// SET ME
    /// </summary>
    public static ILogger Logger = new LoggerStub();
    public void Log(LogLevel logLevel, string message);
    [StringFormatMethod("template")]
    public void Log(LogLevel logLevel, string template, params object[] stuff);

    public static void Trace(string message) =>
        Logger.Log(LogLevel.Trace, message);
    [StringFormatMethod("message")]
    public static void Trace(string message, params object[] stuff) =>
        Logger.Log(LogLevel.Trace, message, stuff);
    
    public static void Debug(string message) =>
        Logger.Log(LogLevel.Debug, message);
    [StringFormatMethod("message")]
    public static void Debug(string message, params object[] stuff) =>
        Logger.Log(LogLevel.Debug, message, stuff);
    
    public static void Info(string message) =>
        Logger.Log(LogLevel.Info, message);
    [StringFormatMethod("message")]
    public static void Info(string message, params object[] stuff) =>
        Logger.Log(LogLevel.Info, message, stuff);
    
    public static void Warning(string message) =>
        Logger.Log(LogLevel.Warning, message);
    [StringFormatMethod("message")]
    public static void Warning(string message, params object[] stuff) =>
        Logger.Log(LogLevel.Warning, message, stuff);
    
    public static void Error(string message) =>
        Logger.Log(LogLevel.Error, message);
    [StringFormatMethod("message")]
    public static void Error(string message, params object[] stuff) =>
        Logger.Log(LogLevel.Error, message, stuff);
    
    public static void Fatal(string message) =>
        Logger.Log(LogLevel.Fatal, message);
    [StringFormatMethod("message")]
    public static void Fatal(string message, params object[] stuff) =>
        Logger.Log(LogLevel.Fatal, message, stuff);
}