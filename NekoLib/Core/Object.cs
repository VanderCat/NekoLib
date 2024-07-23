namespace NekoLib.Core; 

/// <summary>
/// Base object to derive from, provides name and Id for stuff
/// </summary>
public abstract class Object : IDisposable {
    /// <summary>
    /// Name of the object
    /// </summary>
    /// <remarks>this should be visible in hierarchy</remarks>
    public string Name = "GameObject";

    /// <summary>
    /// A randomly generated id of the object
    /// </summary>
    public Guid Id = Guid.NewGuid();

    public new string ToString() => Name;
    
    public virtual void Dispose() {
        
    }

    public static void Destroy(Object obj, float delay = 0f) {
        Timer.Global.After(delay, function => obj.Dispose());
    }
}