using System.Linq.Expressions;
using System.Reflection;

namespace NekoLib.Core; 

/// <summary>
/// Main building block of the logic inside GameObjects
/// </summary>
public abstract class Component : Object {
    
    /// <summary>
    /// GameObject this component part of
    /// </summary>
    public GameObject GameObject { get; internal set; }

    /// <summary>
    /// Tags of the gameObject this component part of
    /// </summary>
    public HashSet<string> Tags => GameObject.Tags;

    /// <summary>
    /// All tags from all parent tree this component part of
    /// </summary>
    public HashSet<string> AllTags => GameObject.AllTags;

    /// <summary>
    /// Alias for this.GameObject.Transform
    /// </summary>
    public Transform Transform => GameObject.Transform;

    /// <summary>
    /// Alias for <c>this.GameObject.Broadcast(string, object o)</c>
    /// </summary>
    public void Broadcast(string methodName, object? o = null) => GameObject.Broadcast(methodName, o);
    
    protected static readonly Dictionary<Type, Dictionary<string, Action<object, object?>>> MethodCache = new();
    
    /// <summary>
    /// Find and run Method inside this Component
    /// </summary>
    /// <param name="methodName">Name of the method to run</param>
    /// <param name="o">Addition argument to run</param>
    public virtual void Invoke(string methodName, object? o = null) {
        var type = GetType();
        if (!MethodCache.TryGetValue(type, out var methods))
            MethodCache[type] = methods = new Dictionary<string, Action<object, object?>>();

        if (!methods.TryGetValue(methodName, out var action))
        {
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (method == null)
                return;
            methods[methodName] = action = CompileMethod(method);
        }

        action(this, o);
    }

    protected static Action<object, object?> CompileMethod(MethodInfo method) {
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
    
    public string ToString() => $"{GetType().Name} of {GameObject.Name}";

    internal bool _awoke = false;
    internal bool _started = false;

    public void StartIfNeeded() {
        if (!_awoke || _started) return;
        Invoke("Start");
        _started = true;
    }

    public override void Dispose() {
        base.Dispose();
        GameObject._components.Remove(this);
        GameObject = null;
    }
}