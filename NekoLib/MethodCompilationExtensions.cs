using System.Linq.Expressions;
using System.Reflection;
using NekoLib.Scenes;

namespace NekoLib;

public static class MethodCompilationExtensions {
    private static readonly Dictionary<Type, Dictionary<string, Action<object, object?>>> MethodCache = new();
    
    //this is as same as in component but separate because im too lazy to fix stuff cause by nonvirtual invoke in component duh
    public static void Invoke(this IScene @object, string name, object? o = null) {
        var type = @object.GetType();
        if (!MethodCache.TryGetValue(type, out var methods))
            MethodCache[type] = methods = new Dictionary<string, Action<object, object?>>();

        if (!methods.TryGetValue(name, out var action))
        {
            var method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (method == null)
                return;
            methods[name] = action = method.Compile();
        }

        action(@object, o);
    }
    
    internal static Action<object, object?> Compile(this MethodInfo method) {
        var instance = Expression.Parameter(typeof(object));
        var param = Expression.Parameter(typeof(object));
        var parameters = method.GetParameters();
        var arguments = parameters.Length == 0 ? null :
            Expression.Convert(param, parameters[0].ParameterType);

        var call = Expression.Call(
            Expression.Convert(instance, method.DeclaringType!),
            method,
            arguments != null ? [arguments] : null
        );

        return Expression.Lambda<Action<object, object?>>(call, instance, param).Compile();
    }
}