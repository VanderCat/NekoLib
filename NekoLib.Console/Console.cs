using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NekoLib.Extra; 

public static partial class Console {
    [ConVariable("max_messages")]
    public static int MaxMessageCount { get; set; } = 256;

    public static Queue<string> MessageLog { get; } = new();

    static Console() {
        ConsoleHotReloadService.OnUpdateApplication += _ => RegisterAll();
    }

    internal static ILogger Logger;
    private static IConsoleFilesystem? _fs;
    public static void Init(ILogger? logger = null, IConsoleFilesystem? fs = null) {
        Logger = logger??NullLogger.Instance;
        _fs = fs;
        RegisterAll();
    }

    public static void Log(string message) {
        while (MessageLog.Count > MaxMessageCount) {
            MessageLog.Dequeue();
        }
        MessageLog.Enqueue(message);
    }
    
    private static Dictionary<string, MethodInfo> _commands = new();
    private static Dictionary<string, PropertyInfo> _convars = new();
    private static Dictionary<string, MethodInfo> _tagHandlers = new();

    public static List<string> CommandList {
        get {
            var candidates = _commands.Keys.ToList();
            candidates.AddRange(_convars.Keys);
            return candidates;
        }
    }
    
    public static void Submit(string commandline) {
        var regex = SubmitRegex();
        var commands = regex.Split(commandline).Select(com => com.Replace("\\;", ";")).ToArray();
        foreach (var command in commands) {
            var args = command.Split(" ").ToList();
            args.RemoveAll(s => s == "");
            if (args.Count <= 0) continue;
            var commandName = args[0];
            args.RemoveAt(0);
            try {
                if (_convars.ContainsKey(commandName)) {
                    if (args.Count <= 0) {
                        PrintVariable(commandName);
                        continue;
                    }

                    SubmitVariable(commandName, args[0]);
                    continue;
                }

                SubmitCommand(commandName, args.Cast<object>().ToArray());
            }
            catch (Exception e) {
                Logger.LogError("Command failed with error {Exception}", e);
            }
        }
    }

    public static void PrintVariable(string variable) {
        if (!_convars.TryGetValue(variable, out var value)) {
            Logger.LogError("Unknown variable {Variable}", variable);
            return;
        }
        if (value.GetMethod is null) {
            Logger.LogError("{Variable} does not have getter", variable);
            return;
        }
        Logger.LogInformation("{Variable} is set to {Value}", 
            variable, 
            value.GetValue(null));
    }

    public static void SubmitVariable(string variable, object? arg) {
        if (!_convars.TryGetValue(variable, out var convar)) {
            Logger.LogError("Unknown variable {Variable}", variable);
            return;
        }
        
        List<string> commandTags = new();
        if (Attribute.IsDefined(convar, typeof(ConTagsAttribute))) {
            commandTags = convar.GetCustomAttribute<ConTagsAttribute>()!.Tags;
        }

        foreach (var tag in commandTags) {
            if (!_tagHandlers.TryGetValue(tag, out var tagHandler)) continue;
            if (!(bool) (tagHandler.Invoke(null, null) ?? true)) {
                return;
            }
        }
        
        if (convar.SetMethod is null) {
            Logger.LogError("{Variable} does not have setter", variable);
            return;
        }
        arg = ConvertValue(arg, convar.PropertyType);
        convar.SetValue(null, arg);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object? ConvertValue(object? obj, Type type) {
        if (type is null) throw new ArgumentNullException(nameof(type));
        ArgumentNullException.ThrowIfNull(type);
        
        if (!type.IsNullable()) return Convert.ChangeType(obj, type);
        
        if (obj is null) return null;
        
        type = new NullableConverter(type).UnderlyingType;
        return Convert.ChangeType(obj, type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T? ConvertValue<T>(object? obj, Type type) => (T?)ConvertValue(obj, type);

    public static void SubmitCommand(string command, params object?[]? args) {
        if (!_commands.TryGetValue(command, out var conCommand)) {
            Logger.LogError("Unknown command {CommandName}", command);
            return;
        }

        List<string> commandTags = new();
        if (Attribute.IsDefined(conCommand, typeof(ConTagsAttribute))) {
            commandTags = conCommand.GetCustomAttribute<ConTagsAttribute>()!.Tags;
        }

        foreach (var tag in commandTags) {
            if (!_tagHandlers.TryGetValue(tag, out var tagHandler)) continue;
            if (!(bool) (tagHandler.Invoke(null, null) ?? true)) {
                return;
            }
        }
        var argList = (args??Array.Empty<object?>()).ToList();
        var param = conCommand.GetParameters();
        for (var index = 0; index < param.Length; index++) {
            var parameter = param[index];
            if (Attribute.IsDefined(parameter, typeof(ParamArrayAttribute))) {
                var newArgList = argList[..index];
                var paramsArg = argList[index..].ToArray();
                var typedParamsArg = Array.CreateInstance(parameter.ParameterType.GetElementType(), paramsArg.Length);
                Array.Copy(paramsArg, typedParamsArg, paramsArg.Length);
                newArgList.Add(typedParamsArg);
                argList = newArgList;
                break;
            }
            if (index >= argList.Count) {
                if (parameter.IsOptional) {
                    argList.Add(parameter.DefaultValue);
                    continue;
                }
                Logger.LogError("Parameter count mismatch in {CommandName}. Missing {Parameter}", command, parameter);
                return;
            }
            if (argList.Count > param.Length && !Attribute.IsDefined(param[^1], typeof(ParamArrayAttribute))) {
                Logger.LogError("Parameter count mismatch in {CommandName}. Too Many Arguments", command);
                return;
            }
            argList[index] = ConvertValue(argList[index], parameter.ParameterType);
        }

        conCommand.Invoke(null, argList.ToArray());
    }


    private static void RegisterAll() {
        _commands = new();
        _convars = new();
        _tagHandlers = new();
        
        var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        
        var assemblyTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(domainAssembly => domainAssembly.GetTypes());

        var allMethods = assemblyTypes.SelectMany(type => 
            type
                .GetMethods(bindingFlags)
                .Where(
                    info => Attribute.IsDefined(info, typeof(ConCommandAttribute))
                )
            );
        foreach (var method in allMethods) {
            var conComName = method.GetCustomAttribute<ConCommandAttribute>()!.Name;
            _commands.Add(conComName, method);
        }
        
        var allHandlers = assemblyTypes.SelectMany(type => 
            type
                .GetMethods(bindingFlags)
                .Where(
                    info => Attribute.IsDefined(info, typeof(ConTagHandlerAttribute))
                )
        );
        foreach (var method in allHandlers) {
            var tag = method.GetCustomAttribute<ConTagHandlerAttribute>()!.Tag;
            _tagHandlers.Add(tag, method);
        }
        
        var allProperties = assemblyTypes.SelectMany(type => 
            type
                .GetProperties(bindingFlags)
                .Where(
                    info => Attribute.IsDefined(info, typeof(ConVariableAttribute))
                )
        );
        foreach (var property in allProperties) {
            var tag = property.GetCustomAttribute<ConVariableAttribute>()!.Name;
            _convars.Add(tag, property);
        }
        
        Logger.LogTrace("Console commands successfully registered");
    }

    ///TODO: this will crash if something was registered under the same name
    [Obsolete("All commands are now registered automatically and refreshed on hot reload")]
    public static void Register(Type type) {
        var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        var methods = type.GetMethods(bindingFlags)
            .Where(info => Attribute.IsDefined(info, typeof(ConCommandAttribute)));
        foreach (var method in methods) {
            var conComName = method.GetCustomAttribute<ConCommandAttribute>()!.Name;
            _commands[conComName] =  method;
        }
        
        var properties = type.GetProperties(bindingFlags)
            .Where(info => Attribute.IsDefined(info, typeof(ConVariableAttribute)));
        foreach (var property in properties) {
            var conVarName = property.GetCustomAttribute<ConVariableAttribute>()!.Name;
            _convars[conVarName] = property;
        }
        
        var tagHandlers = type.GetMethods(bindingFlags)
            .Where(info => Attribute.IsDefined(info, typeof(ConTagHandlerAttribute)));
        foreach (var tagHandler in tagHandlers) {
            var contag = tagHandler.GetCustomAttribute<ConTagHandlerAttribute>()!.Tag;
            _tagHandlers[contag] = tagHandler;
        }
    }
    [Obsolete("All commands are now registered automatically and refreshed on hot reload")]
    public static void Register<T>() => Register(typeof(T));
    
    [GeneratedRegex(@"(?<!\\);")]
    private static partial Regex SubmitRegex();
}