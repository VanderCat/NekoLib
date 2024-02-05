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
    
}