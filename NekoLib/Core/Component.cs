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
    /// Tag of the component
    /// </summary>
    public string Tag = "";

    /// <summary>
    /// Alias for this.GameObject.Transform
    /// </summary>
    public Transform Transform => GameObject.Transform;

    /// <summary>
    /// Alias for <c>this.GameObject.Broadcast(string)</c>
    /// </summary>
    public void Broadcast(string methodName) => GameObject.Broadcast(methodName);
    
    /// <summary>
    /// Find and run Method inside this Component
    /// </summary>
    /// <param name="methodName">Name of the method to run</param>
    public void Invoke(string methodName) {
        var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        
        method?.Invoke(this, null);
    }
    
    public string ToString() => $"{nameof(Transform)} of {GameObject.Name}";
}