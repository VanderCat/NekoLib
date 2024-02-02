using NekoLib.Core;

namespace NekoLib.Scenes;

public interface IScene
{
    public string Name { get; }
    public bool DestroyOnLoad { get; }
    public int Index { get; set; }

    public List<GameObject> GameObjects { get; }
    
    public void Initialize();
    
    public void Update();
    public void Draw();
}