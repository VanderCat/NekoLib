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
    /// Tag of the gameObject this component part of
    /// </summary>
    public string Tag => GameObject.Tag;

    /// <summary>
    /// Alias for this.GameObject.Transform
    /// </summary>
    public Transform Transform => GameObject.Transform;

    /// <summary>
    /// Alias for <c>this.GameObject.Broadcast(string, object o)</c>
    /// </summary>
    public void Broadcast(string methodName, object? o = null) => GameObject.Broadcast(methodName, o);
    
    /// <summary>
    /// Find and run Method inside this Component
    /// </summary>
    /// <param name="methodName">Name of the method to run</param>
    /// <param name="o">Addition argument to run</param>
    public virtual void Invoke(string methodName, object? o = null) {
        var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        
        method?.Invoke(this, o is null ? null : new []{o});
    }
    
    public string ToString() => $"{GetType().Name} of {GameObject.Name}";

    internal bool _awoke = false;
    internal bool _started = false;

    public void StartIfNeeded() {
        if (!_awoke || _started) return;
        Invoke("Start");
        _started = true;
    }
}