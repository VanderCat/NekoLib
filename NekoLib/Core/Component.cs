using System.Reflection;

namespace NekoLib.Core; 

public abstract class Component : Object {
    public GameObject GameObject { get; internal set; }

    public string Tag = "";

    public Transform Transform => GameObject.Transform;

    public void Broadcast(string methodName) => GameObject.Broadcast(methodName);
    
    public void Invoke(string methodName) {
        var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        
        method?.Invoke(this, null);
    }
    
    public string ToString() => $"{nameof(Transform)} of {GameObject.Name}";
}