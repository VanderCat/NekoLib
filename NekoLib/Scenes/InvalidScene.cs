using NekoLib.Core;

namespace NekoLib.Scenes;

internal class InvalidScene : IScene
{
    public string Name => "__INVALID__";
    public bool DestroyOnLoad => true;
    public int Index { get; set; }

    public List<GameObject> GameObjects { get; }

    public void Update()
    {
        throw new InvalidSceneException();
    }

    public void Draw()
    {
        throw new InvalidSceneException();
    }
    
    public void Initialize()
    {
        throw new InvalidSceneException();
    }

    public void Dispose() { }
}