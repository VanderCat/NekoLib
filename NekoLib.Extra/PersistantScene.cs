namespace NekoLib.Extra; 

public class PersistantScene : BaseScene {
    public override string Name => "DontDestroyOnLoad";
    
    public override void Initialize() { }

    public PersistantScene() {
        DestroyOnLoad = false;
    }
}