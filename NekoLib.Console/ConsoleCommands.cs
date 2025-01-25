using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace NekoLib.Extra;

public static partial class Console {
    [ConCommand("echo")]
    [ConDescription("prints text in the console")]
    public static void Echo(params string[] meow) {
        Logger.LogInformation(string.Join(' ', meow));
    }
    
    [ConCommand("test_cmd")]
    [ConDescription("meow")]
    [ConTags("test")]
    [Conditional("DEBUG")]
    public static void TestCommand(string meow, float wow = 12f, int hehe = 33, params string[]? columnthree) {
        Logger.LogInformation(meow);
        Logger.LogInformation("{0}", wow);
        Logger.LogInformation("{0}", hehe);
        if (columnthree is not null) {
            Logger.LogInformation("params is working uwu {Params}", columnthree);
        }
    }
    
    [ConCommand("dotnet_version")]
    [ConDescription("Print running .NET version")]
    public static void DotnetVersion() {
        Logger.LogInformation("Running " 
                                + RuntimeInformation.FrameworkDescription 
                                + " ("
                                + RuntimeInformation.RuntimeIdentifier
                                + ") On " 
                                + RuntimeInformation.OSDescription 
                                + " "
                                + RuntimeInformation.ProcessArchitecture);
    }
    
    [ConVariable("test_variable")]
    [ConTags("test")]
    public static bool TestVariable { get; set; }
    
    [ConCommand("help")]
    [ConDescription("List all available commands and it's description")]
    public static void Help() {
        var list = "Available commands: ";
        foreach (var command in _commands) {
            list += "\n" + command.Key + "(";
            var parameters = command.Value.GetParameters();
            for (var index = 0; index < parameters.Length; index++) {
                var parameter = parameters[index];
                var notLast = (index + 1 < parameters.Length);
                list += $"{parameter.ParameterType} {parameter.Name}";
                if (parameter.IsOptional) list += $" = {parameter.DefaultValue}";
                if (notLast) list += ", ";
            }
            
            list += ")\n\t";
            var desc = command.Value.GetCustomAttribute<ConDescriptionAttribute>();
            if (desc is not null) 
                list += desc.Description;
            else
                list += "No description.";
            var declType = command.Value.DeclaringType;
            if (declType is not null)
                list += " (From "+ declType.FullName+")";
        }

        list += "\nAvailable convars: ";
        foreach (var convars in _convars) {
            list += "\n" + convars.Key + " = ";
            try {
                list += convars.Value.GetValue(null);
            }
            catch (Exception e) {
                list += e.GetType().ToString();
            }

            list += "\n\t";
            var desc = convars.Value.GetCustomAttribute<ConDescriptionAttribute>();
            if (desc is not null) 
                list += desc.Description;
            else
                list += "No description.";
            var declType = convars.Value.DeclaringType;
            if (declType is not null)
                list += " (From "+ declType.FullName+")";
        }
        Logger.LogInformation(list);
    }
    
    [ConCommand("clear")]
    [ConDescription("Clears Console")]
    public static void Clear() {
        MessageLog.Clear();
    }

    [ConCommand("exec")]
    [ConDescription("run commands from thefile")]
    public static void ExecFile(string path) {
        if (_fs is null) {
            Logger.LogError("Console have not access to filesystem");
            return;
        }
        var virtualPath = Path.Combine("cfg", path + ".cfg");
        if (!_fs.Exists(virtualPath)) {
            Logger.LogError("No file {Path} found", path);
            return;
        }

        Submit(_fs.Read(virtualPath).Replace("\r\n", ";").Replace("\n", ";"));
    }

    public static bool CheatsWasEnabled { get; private set; }
    private static bool _cheatsEnabled;

    [ConVariable("sv_cheats")]
    [ConDescription("Enable cheats")]
    public static bool CheatsEnabled {
        get => _cheatsEnabled;
        set {
            CheatsWasEnabled = value || CheatsWasEnabled;
            _cheatsEnabled = value;
        }
    }
    
    [ConVariable("test_cheatvar")]
    [ConTags("cheat", "test")]
    public static bool TestCheatVar { get; set; }

    [ConCommand("test_cheat")]
    [ConTags("cheat", "test")]
    [Conditional("DEBUG")]
    public static void TestCheat() {
        Echo("cheat :3");
    }

    [ConTagHandler("cheat")]
    public static bool HandleCheat() => CheatsEnabled;

    public static List<string> History { get; } = new();

    [ConCommand("history")]
    public static void PrintHistory() {
        Logger.LogInformation("History:\n"+string.Join('\n', History));
    }
}