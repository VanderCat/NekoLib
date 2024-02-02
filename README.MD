# NekoLib
A collection of library agnostic Unity-like stuff

> [!WARNING]  
> It is not finished and i doubt it ever will! (though i hope so)

## Features
1. Scene Management (implement serializing/etc. yourself)  
   just implement `IScene` and you created a scene! Next you can load it by using `SceneManagement.LoadScene(new Scene)`
2. GameObjects + Components (and by extension Transform using vanilla .NET)


## Example:
```csharp
using NekoLib.Core;
using NekiLib.Scenes;

class Scene : IScene {
    public string Name => "Name"
    public bool DestroyOnLoad { get; set; } = true;
    public int Index { get; set; }
   
   
    private List<GameObject> _gameObjects;
    public List<GameObject> GameObjects => _gameObjects;
    
    public void Initialize() {
        var gameObject = new GameObject();
        gameObject.AddComponent<TestComponent>();
        // Do stuff before first frame
    }
    
    public void Update() {
        // Do stuff every frame
    }
    public void Draw() {
        // Draw stuff on frame
    }
}

class TestComponent : Behaviour {
    void Awake() {
        Console.WriteLine("Just like in Unity!")
    }
}

static class Program {

    public static void Main() {
        var scene = new Scene();
        SceneManagement.LoadScene(scene);
    
        while (true) {
            SceneManagement.Update();
            SceneManagement.Draw();
        }
    }
}
```