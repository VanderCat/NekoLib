# NekoLib
A collection of library agnostic gamedev-like stuff

> [!WARNING]  
> Beware of bugs

## Features
1. Scene Management (implement serializing/etc. yourself)  
   just implement `IScene` and you created a scene! Next you can load it by using `SceneManagement.LoadScene(new Scene)`
2. Unity like GameObjects + Components
3. Transform and Object graph
4. Extendable Virtual Filesystem


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