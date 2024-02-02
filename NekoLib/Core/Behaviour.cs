namespace NekoLib.Core; 

public abstract class Behaviour : Component {
    public bool Enabled = true;
    
    public bool IsActiveAndEnabled => GameObject.Active && Enabled;
    
}