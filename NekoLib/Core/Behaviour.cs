using System.Reflection;

namespace NekoLib.Core; 

/// <summary>
/// A component that can be turned off
/// </summary>
public abstract class Behaviour : Component {
    /// <summary>
    /// Is this component enabled or not
    /// </summary>
    public bool Enabled = true;
    
    /// <summary>
    /// Is this component enabled AND parent GameObject is active too. If it is false the behaviour will not update.
    /// </summary>
    public bool IsActiveAndEnabled => GameObject.Active && Enabled;
    
    /// <summary>
    /// Find and run Method inside this Component
    /// </summary>
    /// <param name="methodName">Name of the method to run</param>
    /// <param name="o">Addition argument to run</param>
    public override void Invoke(string methodName, object? o = null) {
        var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (IsActiveAndEnabled)
            method?.Invoke(this, o is null ? null : new []{o});
    }
}