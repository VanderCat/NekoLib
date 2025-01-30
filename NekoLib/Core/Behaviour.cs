using System.Reflection;

namespace NekoLib.Core; 

/// <summary>
/// A component that can be turned off
/// </summary>
public abstract class Behaviour : Component {
    public bool _enabled = true;

    /// <summary>
    /// Is this component enabled or not
    /// </summary>
    public bool Enabled {
        get => _enabled;
        set {
            _enabled = value;
            if (value) base.Invoke("OnEnabled");
            else base.Invoke("OnDisabled");
        }
    }

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
        if (IsActiveAndEnabled)
            base.Invoke(methodName, o);
    }
}